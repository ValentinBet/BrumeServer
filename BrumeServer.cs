﻿using DarkRift.Server;
using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.ServerData;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Numerics;

namespace BrumeServer
{
    public class BrumeServer : Plugin
    {
#pragma warning disable CS0618 // obsolete (WRITE EVENT)

        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 2);

        public GameData gameData = new GameData();

        public Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();
        public Dictionary<ushort, Room> rooms = new Dictionary<ushort, Room>();

        private ushort lastRoomID = 0;
        private NetworkAnimationManager networkAnimationManager = new NetworkAnimationManager();
        private NetworkObjectsManager networkObjectsManager = new NetworkObjectsManager();
        private NetworkInteractibleManager networkInteractibleManager = new NetworkInteractibleManager();
        private NetworkSpellManager networkSpellManager = new NetworkSpellManager();
        public BrumeServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {

            gameData.Init();

            networkAnimationManager.brumeServer = this;
            networkObjectsManager.brumeServer = this;
            networkInteractibleManager.brumeServer = this;
            networkSpellManager.brumeServer = this;

            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Server<-->Client
        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Player newPlayer = new Player(
                e.Client.ID,
                false,
                "Null");

            players.Add(e.Client, newPlayer);

            using (DarkRiftWriter NewPlayerWriter = DarkRiftWriter.Create())
            {
                NewPlayerWriter.Write(newPlayer);

                using (Message Message = Message.Create(Tags.PlayerConnected, NewPlayerWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

            SendAllRooms(sender, e);

            e.Client.MessageReceived += networkAnimationManager.MessageReceivedFromClient;
            e.Client.MessageReceived += networkObjectsManager.MessageReceivedFromClient;
            e.Client.MessageReceived += networkInteractibleManager.MessageReceivedFromClient;
            e.Client.MessageReceived += networkSpellManager.MessageReceivedFromClient;
            e.Client.MessageReceived += MessageReceivedFromClient;
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Player _roomPlayer = players[e.Client];

            if (_roomPlayer.Room != null)
            {
                QuitRoom(e.Client, rooms[_roomPlayer.Room.ID]);

            }
            players.Remove(e.Client);

            e.Client.MessageReceived -= networkAnimationManager.MessageReceivedFromClient;
            e.Client.MessageReceived -= networkObjectsManager.MessageReceivedFromClient;
            e.Client.MessageReceived -= networkInteractibleManager.MessageReceivedFromClient;
            e.Client.MessageReceived -= networkSpellManager.MessageReceivedFromClient;
            e.Client.MessageReceived -= MessageReceivedFromClient;
        }


        private void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.IsPingMessage)
                {
                    Ping(sender, e);
                }
                if (message.Tag == Tags.CreateRoom)
                {
                    CreateRoom(sender, e);
                }
                if (message.Tag == Tags.SendAllRooms)
                {
                    SendAllRooms(sender, e);
                }
                else if (message.Tag == Tags.JoinRoom)
                {
                    JoinRoom(sender, e);
                }
                else if (message.Tag == Tags.QuitRoom)
                {
                    QuitRoomReceiver(sender, e);
                }
                else if (message.Tag == Tags.LobbyStartGame)
                {
                    LobbyStartGame(sender, e);
                }
                else if (message.Tag == Tags.StartPrivateRoom)
                {
                    StartPrivateRoom(sender, e);
                }
                else if (message.Tag == Tags.ConvertPrivateRoomType)
                {
                    ConvertPrivateRoomType(sender, e);
                }
                else if (message.Tag == Tags.SetPrivateRoomCharacter)
                {
                    SetTrainingCharacter(sender, e);
                }
                else if (message.Tag == Tags.ChangeName)
                {
                    ChangeName(sender, e);
                }
                else if (message.Tag == Tags.ChangeTeam)
                {
                    ChangeTeam(sender, e);
                }
                else if (message.Tag == Tags.SetReady)
                {
                    SetReady(sender, e);
                }
                else if (message.Tag == Tags.SetCharacter)
                {
                    SelectCharacter(sender, e);
                }
                else if (message.Tag == Tags.CharacterSwap)
                {
                    CharacterSwap(sender, e);
                }
                else if (message.Tag == Tags.RefuseCharacterSwap)
                {
                    RefuseCharacterSwap(sender, e);
                }
                else if (message.Tag == Tags.StartGame)
                {
                    StartGame(sender, e);
                }
                else if (message.Tag == Tags.SpawnObjPlayer)
                {
                    SpawnObjPlayer(sender, e);
                }
                else if (message.Tag == Tags.MovePlayerTag)
                {
                    SendPlayerMovement(sender, e);
                }
                else if (message.Tag == Tags.RotaPlayer)
                {
                    SendPlayerRota(sender, e);
                }
                else if (message.Tag == Tags.PlayerJoinGameScene)
                {
                    PlayerJoinGameScene(sender, e);
                }
                else if (message.Tag == Tags.AddPoints)
                {
                    AddPointsReceiver(sender, e);
                }
                else if (message.Tag == Tags.Damages)
                {
                    TakeDamages(sender, e);
                }
                else if (message.Tag == Tags.Heal)
                {
                    Heal(sender, e);
                }
                else if (message.Tag == Tags.AddHealth)
                {
                    AddHealPoint(sender, e);
                }
                else if (message.Tag == Tags.KillCharacter)
                {
                    KillCharacter(sender, e);
                }
                else if (message.Tag == Tags.StateUpdate)
                {
                    SendPlayerState(sender, e);
                }
                else if (message.Tag == Tags.AddForcedMovement)
                {
                    SendForcedMovement(sender, e);
                }
                else if (message.Tag == Tags.AddStatus)
                {
                    SendNewStatus(sender, e);
                }
                else if (message.Tag == Tags.BrumeSoulPicked)
                {
                    BrumeSoulPicked(sender, e);
                }
                else if (message.Tag == Tags.AddUltimatePoint)
                {
                    AddUltimatePoint(sender, e);
                }
                else if (message.Tag == Tags.UseUltPoint)
                {
                    UseUltPoint(sender, e);
                }
                else if (message.Tag == Tags.AddUltimatePointToAllTeam)
                {
                    AddUltimatePointToAllTeam(sender, e);
                }
                else if (message.Tag == Tags.AskForStopGame)
                {
                    AskForStopGame(sender, e);
                }
                else if (message.Tag == Tags.NewChatMessage)
                {
                    NewChatMessage(sender, e);
                }
                else if (message.Tag == Tags.WXSonarState)
                {
                    OnSonarState(sender, e);
                }
                else if (message.Tag == Tags.AskSkipToNextRound)
                {
                    AskSkipToNextRound(sender, e);
                }
            }
        }


        private void AskSkipToNextRound(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    rooms[players[e.Client].Room.ID].NewPlayerWantToSkipEndStats(e.Client.ID);
                }
            }
        }

        private void Ping(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    using (Message acknowledgementMessage = Message.Create(Tags.Ping, Writer))
                    {
                        acknowledgementMessage.MakePingAcknowledgementMessage(message);

                        e.Client.SendMessage(acknowledgementMessage, e.SendMode);
                    }
                }
            }
        }

        #endregion

        #region Player

        private void KillCharacter(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _killerID = reader.ReadUInt16();
                    Character _character = (Character)reader.ReadUInt16();

                    // -------
                    ushort _roomID = players[e.Client].Room.ID;

                    // rooms[_roomID].AddUltimateStacks(_killerID, 1);
                    rooms[_roomID].KillPlayer(_killerID, _character, e.Client);


                }
            }
        }


        private void TakeDamages(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _targetID = reader.ReadUInt16();
                    ushort _damages = reader.ReadUInt16();

                    rooms[players[e.Client].Room.ID].PlayerTakeDamages(_targetID, _damages, e.Client);


                }
            }
        }


        private void Heal(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _targetID = reader.ReadUInt16();
                    ushort _healValue = reader.ReadUInt16();

                    rooms[players[e.Client].Room.ID].PlayerHealed(_targetID, _healValue, e.Client);



                }
            }
        }

        private void AddHealPoint(object sender, MessageReceivedEventArgs e)
        {
            //using (Message message = e.GetMessage() as Message)
            //{
            //    using (DarkRiftReader reader = message.GetReader())
            //    {
            //        ushort _healPoint = reader.ReadUInt16();

            //        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            //        {
            //            Writer.Write(_healPoint);

            //            using (Message Message = Message.Create(Tags.AddHealth, Writer))
            //            {
            //                foreach (KeyValuePair<IClient, Player> client in rooms[players[e.Client].Room.ID].Players)
            //                {
            //                    if (client.Key != e.Client)
            //                    {
            //                        client.Key.SendMessage(Message, SendMode.Reliable);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }


        private void ChangeName(object sender, MessageReceivedEventArgs e)
        {
            string _name = "";


            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _name = reader.ReadString();
                }

                players[e.Client].Name = _name;

            }

            using (DarkRiftWriter NameWriter = DarkRiftWriter.Create())
            {
                NameWriter.Write(_name);

                using (Message Message = Message.Create(Tags.ChangeName, NameWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

            WriteEvent("Player : " + e.Client.ID + " change his name to --> " + _name, LogType.Info);

        }


        private void ChangeTeam(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    players[e.Client].playerTeam = (Team)reader.ReadUInt16();
                }
            }

            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write(players[e.Client].ID);
                TeamWriter.Write((ushort)players[e.Client].playerTeam);

                using (Message Message = Message.Create(Tags.ChangeTeam, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in rooms[players[e.Client].Room.ID].Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }

            WriteEvent("Player : " + e.Client.ID + " change his Team to --> " + players[e.Client].playerTeam, LogType.Info);

        }

        private void SetReady(object sender, MessageReceivedEventArgs e)
        {
            bool value = false;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    value = reader.ReadBoolean();
                }
            }

            players[e.Client].IsReady = value;

            if (players[e.Client].Room != null)
            {
                using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
                {
                    // Recu par les joueurs déja présent dans la room

                    TeamWriter.Write(players[e.Client].ID);
                    TeamWriter.Write(value);

                    using (Message Message = Message.Create(Tags.SetReady, TeamWriter))
                    {
                        foreach (KeyValuePair<IClient, Player> client in rooms[players[e.Client].Room.ID].Players)
                            client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        private void AddPointsReceiver(object sender, MessageReceivedEventArgs e)
        {
            ushort value;
            ushort targetTeam;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    targetTeam = reader.ReadUInt16();
                    value = reader.ReadUInt16();
                }
            }
            AddPoints(players[e.Client].Room.ID, targetTeam, value);
        }

        private void AddPoints(ushort roomID, ushort targetTeam, ushort value)
        {
            rooms[roomID].Addpoints(targetTeam, value);
        }

        private void AddUltimatePoint(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort value = reader.ReadUInt16();

                    // rooms[players[e.Client].Room.ID].AddUltimateStacks(e.Client.ID, value);
                }
            }
        }

        private void UseUltPoint(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort value = reader.ReadUInt16();

                    rooms[players[e.Client].Room.ID].UseUltimateStacks(e.Client.ID, value);
                }
            }
        }


        private void AddUltimatePointToAllTeam(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Team team = (Team)reader.ReadUInt16();
                    ushort value = reader.ReadUInt16();
                    // rooms[players[e.Client].Room.ID].AddUltimateStackToAllTeam(team, value);
                }
            }
        }


        #endregion

        #region Room

        private void SendAllRooms(object sender, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter SendAllRoomsWriter = DarkRiftWriter.Create())
            {
                SendAllRoomsWriter.Write(rooms.Where(x => x.Value.roomType == RoomType.Classic).Count());

                foreach (KeyValuePair<ushort, Room> r in rooms.Where(x => x.Value.roomType == RoomType.Classic))
                {
                    SendAllRoomsWriter.Write(r.Value);
                }

                using (Message Message = Message.Create(Tags.SendAllRooms, SendAllRoomsWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        private void SendAllRooms(object sender, MessageReceivedEventArgs e)
        {
            using (DarkRiftWriter SendAllRoomsWriter = DarkRiftWriter.Create())
            {
                SendAllRoomsWriter.Write(rooms.Where(x => x.Value.roomType == RoomType.Classic).Count()); ;

                foreach (KeyValuePair<ushort, Room> r in rooms.Where(x => x.Value.roomType == RoomType.Classic))
                {
                    SendAllRoomsWriter.Write(r.Value);
                }

                using (Message Message = Message.Create(Tags.SendAllRooms, SendAllRoomsWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        private void CreateRoom(object sender, MessageReceivedEventArgs e)
        {
            lastRoomID += 1;

            Room newRoom = new Room(
            this,
            lastRoomID,
            players[e.Client].Name + "'s room",
            players[e.Client],
            e.Client
            );

            players[e.Client].IsHost = true;
            players[e.Client].Room = newRoom;
            players[e.Client].playerTeam = Team.red;

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Nouvelle room crée recu par tout les joueurs

                RoomWriter.Write(newRoom);
                RoomWriter.Write(newRoom.Host.ID);

                using (Message Message = Message.Create(Tags.CreateRoom, RoomWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }

            rooms.Add(lastRoomID, newRoom);

            WriteEvent(players[e.Client].ID + " - " + players[e.Client].Name + " created Room : " + lastRoomID + " as team --> " + players[e.Client].playerTeam, LogType.Info);

            using (DarkRiftWriter ClientRoomWriter = DarkRiftWriter.Create())
            {

                // Recu par le créateur de la room

                ClientRoomWriter.Write(newRoom);
                ClientRoomWriter.Write(newRoom.Host.ID);
                ClientRoomWriter.Write((ushort)players[e.Client].playerTeam);

                using (Message Message = Message.Create(Tags.CreateRoom, ClientRoomWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

        }


        private void StartPrivateRoom(object sender, MessageReceivedEventArgs e)
        {
            bool isTraining = true;
            RoomType _temp;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    isTraining = reader.ReadBoolean();
                }

            }

            lastRoomID += 1;

            if (isTraining)
            {
                _temp = RoomType.Training;
            }
            else
            {
                _temp = RoomType.Tutorial;
            }

            Room newRoom = new Room(
            this,
            lastRoomID,
            players[e.Client].Name + "' private room",
            players[e.Client],
            e.Client,
            6,
            _temp
            );

            players[e.Client].IsHost = true;
            players[e.Client].Room = newRoom;
            players[e.Client].playerTeam = Team.red;

            rooms.Add(lastRoomID, newRoom);

            Log.Message("Private ROOM : " + players[e.Client].ID + " - " + players[e.Client].Name);

            newRoom.StartPrivateRoom(players[e.Client].ID, isTraining);


        }

        private void ConvertPrivateRoomType(object sender, MessageReceivedEventArgs e)
        {
            RoomType _temp;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _temp = (RoomType)reader.ReadUInt16();
                }

                Room room = rooms[players[e.Client].Room.ID];

                room.roomType = _temp;

                room.ConvertPrivateRoomType(e.Client);
            }
        }


        private void SetTrainingCharacter(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    players[e.Client].Room.TryPickCharacter((Character)reader.ReadUInt16(), e.Client);
                }

            }

        }


        private void JoinRoom(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomID = 0;
            Random r = new Random();

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    bool isRandomJoin = reader.ReadBoolean();

                    if (isRandomJoin)
                    {
                        _roomID = (ushort)(r.Next(0, rooms.Count));
                    }
                    else
                    {
                        _roomID = reader.ReadUInt16();
                    }
                }

            }

            if (rooms[_roomID].PlayerCanEnterThis() == false)
            {
                return;
            }

            players[e.Client].playerTeam = rooms[_roomID].GetTeamWithLowestPlayerAmount();
            players[e.Client].Room = rooms[_roomID];

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                RoomWriter.Write(players[e.Client]);

                using (Message Message = Message.Create(Tags.PlayerJoinedRoom, RoomWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in rooms[_roomID].Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }

            rooms[_roomID].Players.Add(e.Client, players[e.Client]);

            WriteEvent(players[e.Client].ID + " - " + players[e.Client].Name + " joined Room : " + _roomID + " as team --> " + players[e.Client].playerTeam, LogType.Info);

            using (DarkRiftWriter JoinWriter = DarkRiftWriter.Create())
            {
                // Recu par le joueur qui rejoint la room

                JoinWriter.Write(rooms[_roomID].ID);
                JoinWriter.Write((ushort)players[e.Client].playerTeam);

                // Liste des joueurs déja présents dans la room
                JoinWriter.Write(rooms[_roomID].Players.Count);

                foreach (KeyValuePair<IClient, Player> p in rooms[_roomID].Players)
                {
                    JoinWriter.Write(p.Value);
                }

                using (Message Message = Message.Create(Tags.JoinRoom, JoinWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

        }

        private void QuitRoomReceiver(object sender, MessageReceivedEventArgs e)
        {
            Room _room;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _roomID = reader.ReadUInt16();
                    _room = rooms[_roomID];
                }
            }

            QuitRoom(e.Client, _room);
        }

        private void QuitRoom(IClient Eclient, Room room)
        {
            using (DarkRiftWriter QuitWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                QuitWriter.Write(players[Eclient]);

                using (Message Message = Message.Create(Tags.PlayerQuitRoom, QuitWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in rooms[room.ID].Players)
                    {
                        if (Eclient.ID == client.Key.ID)
                        {
                            continue;
                        }
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }

                }
            }
            room.NewServerChatMessage(players[Eclient].Name + " QUIT");
            room.Players.Remove(Eclient);

            if (players[Eclient].IsInGameScene) // Si en game
            {
                players[Eclient].IsInGameScene = false;
                room.SupprPlayerObj(Eclient.ID);
            }

            players[Eclient].Room = null;
            players[Eclient].ResetData();



            if (players[Eclient].IsHost)
            {
                SwapHost(Eclient, room);
            }

            using (DarkRiftWriter QuitWriter = DarkRiftWriter.Create())
            {
                // Recu par le joueur qui quitte la room

                using (Message Message = Message.Create(Tags.QuitRoom, QuitWriter))
                {
                    Eclient.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        private void SwapHost(IClient Eclient, Room room)
        {
            players[Eclient].IsHost = false;

            if (room.Players.Count == 0)
            {
                RoomEmpty(rooms[room.ID]);
                return;
            }

            Player newhost = room.Players.First().Value;

            newhost.IsHost = true;
            room.Host = newhost;

            using (DarkRiftWriter SwapHostWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                SwapHostWriter.Write(newhost.ID);

                using (Message Message = Message.Create(Tags.SwapHostRoom, SwapHostWriter))
                {
                    foreach (KeyValuePair<IClient, Player> client in room.Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }



        }

        private void RoomEmpty(Room room)
        {
            if (room.Players.Count == 0)
            {
                rooms[room.ID].Destroy();
                rooms.Remove(room.ID);

                using (DarkRiftWriter DeleteRoomWriter = DarkRiftWriter.Create())
                {
                    // Recu par les joueurs déja présent dans la room

                    DeleteRoomWriter.Write(room.ID);

                    using (Message Message = Message.Create(Tags.DeleteRoom, DeleteRoomWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                            client.SendMessage(Message, SendMode.Reliable);
                    }
                }

            }
        }

        private void SpawnObjPlayer(object sender, MessageReceivedEventArgs e)
        {
            bool isres;
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    isres = reader.ReadBoolean();
                }
                ushort ID = e.Client.ID;

                rooms[players[e.Client].Room.ID].SpawnObjPlayer(ID, isres);
            }
        }

        private void SendPlayerMovement(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    float newX = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    players[e.Client].SetPos(newX, newZ);

                    rooms[players[e.Client].Room.ID].SendMovement(sender, e, newX, newZ);
                }
            }
        }

        private void SendPlayerRota(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    short rotaY = reader.ReadInt16();

                    rooms[players[e.Client].Room.ID].SendRota(sender, e, rotaY);
                }
            }
        }

        private void SendPlayerState(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();

                    int newFlag = reader.ReadInt32();

                    rooms[_roomId].SendState(sender, e, newFlag);
                }
            }
        }

        private void SendForcedMovement(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();

                    sbyte newXDirection = reader.ReadSByte();
                    sbyte newZDirection = reader.ReadSByte();
                    ushort newDuration = reader.ReadUInt16();
                    ushort newStrength = reader.ReadUInt16();

                    ushort targetId = reader.ReadUInt16();


                    rooms[_roomId].SendForcedMovemment(sender, e, newXDirection, newZDirection, newDuration, newStrength, targetId);
                }
            }
        }


        private void SendNewStatus(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();

                    int newStatus = reader.ReadInt32();

                    ushort playerTargeted = reader.ReadUInt16();

                    rooms[_roomId].SendStatus(sender, e, newStatus, playerTargeted);
                }
            }
        }

        #endregion

        #region Game
        private void LobbyStartGame(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();
                }

                Room _room = rooms[_roomId];

                _room.IsStarted = true;

                if (!_room.IsAllPlayersReady())
                {
                    return;
                }

                using (DarkRiftWriter StartGameWriter = DarkRiftWriter.Create())
                {
                    using (Message Message = Message.Create(Tags.LobbyStartGame, StartGameWriter))
                    {
                        foreach (KeyValuePair<IClient, Player> client in _room.Players)
                            client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        private void StartGame(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomID;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomID = reader.ReadUInt16();
                }
            }

            rooms[_roomID].StartGame();


        }

        private void PlayerJoinGameScene(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                players[e.Client].IsInGameScene = true;

                using (DarkRiftReader reader = message.GetReader())
                {
                    rooms[reader.ReadUInt16()].PlayerJoinGameScene();
                }
            }
        }

        #endregion

        #region ChampSelect

        private void SelectCharacter(object sender, MessageReceivedEventArgs e)
        {
            Character _character;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _character = (Character)reader.ReadUInt16();
                }
            }

            rooms[players[e.Client].Room.ID].TryPickCharacter(_character, e.Client);
        }


        private void CharacterSwap(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort askingPlayer = reader.ReadUInt16();

                    rooms[players[e.Client].Room.ID].CharacterSwap(askingPlayer, e.Client.ID);
                }
            }
        }
        private void RefuseCharacterSwap(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort askingPlayer = reader.ReadUInt16();

                    rooms[players[e.Client].Room.ID].RefuseCharacterSwap(e.Client);
                }
            }
        }

        private void BrumeSoulPicked(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort soulBrumeIndex = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room

                        Writer.Write(soulBrumeIndex);

                        using (Message Message = Message.Create(Tags.BrumeSoulPicked, Writer))
                        {
                            foreach (IClient client in rooms[players[e.Client].Room.ID].Players.Keys)
                                client.SendMessage(Message, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        #endregion

        private void NewChatMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    string _message = reader.ReadString();
                    bool _teamOnly = reader.ReadBoolean();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(e.Client.ID);
                        Writer.Write(_message);
                        Writer.Write(false);

                        using (Message Message = Message.Create(Tags.NewChatMessage, Writer))
                        {

                            foreach (KeyValuePair<IClient, Player> client in rooms[players[e.Client].Room.ID].Players)
                            {
                                if (_teamOnly && players[e.Client].playerTeam != client.Value.playerTeam)
                                {
                                    continue;
                                }

                                client.Key.SendMessage(Message, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }


        private void OnSonarState(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _wxState = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_wxState);

                        using (Message Message = Message.Create(Tags.WXSonarState, Writer))
                        {
                            Room currentRoom = rooms[players[e.Client].Room.ID];

                            foreach (KeyValuePair<IClient, Player> client in currentRoom.Players)
                            {
                                if (e.Client.ID != client.Value.ID &&
                                    currentRoom.GetPlayerByID(e.Client.ID).playerTeam == currentRoom.GetPlayerByID(client.Value.ID).playerTeam)
                                {
                                    client.Key.SendMessage(Message, SendMode.Reliable);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AskForStopGame(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    rooms[players[e.Client].Room.ID].StopGame();
                }
            }
        }



        public override Command[] Commands => new Command[]
        {
            new Command("GetRoomInfo", "Retreives the room info from the game.", "room info (-room=<id>)", RoomCommand),
        new Command("GetRoomInterInfo", "Retreives the room inter info from the game.", "room info (-room=<id>)", InteractibleCommand)
        };


        void RoomCommand(object sender, CommandEventArgs e)
        {
            if (e.RawArguments[0] == null)
            {
                Log.Message("NEED ROOM ID AS PARAMETERS", MessageType.Error);
                return;
            }

            ushort roomID = Convert.ToUInt16(e.RawArguments[0]);

            Room r = null;
            if (rooms.ContainsKey(roomID))
            {
               r = rooms[roomID];
            } else
            {
                Log.Message("Room ID not existing", MessageType.Error);
                return;
            }

            r.ShowInfoInConsole();


        }




        void InteractibleCommand(object sender, CommandEventArgs e)
        {
            if (e.RawArguments[0] == null)
            {
                Log.Message("NEED ROOM ID AS PARAMETERS", MessageType.Error);
                return;
            }

            ushort roomID = Convert.ToUInt16(e.RawArguments[0]);

            Room r = null;
            if (rooms.ContainsKey(roomID))
            {
                r = rooms[roomID];
            }
            else
            {
                Log.Message("Room ID not existing", MessageType.Error);
                return;
            }

            r.InteractibleInfoConsole();


        }




    }


}
