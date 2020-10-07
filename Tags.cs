using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    class Tags
    {
        // Lobby >>       
        public static readonly ushort PlayerConnected = 0;
        public static readonly ushort SendAllRooms = 5;
        public static readonly ushort CreateRoom = 10;
        public static readonly ushort DeleteRoom = 20;
        public static readonly ushort JoinRoom = 30;
        public static readonly ushort PlayerJoinedRoom = 40;
        public static readonly ushort QuitRoom = 50;
        // <<
    }
}
