using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Text;
using static BrumeServer.ServerData;

namespace BrumeServer
{
    public class Player : IDarkRiftSerializable
    {
        public ushort ID { get; set; }
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }
        public bool IsReady { get; set; }
        public ushort ultStacks { get; set; }
        public float X { get; set; }
        public float Z { get; set; }
        public ushort lifePoint { get; set; }

        public Team playerTeam = Team.none;
        public Character playerCharacter = Character.none;

        public bool IsInGameScene = false;

        public Player(ushort ID, bool isHost, string name, Team team = Team.none)
        {
            this.ID = ID;
            this.IsHost = isHost;
            this.Name = name;
            this.playerTeam = team;
        }
        public void Deserialize(DeserializeEvent e)
        {
            this.ID = e.Reader.ReadUInt16();
            this.IsHost = e.Reader.ReadBoolean();
            this.Name = e.Reader.ReadString();
            this.IsReady = e.Reader.ReadBoolean();
            this.ultStacks = e.Reader.ReadUInt16();
            this.playerTeam = (Team)e.Reader.ReadUInt16();
            this.playerCharacter = (Character)e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(IsHost);
            e.Writer.Write(Name);
            e.Writer.Write(IsReady);
            e.Writer.Write(ultStacks);
            e.Writer.Write((ushort)playerTeam);
            e.Writer.Write((ushort)playerCharacter);
        }

        internal void SetPos(float newX, float newZ)
        {
            X = newX;
            Z = newZ;
        }
        internal void SetLifePoint(ushort life)
        {
            lifePoint = life;
        }

        internal void TakeDamages(ushort life)
        {
            lifePoint -= life;
        }
    }
}
