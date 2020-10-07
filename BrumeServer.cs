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

            SendAllRooms(sender ,e);

            e.Client.MessageReceived += MessageReceivedFromClient;
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
            string name = "";
            lastRoomID += 1;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    name = reader.ReadString();
                }
            }

            Room newRoom = new Room(
            lastRoomID,
            name,
            players[e.Client]
            );

            players[e.Client].IsHost = true;

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Nouvelle room crée recu par tout les joueurs

                RoomWriter.Write(lastRoomID);
                RoomWriter.Write(name);
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
                ClientRoomWriter.Write(name);
                ClientRoomWriter.Write(newRoom.Host.ID);

                using (Message Message = Message.Create(Tags.CreateRoom, ClientRoomWriter))
                {
                    e.Client.SendMessage(Message, SendMode.Reliable);
                }
            }

        }

        private void JoinRoom(object sender, MessageReceivedEventArgs e)
        {
            ushort roomID = 0;
            Random r = new Random();
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    bool isRandomJoin = reader.ReadBoolean();

                    if (isRandomJoin)
                    {
                        roomID = (ushort)(r.Next(0, rooms.Count));
                    }
                    else
                    {
                        roomID = reader.ReadUInt16();
                    }
                }


            }

            rooms[roomID].Players.Add(players[e.Client]);

            using (DarkRiftWriter RoomWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                RoomWriter.Write(players[e.Client]);

                using (Message Message = Message.Create(Tags.PlayerJoinedRoom, RoomWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client && rooms[roomID].Players.Contains(players[x])))
                        client.SendMessage(Message, SendMode.Reliable);
                }
            }


            using (DarkRiftWriter JoinWriter = DarkRiftWriter.Create())
            {
                // Recu par le joueur qui rejoint la room

                JoinWriter.Write(rooms[roomID].ID);

                // Liste des joueurs déja présents dans la room
                RoomPlayer[] _playerInThisRoom = rooms[roomID].Players.ToArray();
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

    }
}
