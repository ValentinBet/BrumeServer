﻿using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BrumeServer.GameData;

namespace BrumeServer
{
    public class Room : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public string Name { get; set; }
        public ushort MaxPlayers { get; set; }
        public Player Host { get; set; }
        public Dictionary<IClient, Player> Players = new Dictionary<IClient, Player>();

        public RoomTimers Timers;
        public RoomAltars Altars;
        public ChampSelect champSelect;
        // InGame >>
        public Dictionary<Team, ushort> Scores = new Dictionary<Team, ushort>();
        // <<

        public bool IsStarted = false;
        public bool GameInit = false;

        public Room(ushort ID, string name, Player host, IClient hostClient, ushort maxPlayers = 6)
        {
            Timers = new RoomTimers(this);
            Altars = new RoomAltars();
            champSelect = new ChampSelect();

            this.ID = ID;
            this.Name = name;
            this.Host = host;
            this.MaxPlayers = maxPlayers;

            Players.Add(hostClient, host);
            Scores.Add(Team.blue, 0);
            Scores.Add(Team.red, 0);
        }

        public Room() { }

        internal void Destroy()
        {
            Timers.StopTimersInstantly(true);
        }

        public void ResetGameData()
        {
            GameInit = false;
            Altars.ResetAltars();
            Timers.StopTimersInstantly();
            champSelect.ResetData();
            Scores.Clear();

            Scores.Add(Team.blue, 0);
            Scores.Add(Team.red, 0);
        }


        public Team GetTeamWithLowestPlayerAmount()
        {
            if (GetPlayerAmountInCertainTeam(Team.red) > GetPlayerAmountInCertainTeam(Team.blue))
            {
                return Team.blue;
            }
            else
            {
                return Team.blue;
            }
        }

        public int GetPlayerAmountInCertainTeam(Team team)
        {
            int _count = 0;

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                if (player.Value.playerTeam == team)
                {
                    _count++;
                }
            }

            return _count;
        }


        public void Deserialize(DeserializeEvent e)
        {
            this.ID = e.Reader.ReadUInt16();
            this.Name = e.Reader.ReadString();
            this.MaxPlayers = e.Reader.ReadUInt16();
            this.Scores[Team.blue] = e.Reader.ReadUInt16();
            this.Scores[Team.red] = e.Reader.ReadUInt16();
            IsStarted = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Name);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(Scores[Team.blue]);
            e.Writer.Write(Scores[Team.red]);
            e.Writer.Write(IsStarted);
            e.Writer.Write((ushort)Players.Count); // LocalOnly
        }

        public Player FindPlayerByID(ushort ID)
        {
            return Players.Single(x => x.Key.ID == ID).Value;
        }

        public void StartGame()
        {

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsReady = false;
            }

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StartGame, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
            Log.Message("Game Started in Room : " + ID);
        }


        public void PlayerJoinGameScene() // Un joueur rejoint la scene de jeu
        {
            if (GameInit)
                return;

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                if (!(player.Value.IsInGameScene))
                {
                    return;
                }
            }

            // Tout les joueurs sont prets

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.AllPlayerJoinGameScene, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
            GameInit = true;

            champSelect.ResetData();
            StartGameTimer();
            StartAltarTimer();
        }

        public void SpawnObjPlayer(ushort ID, bool resurect = false)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                GameWriter.Write(ID);
                GameWriter.Write(resurect);

                using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }

            ////Spawn Old Players
            //using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            //{
            //    foreach (KeyValuePair<IClient, Player> client in Players)
            //    {
            //        if (e.Client == client.Key) { continue; }

            //        ushort ID = client.Key.ID;
            //        GameWriter.Write(ID);

            //        using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
            //        {
            //            e.Client.SendMessage(Message, SendMode.Reliable);
            //        }
            //    }
            //}
        }

        public void SupprPlayerObj(ushort ID)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                GameWriter.Write(ID);

                using (Message Message = Message.Create(Tags.SupprObjPlayer, GameWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        public void SendMovement(object sender, MessageReceivedEventArgs e, float posX, float posZ, float rotaY)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(e.Client.ID);

                    writer.Write(posX);
                    writer.Write(posZ);

                    writer.Write(rotaY);

                    message.Serialize(writer);
                }

                foreach (KeyValuePair<IClient, Player> client in Players)
                {
                    if (e.Client == client.Key) { continue; }
                    client.Key.SendMessage(message, e.SendMode);
                }
            }
        }

        public void SendState(object sender, MessageReceivedEventArgs e, uint _state)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(e.Client.ID);

                    writer.Write(_state);

                    message.Serialize(writer);
                }

                foreach (KeyValuePair<IClient, Player> client in Players)
                {
                    if (e.Client == client.Key) { continue; }
                    client.Key.SendMessage(message, e.SendMode);
                }
            }
        }

        public void StartTimer() // Timer local des joueurs
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StartTimer, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }

                }
            }
        }

        public void StopGame()
        {
            ResetGameData();
                          
            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsInGameScene = false;
            }

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StopGame, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        internal void NewRound(ushort team)
        {
            Timers.StopTimersInstantly();
            GameInit = false;

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsInGameScene = false;
            }

            if (Scores[(Team)team] == GameData.RoundToWin - 1)
            {
                StopGame();
                return;
            }

            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write(team);

                using (Message Message = Message.Create(Tags.NewRound, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        public void Addpoints(ushort targetTeam, ushort value)
        {
            Scores[(Team)targetTeam] += value;

            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write(targetTeam);
                TeamWriter.Write(value);

                using (Message Message = Message.Create(Tags.AddPoints, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        internal bool IsAllPlayersReady()
        {
            foreach (Player p in Players.Values)
            {
                if (!p.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        #region Timers
        internal void StartNewFrogTimer(ushort frogID)
        {
            Timers.StartNewFrogTimer(frogID, GameData.FrogRespawnTime);
        }

        internal void FrogTimerElapsed(ushort frogID)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room 

                Writer.Write(frogID);

                using (Message Message = Message.Create(Tags.FrogTimerElapsed, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        internal void StartNewVisionTowerTimer(ushort iD)
        {
            Timers.StartNewVisionTowerTimer(iD, GameData.VisionTowerReactivateTime);
        }

        internal void VisionTowerTimerElapsed(ushort ID)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room 

                Writer.Write(ID);

                using (Message Message = Message.Create(Tags.VisionTowerTimerElapsed, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        public void TimerCreated() // Call in RoomTimers
        {
            Log.Message("BrumeServer - RoomTimers generated for Room : " + ID);
        }

        public void StartAltarTimer()
        {
            Timers.StartNewAltarTimer(GameData.AltarLockTime);
        }

        public void AltarTimerElapsed()
        {
            ushort chosenAltar = Altars.GetRandomFreeAltar();
            Altars.ChooseAltar(chosenAltar);
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                Writer.Write(chosenAltar);

                using (Message Message = Message.Create(Tags.UnlockInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        public void StartGameTimer()
        {
            StartTimer();
            Timers.StartNewGameStopWatch();
        }

        #endregion

        internal void TryPickCharacter(Character character, IClient Iclient)
        {
            if (champSelect.TryPickChamp(Players[Iclient].playerTeam, Players[Iclient], character)) // Si slot character libre
            {
                Players[Iclient].playerCharacter = character;
                Players[Iclient].IsReady = true;

                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(Iclient.ID);
                    Writer.Write((ushort)character);

                    using (Message Message = Message.Create(Tags.SetCharacter, Writer))
                    {
                        foreach (KeyValuePair<IClient, Player> client in Players)
                            client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
            else
            {
                return;
            }

        }

    }
}