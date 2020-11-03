
using System.Collections.Generic;

namespace BrumeServer
{
    public class GameData
    {
        public enum Team : ushort
        {
            none = 0,
            red = 10,
            blue = 20
        }
        public enum Character : ushort
        {
            none = 0,
            shili = 10,
            A = 20,
            B = 30
        }

        public static readonly List<ushort> altarsID = new List<ushort>() { 0, 1, 2 };
    }
}
