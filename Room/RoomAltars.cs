using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.ServerData;

namespace BrumeServer
{
    public class RoomAltars
    {
        public List<ushort> remainingAltarID = new List<ushort>(); // Altar in game must have this ID

        public Dictionary<Team, ushort> capturedAltarCount = new Dictionary<Team, ushort>();

        readonly Random random = new Random();

        public bool canUnlockMoreAltars = true;

        private Room room;
        public RoomAltars(Room room)
        {
            this.room = room;
            InitAltars();
        }

        public void InitAltars()
        {
            capturedAltarCount.Add(Team.red, 0);
            capturedAltarCount.Add(Team.blue, 0);

            foreach (ushort alrID in ServerData.altarsID)
            {
                remainingAltarID.Add(alrID);
            }
        }

        public void ResetAltars()
        {
            canUnlockMoreAltars = true;
            capturedAltarCount.Clear();
            remainingAltarID.Clear();
            InitAltars();
        }

        internal ushort GetRandomFreeAltar()
        {
            int chosenID = random.Next(remainingAltarID.Count);

            return remainingAltarID[chosenID];
        }

        public void ChooseAltar(ushort altarListID)
        {
            remainingAltarID.Remove(altarListID);
        }

        public void CaptureAltar(Team team)
        {
            capturedAltarCount[team]++;
            if (capturedAltarCount[team] >= room.brumeServerRef.gameData.AltarCountNeededToWin)
            {
                canUnlockMoreAltars = false;
                room.StartRoundFinalPhase();
            }
        }
    }
}
