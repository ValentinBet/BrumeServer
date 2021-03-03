﻿using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BrumeServer.ServerData;

namespace BrumeServer
{
    public class Room : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public string Name { get; set; }
        public ushort MaxPlayers { get; set; }
        public Player Host { get; set; }
        public Dictionary<IClient, Player> Players = new Dictionary<IClient, Player>();
        public BrumeServer brumeServerRef;
        public RoomTimers Timers;
        public RoomAltars Altars;
        public ChampSelect champSelect;
        // InGame >>
        public Dictionary<Team, ushort> Scores = new Dictionary<Team, ushort>();
        public Dictionary<ushort, ushort> InGameUniqueIDList = new Dictionary<ushort, ushort>();
        public Dictionary<Team, ushort> assignedSpawn = new Dictionary<Team, ushort>();
        public Dictionary<ushort, Interactible> InCaptureInteractible = new Dictionary<ushort, Interactible>();
        // <<

        public bool IsStarted = false;
        public bool GameInit = false;


        public Room(BrumeServer brumeServer, ushort ID, string name, Player host, IClient hostClient, ushort maxPlayers = 6)
        {
            this.brumeServerRef = brumeServer;

            Timers = new RoomTimers(this);
            Altars = new RoomAltars(this);
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
            InGameUniqueIDList.Clear();

            foreach (Player p in Players.Values)
            {
                p.ultStacks = 0;
            }

            assignedSpawn.Clear();
            Scores.Add(Team.blue, 0);
            Scores.Add(Team.red, 0);
            InCaptureInteractible.Clear();
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

        public Player GetPlayerByID(ushort ID)
        {
            return Players.Single(x => x.Key.ID == ID).Value;
        }
        public IClient GetPlayerClientByID(ushort ID)
        {
            return Players.Single(x => x.Key.ID == ID).Key;
        }

        public Dictionary<IClient, Player> GetPlayerListInTeam(Team team)
        {
            Dictionary<IClient, Player> _temp = new Dictionary<IClient, Player>();

            foreach (KeyValuePair<IClient, Player> p in Players)
            {
                if (p.Value.playerTeam == team)
                {
                    _temp.Add(p.Key, p.Value);
                }
            }

            return _temp;
        }


        internal ushort? GetPlayerCharacterInTeam(Team team, Character character)
        {
            Player _tempPlayer = Players.Values.Where(x => x.playerTeam == team && x.playerCharacter == character).FirstOrDefault();

            if (_tempPlayer != null)
            {
                return _tempPlayer.ID;
            }

            return null;
        }


        public void StartGame()
        {
            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsReady = false;
            }
            // InitializeUltimateDic();
            SetAndSendInGameUniqueIDs();
            SetSpawnAssignement();

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(assignedSpawn[Team.red]);
                Writer.Write(assignedSpawn[Team.blue]);

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

            // Tout les joueurs sont prets à jouer

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

        public void SendState(object sender, MessageReceivedEventArgs e, ushort _state)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);
                writer.Write(_state);

                using (Message Message = Message.Create(Tags.StateUpdate, writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        if (e.Client == client.Key) { continue; }
                        client.Key.SendMessage(Message, e.SendMode);
                    }
                }
            }
        }



        internal void SendForcedMovemment(object sender, MessageReceivedEventArgs e, sbyte newXDirection, sbyte newZDirection, ushort newDuration, ushort newStrength, ushort targetId)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(newXDirection);
                writer.Write(newZDirection);
                writer.Write(newDuration);
                writer.Write(newStrength);
                writer.Write(targetId);

                using (Message Message = Message.Create(Tags.AddForcedMovement, writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        if (client.Key.ID == targetId) { client.Key.SendMessage(Message, e.SendMode); return; }
                    }
                }
            }
        }

        public void SendStatus(object sender, MessageReceivedEventArgs e, ushort _statusToSend, ushort playerTargeted)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);
                writer.Write(_statusToSend);
                writer.Write(playerTargeted);

                using (Message Message = Message.Create(Tags.AddStatus, writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, e.SendMode);
                    }
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

        public void StopGame(ushort team = (ushort)Team.none)
        {
            if (!GameInit)
            {
                Log.Message("NEW GAME ASKED IN A NONE INIT GAME", MessageType.Error);
                return;
            }

            ResetGameData();

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsInGameScene = false;
                player.Value.playerCharacter = Character.none;
            }

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(team);
                using (Message Message = Message.Create(Tags.StopGame, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        internal void StartRoundFinalPhase(ushort altarID)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(altarID);

                using (Message Message = Message.Create(Tags.RoundFinalPhase, Writer))
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
            if (!GameInit)
            {
                Log.Message("NEW ROUND ASKED MULTIPLE TIMES", MessageType.Error);
                return;
            }

            Timers.StopTimersInstantly();
            InCaptureInteractible.Clear();

            foreach (KeyValuePair<IClient, Player> player in Players)
            {
                player.Value.IsInGameScene = false;
            }

            if (Altars.canUnlockMoreAltars)
            {
                Addpoints(team, 1);
            } else
            {
                Addpoints(team, 2);
            }

            if (Scores[(Team)team] >= brumeServerRef.gameData.RoundToWin)
            {
                StopGame(team);
                return;
            }

            Altars.ResetAltars();
            assignedSpawn.Clear();
            SetSpawnAssignement();

            GameInit = false;

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                Writer.Write(team);
                Writer.Write(assignedSpawn[Team.red]);
                Writer.Write(assignedSpawn[Team.blue]);

                using (Message Message = Message.Create(Tags.NewRound, Writer))
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
        public void StartGameTimer()
        {
            StartTimer();
            Timers.StartNewSoulTimer(Factory.GenerateRandomNumber(brumeServerRef.gameData.BrumeSoulRespawnMinTime, brumeServerRef.gameData.BrumeSoulRespawnMaxTime));
            Timers.StartGameInitTimer(brumeServerRef.gameData.GameInitTime, brumeServerRef.gameData.SpawnWallTime);
            Timers.StartNewGameStopWatch();
        }

        internal void GameInitTimerElapsed()
        {
            AltarTimerElapsed();
           //  UnlockAllVisionTowers(); DEPRECATED
        }
        internal void WallTimerElapsed()
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.DynamicWallState, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        internal void StartNewFrogTimer(ushort frogID)
        {
            Timers.StartNewFrogTimer(frogID, brumeServerRef.gameData.FrogRespawnTime);
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

        internal void StartHealthPackTimer(ushort healthPackID)
        {
            Timers.StartNewHealthPackTimer(healthPackID, brumeServerRef.gameData.healthPackRespawnTime);
        }

        internal void HealthPackTimerElapsed(ushort healthPackID)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room 

                Writer.Write(healthPackID);

                using (Message Message = Message.Create(Tags.HealthPackTimerElapsed, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        internal void StartUltPickupTimer(ushort interID)
        {
            Timers.StartNewUltPickupTimer(interID, brumeServerRef.gameData.UltPickupRespawnTime);
        }
        internal void UltPickupTimerElapsed(ushort interID)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room 

                Writer.Write(interID);

                using (Message Message = Message.Create(Tags.UnlockInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }


        internal void StartNewVisionTowerTimer(ushort iD)
        {
            Timers.StartNewVisionTowerTimer(iD, brumeServerRef.gameData.VisionTowerReactivateTime);
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

        public void AltarCaptured(Team team, ushort altarID)
        {
            Altars.CaptureAltar(team, altarID);

            if (Altars.canUnlockMoreAltars)
            {
                Timers.StartNewAltarTimer(brumeServerRef.gameData.AltarLockTime);
            }

        }

        public void AltarTimerElapsed()
        {
            ushort chosenAltar = Altars.GetRandomFreeAltar();
            Altars.ChooseAltar(chosenAltar);

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                Writer.Write(chosenAltar);

                using (Message Message = Message.Create(Tags.UnlockInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        internal void SoulTimerElapsed()
        {
            ushort chosenBrume = (ushort)Factory.GenerateRandomNumber(0, brumeServerRef.gameData.BrumeCount);
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                Writer.Write(chosenBrume);

                using (Message Message = Message.Create(Tags.BrumeSoulSpawnCall, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }

            Timers.StartNewSoulTimer(Factory.GenerateRandomNumber(brumeServerRef.gameData.BrumeSoulRespawnMinTime, brumeServerRef.gameData.BrumeSoulRespawnMaxTime));
        }

        #endregion

        #region Interactible

        public void TryCaptureNewInteractible(ushort _interID, ushort team, System.Numerics.Vector3 pos, ushort iD, InteractibleType type)
        {
            if (InCaptureInteractible.ContainsKey(_interID))
            {
                InCaptureInteractible[_interID].AddPlayerInZone(GetPlayerByID(iD));
            }
            else
            {
                Interactible _inter = new Interactible(_interID, pos, (Team)team, GetPlayerByID(iD), type, this);
                InCaptureInteractible.Add(_interID, _inter);
            }
        }

        public void QuitInteractibleZone(ushort _interID, ushort iD)
        {
            if (InCaptureInteractible.ContainsKey(_interID))
            {
                InCaptureInteractible[_interID].RemovePlayerInZone(GetPlayerByID(iD));
            }
        }

        public void RemoveInteractible(ushort _interID)
        {
            if (InCaptureInteractible.ContainsKey(_interID))
            {
                InCaptureInteractible.Remove(_interID);
            }
        }

        internal void InteractibleCaptureProgress(ushort _interID, float progress)
        {
            if (InCaptureInteractible.ContainsKey(_interID))
            {
                InCaptureInteractible[_interID].CaptureProgress(progress);
            }
        }

        private void UnlockAllVisionTowers()
        {
            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write((ushort)InteractibleType.VisionTower);

                using (Message Message = Message.Create(Tags.UnlockAllInteractibleOfType, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        internal void EndZoneCaptured(Team team)
        {
            NewRound((ushort)team);
        }

        #endregion

        #region ChampSelect
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
            else if ((Players[Iclient].playerCharacter != Character.none) && (character != Players[Iclient].playerCharacter))// Sinon demande de swap
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    ushort targetID = (ushort)GetPlayerCharacterInTeam(Players[Iclient].playerTeam, character);

                    Writer.Write(Iclient.ID);
                    Writer.Write(targetID);
                    Writer.Write((ushort)Players[Iclient].playerCharacter);

                    using (Message Message = Message.Create(Tags.AskForCharacterSwap, Writer))
                    {
                        foreach (KeyValuePair<IClient, Player> client in Players.Where(x => x.Value.playerTeam == Players[Iclient].playerTeam))
                        {
                            client.Key.SendMessage(Message, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        internal void CharacterSwap(ushort askingPlyr, ushort targetedPlyr) // Echange les personnages de 2 joueurs
        {
            Character _temp = GetPlayerByID(askingPlyr).playerCharacter;
            champSelect.ForcePickChamp(GetPlayerByID(targetedPlyr).playerCharacter, GetPlayerByID(askingPlyr));
            champSelect.ForcePickChamp(_temp, GetPlayerByID(targetedPlyr));

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(targetedPlyr);
                Writer.Write(askingPlyr);
                Writer.Write((ushort)GetPlayerByID(targetedPlyr).playerCharacter);
                Writer.Write((ushort)GetPlayerByID(askingPlyr).playerCharacter);

                using (Message Message = Message.Create(Tags.CharacterSwap, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        internal void RefuseCharacterSwap(IClient targetClient)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.RefuseCharacterSwap, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players.Where(x => x.Value.playerTeam == Players[targetClient].playerTeam))
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        #endregion

        #region Assignement
        internal void SetAndSendInGameUniqueIDs()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                ushort uniqueID = 0;
                List<ushort> playerID = new List<ushort>();

                foreach (KeyValuePair<IClient, Player> player in Players)
                {
                    playerID.Add(player.Key.ID);

                    uniqueID++;
                    InGameUniqueIDList.Add(player.Key.ID, uniqueID);
                }

                writer.Write(playerID.ToArray());

                using (Message message = Message.Create(Tags.SetInGameUniqueID, writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                        client.Key.SendMessage(message, SendMode.Reliable);
                }

            }
        }
        private void SetSpawnAssignement()
        {
            Random r = new Random();

            ushort redTeamAssignement = (ushort)Factory.GenerateRandomNumber(1, 4);
            ushort blueTeamAssignement = (ushort)Factory.GenerateRandomNumber(1, 4);

            while (blueTeamAssignement == redTeamAssignement) //pas propre
            {
                blueTeamAssignement = (ushort)Factory.GenerateRandomNumber(1, 4);
            }

            assignedSpawn.Add(Team.red, redTeamAssignement);
            assignedSpawn.Add(Team.blue, blueTeamAssignement);
        }
        #endregion

        #region Ultimate
        public void AddUltimateStacks(ushort id, ushort value)
        {

            Player _p = GetPlayerByID(id);
                if (_p.ultStacks + value > brumeServerRef.gameData.ChampMaxUltStacks[_p.playerCharacter])
                {
                _p.ultStacks = brumeServerRef.gameData.ChampMaxUltStacks[_p.playerCharacter];
                }
                else
                {
                _p.ultStacks += value;
                }

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(id);
                writer.Write(_p.ultStacks);

                using (Message message = Message.Create(Tags.AddUltimatePoint, writer)) // SET
                {
                    foreach (KeyValuePair<IClient, Player> client in Players.Where(x => x.Value.playerTeam == _p.playerTeam))
                        client.Key.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        internal void UseUltimateStacks(ushort iD, ushort value)
        {
            Player _p = GetPlayerByID(iD);

            if ((int)_p.ultStacks - (int)value <= 0)
            {
                _p.ultStacks = 0;
            } else
            {
                _p.ultStacks -= value;
            }

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(iD);
                writer.Write(_p.ultStacks);

                using (Message message = Message.Create(Tags.AddUltimatePoint, writer)) // CAUSE ADD = SET
                {
                    foreach (KeyValuePair<IClient, Player> client in Players.Where(x => x.Value.playerTeam == GetPlayerByID(iD).playerTeam))
                        client.Key.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        internal void AddUltimateStackToAllTeam(Team team, ushort value)
        {
            foreach (KeyValuePair<IClient,Player> p in Players.Where(x => x.Value.playerTeam == team))
            {
                Player _p = GetPlayerByID(p.Key.ID);

                if (brumeServerRef.gameData.ChampMaxUltStacks[_p.playerCharacter] < _p.ultStacks + value)
                {
                    _p.ultStacks = brumeServerRef.gameData.ChampMaxUltStacks[_p.playerCharacter];
                }
                else
                {
                    _p.ultStacks += value;
                }
            }

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write((ushort)team);
                writer.Write(value);

                using (Message message = Message.Create(Tags.AddUltimatePointToAllTeam, writer)) // ADD
                {
                    Log.Message(team.ToString());
                    foreach (KeyValuePair<IClient, Player> client in Players.Where(x => x.Value.playerTeam == team))
                    {
                        client.Key.SendMessage(message, SendMode.Reliable);
                    }

                }
            }

        }
        #endregion

        #region Chat
        public void NewServerChatMessage(string _message)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write((ushort)0);
                Writer.Write(_message);
                Writer.Write(true);

                using (Message Message = Message.Create(Tags.NewChatMessage, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }
        #endregion
    }
}
