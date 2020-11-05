using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public class RoomAltars
    {
        public List<ushort> altarID = new List<ushort>(); // Altar in game must have this ID

        readonly Random random = new Random();
        public RoomAltars()
        {
            InitAltars();
        }

        public void InitAltars()
        {
            foreach (ushort alrID in GameData.altarsID)
            {
                altarID.Add(alrID);
            }
        }

        public void ResetAltars()
        {
            altarID.Clear();
            InitAltars();
        }

        internal ushort GetRandomFreeAltar()
        {
            int chosenID = random.Next(altarID.Count);

            return altarID[chosenID];
        }

        public void ChooseAltar(ushort altarListID)
        {
            altarID.Remove(altarListID);
        }
    }
}
