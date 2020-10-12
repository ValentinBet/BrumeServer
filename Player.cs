using DarkRift;

namespace BrumeServer
{
    public class Player : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public float X { get; set; }
        public float Z { get; set; }

        public Player(ushort ID, float x, float y)
        {
            this.ID = ID;
            this.X = x;
            this.Z = y;
        }

        public void Deserialize(DeserializeEvent e)
        {
            this.ID = e.Reader.ReadUInt16();
            this.X = e.Reader.ReadSingle();
            this.Z = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(X);
            e.Writer.Write(Z);
        }
    }
}