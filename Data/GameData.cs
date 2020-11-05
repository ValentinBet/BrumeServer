
using System.Collections.Generic;

namespace BrumeServer
{
    public class GameData
    {        
        // NE PAS modifier ces valeurs >>

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

        public enum InteractibleType : ushort
        {
            none = 0,
            Altar = 1,
            VisionTower = 2,
            Frog = 3
        }

        public static readonly List<ushort> altarsID = new List<ushort>() { 0, 1, 2 };

        public static readonly Dictionary<Character, Player> ChampSelectSlots = new Dictionary<Character, Player>() { 
            { Character.shili, null }, 
            { Character.A, null },
            { Character.B, null } 
        };

        //<<

        public static int GameTime = 300000; // ms
        public static int AltarLockTime = 15000; // ms
        public static int FrogRespawnTime = 10000; // ms


    }
}
