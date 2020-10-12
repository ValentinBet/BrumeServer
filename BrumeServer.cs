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
            ClientManager.ClientDisconnected += OnClientDisonnected;
        }

        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 0);


        Dictionary<IClient, RoomPlayer> players = new Dictionary<IClient, RoomPlayer>();
        Dictionary<ushort, Room> rooms = new Dictionary<ushort, Room>();

        private ushort lastRoomID = 0;

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            /*
            Random r = new Random();
            RoomPlayer newPlayer = new RoomPlayer(
                e.Client.ID,
                false,
                "TEST",
                 (byte)r.Next(0, 200),
                 (byte)r.Next(0, 200),
                 (byte)r.Next(0, 200));

            players.Add(e.Client, newPlayer);

            e.Client.MessageReceived += MessageReceivedFromClient;
            */

            SamPlayers.Add(e.Client);

            SpawnObjPlayer(sender, e);

            e.Client.MessageReceived += MessageReceivedFromClient;
        }
        
        private void OnClientDisonnected(object sender, ClientDisconnectedEventArgs e)
        {
            PlayerDisconnected(sender, e);
            SamPlayers.Remove(e.Client);
        }

        void PlayerDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                ushort ID = e.Client.ID;

                GameWriter.Write(ID);

                using (Message Message = Message.Create(Tags.SupprObjPlayer, GameWriter))
                {
                    foreach (IClient client in SamPlayers.Where(x => x != e.Client))
                    {
                        client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        private void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.CreateRoom)
                {
                    CreateRoom(sender, e);
                }
                
                if (message.Tag == Tags.JoinRoom)
                {
                    JoinRoom(sender, e);
                }

                if (message.Tag == Tags.MovePlayerTag)
                {
                    SendMovement(sender, e);
                }

                if (message.Tag == Tags.SendAnim)
                {
                    SendAnim(sender, e);
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

                JoinWriter.Write(roomID);

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

        List<IClient> SamPlayers = new List<IClient>();
        private void SpawnObjPlayer(object sender, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                ushort ID = e.Client.ID;

                GameWriter.Write(ID);

                using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
                {
                    foreach (IClient client in SamPlayers)
                    {
                        client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }

            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                foreach (IClient client in SamPlayers.Where(x => x != e.Client))
                {
                    ushort ID = client.ID;
                    GameWriter.Write(ID);

                    using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
                    {
                        e.Client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        private void SendMovement(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newZ = reader.ReadSingle();

                        float rotaX = reader.ReadSingle();
                        float rotaY = reader.ReadSingle();
                        float rotaZ = reader.ReadSingle();

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(e.Client.ID);

                            writer.Write(newX);
                            writer.Write(newZ);

                            writer.Write(rotaX);
                            writer.Write(rotaY);
                            writer.Write(rotaZ);

                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, e.SendMode);
                    }
                }
            }
        }

        private void SendAnim(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    float foward = reader.ReadSingle();
                    float right = reader.ReadSingle();

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(e.Client.ID);

                        writer.Write(foward);
                        writer.Write(right);

                        message.Serialize(writer);
                    }

                    foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                        c.SendMessage(message, e.SendMode);
                }
            }
        }
    }
}
