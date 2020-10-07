using DarkRift;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrumeServer
{
    public class RoomPlayer : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public ushort RoomID { get; set; }
        public RoomPlayer(ushort ID, bool isHost, string name, byte colorR, byte colorG, byte colorB)
        {
            this.ID = ID;
            this.IsHost = isHost;
            this.Name = name;
            this.ColorR = colorR;
            this.ColorG = colorG;
            this.ColorB = colorB;
        }

        public void Deserialize(DeserializeEvent e)
        {
            this.ID = e.Reader.ReadUInt16();
            this.IsHost = e.Reader.ReadBoolean();
            this.Name = e.Reader.ReadString();
            this.ColorR = e.Reader.ReadByte();
            this.ColorG = e.Reader.ReadByte();
            this.ColorB = e.Reader.ReadByte();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(IsHost);
            e.Writer.Write(Name);
            e.Writer.Write(ColorR);
            e.Writer.Write(ColorG);
            e.Writer.Write(ColorB);
        }

    }
}
