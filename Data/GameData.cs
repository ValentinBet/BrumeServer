using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.ServerData;

namespace BrumeServer
{
    public class GameData
    {
        public string fileName = "GameData.JSON";
        public int SpawnWallTime = 15000; // mss
        public int GameInitTime = 5000; // mss
        public int EndZoneTime = 60000; // mss
        public int AltarLockTime = 15000; // ms
        public int FrogRespawnTime = 30000; // ms
        public int healthPackRespawnTime = 20000; // ms
        public int UltPickupRespawnTime = 30000; // ms
        public int VisionTowerReactivateTime = 20000; // ms       
        public int BrumeSoulRespawnMinTime = 5000; // ms       
        public int BrumeSoulRespawnMaxTime = 7000; // ms       
        public int RoundToWin = 5;
        public int BrumeCount = 9;
        public int AltarCountNeededToWin = 2;

        public Dictionary<Character, ushort> ChampMaxUltStacks = new Dictionary<Character, ushort>() {
            { Character.shili, 7 },
            { Character.A, 7 },
            { Character.B, 7 }
        };

        public GameData()
        {

        }

        internal void Init()
        {
            ReadAndSet();
        }

        public void Write()
        {
            string output = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(fileName, output);
        }

        public void ReadAndSet()
        {
            string output = File.ReadAllText(fileName);

            GameData newGameData = JsonConvert.DeserializeObject<GameData>(output);

            this.SpawnWallTime = newGameData.SpawnWallTime;
            this.GameInitTime = newGameData.GameInitTime;
            this.EndZoneTime = newGameData.EndZoneTime;
            this.AltarLockTime = newGameData.AltarLockTime;
            this.FrogRespawnTime = newGameData.FrogRespawnTime;
            this.healthPackRespawnTime = newGameData.healthPackRespawnTime;
            this.UltPickupRespawnTime = newGameData.UltPickupRespawnTime;
            this.VisionTowerReactivateTime = newGameData.VisionTowerReactivateTime;
            this.BrumeSoulRespawnMinTime = newGameData.BrumeSoulRespawnMinTime;
            this.BrumeSoulRespawnMaxTime = newGameData.BrumeSoulRespawnMaxTime;
            this.RoundToWin = newGameData.RoundToWin;
            this.BrumeCount = newGameData.BrumeCount;
            this.AltarCountNeededToWin = newGameData.AltarCountNeededToWin;
            this.ChampMaxUltStacks = newGameData.ChampMaxUltStacks;

            Log.Message("JSON Game Data loaded !");
        }

    }
}
