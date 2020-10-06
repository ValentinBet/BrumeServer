using System;
using System.Collections.Generic;
using System.Text;

namespace BrumeServer
{
    public class Room
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


    }
}
