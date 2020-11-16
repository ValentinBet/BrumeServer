using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.GameData;

namespace BrumeServer
{
    class Factory
    {
        public static Team GetOtherTeam(Team team)
        {
            if (team == Team.blue)
            {
                return Team.red;
            }
            else
            {
                return Team.blue;
            }
        }

        public static bool CheckSendPlayerAnim(Room room, ushort idPlayer1, ushort idPlayer2)
        {
            Vector2 posPlayer1 = new Vector2(room.GetPlayerByID(idPlayer1).X, room.GetPlayerByID(idPlayer1).Z);
            Vector2 posPlayer2 = new Vector2(room.GetPlayerByID(idPlayer2).X, room.GetPlayerByID(idPlayer2).Z);

            if (Vector2.Distance(posPlayer1, posPlayer2) > 15)
            {
                return false;
            }
            return true;
        }
    }

}
