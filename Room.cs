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
        public RoomPlayer Host { get; set; }

        public List<RoomPlayer> Players = new List<RoomPlayer>();

        public Room(ushort ID, string name, RoomPlayer host)
        {
            this.ID = ID;
            this.Name = name;
            this.Host = host;

            Players.Add(host);
        }

        public void Deserialize(DeserializeEvent e)
        {
            this.ID = e.Reader.ReadUInt16();
            this.Name = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Name);
        }
    }
}
