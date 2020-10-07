using DarkRift.Server;
using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public class BrumeServer : Plugin
    {
        public BrumeServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 0);


        Dictionary<IClient, RoomPlayer> players = new Dictionary<IClient, RoomPlayer>();
        Dictionary<ushort, Room> rooms = new Dictionary<ushort, Room>();

        private ushort lastRoomID = 0;

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Random r = new Random();
            RoomPlayer newPlayer = new RoomPlayer(
                e.Client.ID,
                false,
                "TEST",
                 (byte)r.Next(0, 200),
                 (byte)r.Next(0, 200),
                 (byte)r.Next(0, 200));

            players.Add(e.Client, newPlayer);

            using (DarkRiftWriter NewPlayerWriter = DarkRiftWriter.Create())
            {

                // Recu par le créateur de la room

                NewPlayerWriter.Write(newPlayer);

                using (Message Message = Message.Create(Tags.PlayerConnected, NewPlayerWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

            SendAllRooms(sender, e);

            e.Client.MessageReceived += MessageReceivedFromClient;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            RoomPlayer _roomPlayer = players[e.Client];

            if (_roomPlayer.RoomID != 0)
            {
                QuitRoom(e.Client, rooms[_roomPlayer.RoomID]);
            }

            players.Remove(e.Client);
        }


        private void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.CreateRoom)
                {
                    CreateRoom(sender, e);
                }
                else if (message.Tag == Tags.JoinRoom)
                {
                    JoinRoom(sender, e);
                }
                else if (message.Tag == Tags.QuitRoom)
                {
                    QuitRoomReceiver(sender, e);
                }
                else if (message.Tag == Tags.StartGame)
                {
                    StartGame(sender, e);
                }
                else if (message.Tag == Tags.QuitGame)
                {
                    QuitGame(sender, e);
                }
            }
        }

        private void StartGame(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();
                }

                Room _room = rooms[_roomId];

                SpawnPlayers(_room);

                using (DarkRiftWriter StartGameWriter = DarkRiftWriter.Create())
                {
                    using (Message Message = Message.Create(Tags.StartGame, StartGameWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients().Where(x => _room.Players.Contains(players[x])))
                            client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        private void SpawnPlayers(Room room)
        {

            /////////////////////////////////////////////////
            //////////////// FAIRE SPAWN LES JOUEURS
            /////////////////////////////////////////////////
            
            using (DarkRiftWriter StartGameWriter = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.SpawnObjPlayer, StartGameWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => room.Players.Contains(players[x])))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        private void QuitGame(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();
                }

                Room _room = rooms[_roomId];

                using (DarkRiftWriter QuitGameWriter = DarkRiftWriter.Create())
                {
                    using (Message Message = Message.Create(Tags.QuitGame, QuitGameWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients().Where(x => _room.Players.Contains(players[x])))
                            client.SendMessage(Message, SendMode.Reliable);
                    }
                }

                /////////////////////////////////////////////////
                //////////////// Rajouter le retour OU suppression de la room ETC
                /////////////////////////////////////////////////
            }
        }



        private void SendAllRooms(object sender, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter SendAllRoomsWriter = DarkRiftWriter.Create())
            {
                SendAllRoomsWriter.Write(rooms.Count);

                foreach (KeyValuePair<ushort, Room> r in rooms)
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
            string _name = "";
            lastRoomID += 1;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _name = reader.ReadString();
                }
            }

            Room newRoom = new Room(
            lastRoomID,
            _name,
            players[e.Client]
            );

            players[e.Client].IsHost = true;
            players[e.Client].RoomID = lastRoomID;

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Nouvelle room crée recu par tout les joueurs

                RoomWriter.Write(lastRoomID);
                RoomWriter.Write(_name);
                RoomWriter.Write(newRoom.Host.ID);

                using (Message Message = Message.Create(Tags.CreateRoom, RoomWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }

            rooms.Add(lastRoomID, newRoom);

            using (DarkRiftWriter ClientRoomWriter = DarkRiftWriter.Create())
            {

                // Recu par le créateur de la room

                ClientRoomWriter.Write(lastRoomID);
                ClientRoomWriter.Write(_name);
                ClientRoomWriter.Write(newRoom.Host.ID);

                using (Message Message = Message.Create(Tags.CreateRoom, ClientRoomWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
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

            rooms[_roomID].Players.Add(players[e.Client]);
            players[e.Client].RoomID = _roomID;

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                RoomWriter.Write(players[e.Client]);

                using (Message Message = Message.Create(Tags.PlayerJoinedRoom, RoomWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client && rooms[_roomID].Players.Contains(players[x])))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }


            using (DarkRiftWriter JoinWriter = DarkRiftWriter.Create())
            {
                // Recu par le joueur qui rejoint la room

                JoinWriter.Write(rooms[_roomID].ID);

                // Liste des joueurs déja présents dans la room
                RoomPlayer[] _playerInThisRoom = rooms[_roomID].Players.ToArray();
                JoinWriter.Write(_playerInThisRoom.Length);

                foreach (RoomPlayer p in _playerInThisRoom)
                {
                    JoinWriter.Write(p);
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
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != Eclient && rooms[room.ID].Players.Contains(players[x])))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }

            rooms[room.ID].Players.Remove(players[Eclient]);
            rooms[room.ID].Players.RemoveAll(x => x == null);
            players[Eclient].RoomID = 0;

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

            RoomPlayer newhost = room.Players.First();

            using (DarkRiftWriter SwapHostWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                SwapHostWriter.Write(newhost.ID);

                using (Message Message = Message.Create(Tags.SwapHostRoom, SwapHostWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => room.Players.Contains(players[x])))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }

            newhost.IsHost = true;
            room.Host = newhost;

        }

        private void RoomEmpty(Room room)
        {
            if (room.Players.Count == 0)
            {
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
    }
}
