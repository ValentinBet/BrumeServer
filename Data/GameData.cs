﻿
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
            Frog = 3,
            ResurectAltar = 4,
            HealthPack = 5,
            UltPickup = 6
        }
        public enum SpellStep : ushort
        {
            canalisation = 0,
            annonciation = 1,
            resolution = 2,
            throwback = 3
        }

        public static readonly ushort resObjInstansiateID = 5;

        public static readonly List<ushort> altarsID = new List<ushort>() { 0, 1, 2 };

        public static readonly Dictionary<Character, Player> ChampSelectSlots = new Dictionary<Character, Player>() { 
            { Character.shili, null }, 
            { Character.A, null },
            { Character.B, null } 
        };

        public static readonly Dictionary<Character, ushort> ChampMaxUltStacks = new Dictionary<Character, ushort>() {
            { Character.shili, 7 },
            { Character.A, 7 },
            { Character.B, 7 }
        };

        //<<

        public static int SpawnWallTime = 15000; // mss
        public static int GameInitTime = 5000; // mss
        public static int AltarLockTime = 15000; // ms
        public static int FrogRespawnTime = 30000; // ms
        public static int healthPackRespawnTime = 20000; // ms
        public static int UltPickupRespawnTime = 30000; // ms
        public static int VisionTowerReactivateTime = 20000; // ms       
        public static int BrumeSoulRespawnMinTime = 5000; // ms       
        public static int BrumeSoulRespawnMaxTime = 7000; // ms       
           
        public static int RoundToWin = 5;
        public static int BrumeCount = 9;


    }
}
