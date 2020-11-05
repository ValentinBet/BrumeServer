using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.GameData;

namespace BrumeServer
{

    public class ChampSelect
    {
        public Dictionary<Character, Player> RedTeamCharacterPlayerPair = new Dictionary<Character, Player>();
        public Dictionary<Character, Player> BlueTeamCharacterPlayerPair = new Dictionary<Character, Player>();

        public ChampSelect()
        {
            ResetData();
        }

        public void ResetData()
        {
            RedTeamCharacterPlayerPair.Clear();
            BlueTeamCharacterPlayerPair.Clear();

            foreach (KeyValuePair<Character, Player> item in GameData.ChampSelectSlots)
            {
                RedTeamCharacterPlayerPair.Add(item.Key, item.Value);
                BlueTeamCharacterPlayerPair.Add(item.Key, item.Value);
            }
        }

        public bool TryPickChamp(Team team, Player player, Character character)
        {
            Dictionary<Character, Player> _temp = null;

            switch (team)
            {
                case Team.red:
                    _temp = RedTeamCharacterPlayerPair;
                    break;
                case Team.blue:
                    _temp = BlueTeamCharacterPlayerPair;
                    break;
                default: throw new Exception("Team error");
            }

            if (_temp[character] != null) // If there is a player on the slot / Quit
            {
                return false;
            }
            // If there is NO player on the slot / Quit

            if (player.playerCharacter != Character.none) // If the player has already choose a character
            {
                _temp[player.playerCharacter] = null; // He let his last slot free
            }

            _temp[character] = player; // assign his new character

            return true;

        }
    }

}
