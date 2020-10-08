using DarkRift;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrumeServer
{
    public class Room : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayersCount { get; set; }
        public RoomPlayer Host { get; set; }

        public List<RoomPlayer> Players = new List<RoomPlayer>();

        public Room(ushort ID, string name, RoomPlayer host, int maxPlayers = 12)
        {
            this.ID = ID;
            this.Name = name;
            this.Host = host;
            this.MaxPlayers = maxPlayers;
            Players.Add(host);
        }

        public Room()
        {

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
    }
}
