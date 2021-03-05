
using System.Collections.Generic;

namespace BrumeServer
{
    public class ServerData
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
            WuXin = 10,
            Re = 20,
            Leng = 30
        }

        public enum InteractibleType : ushort
        {
            none = 0,
            Altar = 1,
            VisionTower = 2, // DEPRECATED
            Frog = 3,
            ResurectAltar = 4, // DEPRECATED
            HealthPack = 5, // DEPRECATED
            UltPickup = 6,
            EndZone = 7
        }
        public enum SpellStep : ushort
        {
            canalisation = 0,
            annonciation = 1,
            resolution = 2,
            throwback = 3
        }

        public static readonly ushort resObjInstansiateID = 5; // DEPRECATED

        public static readonly List<ushort> altarsID = new List<ushort>() { 0, 1, 2 };

        public static readonly Dictionary<Character, Player> ChampSelectSlots = new Dictionary<Character, Player>() { 
            { Character.WuXin, null }, 
            { Character.Re, null },
            { Character.Leng, null } 
        };


        //<<



    }
}
