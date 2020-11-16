using System;
using System.Collections.Generic;
using System.Linq;
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
    }

}
