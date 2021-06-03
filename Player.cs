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
        public ushort LifePoint { get; set; }
        public ushort MaxlifePoint { get; set; }

        public Team playerTeam = Team.none;
        public Character playerCharacter = Character.none;

        public En_CharacterState playerState = En_CharacterState.Clear;

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
            LifePoint = life;
        }

        internal void TakeDamages(ushort life)
        {
            LifePoint -= life;
        }

        internal void ResetData()
        {
            Room = null;
            playerTeam = Team.none;
            playerCharacter = Character.none;
            IsReady = false;
            SetPos(0, 0);

        }

        internal void ResetDataBetweenRounds()
        {
            SetPos(0, 0);
            UpdateState(1);
            IsInGameScene = false;
        }

        internal void UpdateState(int _state)
        {
            playerState = (En_CharacterState)_state;
        }

        internal bool CanTakeDamages()
        {
            if (playerState.HasFlag(En_CharacterState.Intangenbility) || playerState.HasFlag(En_CharacterState.Invulnerability))
            {
                return false;
            }

            return true;
        }
    }
}


[System.Flags]
public enum En_CharacterState
{
    Clear = 1 << 0,
    Slowed = 1 << 1,
    SpedUp = 1 << 2,
    Root = 1 << 3,
    Canalysing = 1 << 4,
    Silenced = 1 << 5,
    Crouched = 1 << 6,
    Embourbed = 1 << 7,
    WxMarked = 1 << 8,
    Hidden = 1 << 9,
    Invulnerability = 1 << 10,
    Intangenbility = 1 << 11,
    PoweredUp = 1 << 12,
    ForcedMovement = 1 << 13,
    StopInterpolate = 1 << 14,

    Stunned = Silenced | Root,
}

