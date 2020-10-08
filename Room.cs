using DarkRift;
using System;
using System.Collections.Generic;
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

        public List<PlayerData> Players = new List<PlayerData>();

        public Room(ushort ID, string name, PlayerData host, int maxPlayers = 12)
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

            foreach (PlayerData player in Players)
            {
                if (player.playerTeam == team)
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
    }
}
