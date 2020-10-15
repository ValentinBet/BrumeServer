using DarkRift;
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
        public int MaxPlayers { get; set; }
        public int PlayersCount { get; set; }
        public PlayerData Host { get; set; }

        public Dictionary<IClient, PlayerData> Players = new Dictionary<IClient, PlayerData>();

        public Room(ushort ID, string name, PlayerData host, IClient hostClient, int maxPlayers = 12)
        {
            this.ID = ID;
            this.Name = name;
            this.Host = host;
            this.MaxPlayers = maxPlayers;
            Players.Add(hostClient, host);
        }

        public Room()
        {

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

            foreach (KeyValuePair<IClient, PlayerData> player in Players)
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
            this.MaxPlayers = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Name);
            e.Writer.Write(MaxPlayers);
        }

        public PlayerData FindPlayerByID(ushort ID)
        {
            return Players.Single(x => x.Key.ID == ID).Value;
        }


        public void StartGame()
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StartGame, Writer))
                {

                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }

                }
            }
        }

        public void SpawnObjPlayer(object sender, MessageReceivedEventArgs e)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                ushort ID = e.Client.ID;

                GameWriter.Write(ID);

                using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }

            //Spawn Old Players
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                foreach (KeyValuePair<IClient, PlayerData> client in Players)
                {
                    if (e.Client == client.Key) { continue; }

                    ushort ID = client.Key.ID;
                    GameWriter.Write(ID);

                    using (Message Message = Message.Create(Tags.SpawnObjPlayer, GameWriter))
                    {
                        e.Client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        public void SupprPlayer(ushort ID)
        {
            using (DarkRiftWriter GameWriter = DarkRiftWriter.Create())
            {
                GameWriter.Write(ID);

                using (Message Message = Message.Create(Tags.SupprObjPlayer, GameWriter))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
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

                foreach (KeyValuePair<IClient, PlayerData> client in Players)
                {
                    if (e.Client == client.Key) { continue; }
                    client.Key.SendMessage(message, e.SendMode);
                }
            }
        }
        public void SendAnim(object sender, MessageReceivedEventArgs e, float foward, float right)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(e.Client.ID);

                    writer.Write(foward);
                    writer.Write(right);

                    message.Serialize(writer);
                }

                foreach (KeyValuePair<IClient, PlayerData> c in Players)
                {
                    if (e.Client == c.Key) { continue; }
                    c.Key.SendMessage(message, e.SendMode);
                }
            }
        }

        public void StartTimer()
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StartTimer, Writer))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }

                }
            }
        }


        public void StopGame()
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                using (Message Message = Message.Create(Tags.StopGame, Writer))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }


        public void Addpoints(ushort targetTeam, ushort value)
        {
            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write(targetTeam);
                TeamWriter.Write(value);

                using (Message Message = Message.Create(Tags.AddPoints, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        public void SyncTrigger(ushort _id, string trigger)
        {
            using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room

                TeamWriter.Write(_id);
                TeamWriter.Write(trigger);

                using (Message Message = Message.Create(Tags.SyncTrigger, TeamWriter))
                {
                    foreach (KeyValuePair<IClient, PlayerData> client in Players)
                        client.Key.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
    }
}
