using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.ServerData;

namespace BrumeServer
{
    public class Interactible
    {
        public ushort ID;
        public List<Player> playerTriggeredInZone = new List<Player>();
        public Player capturingPlayer;
        public Team capturingTeam = Team.none;
        public Vector3 position;
        public InteractibleType type = InteractibleType.none;
        public Room room;
        public bool contestable = false;

        public bool captured = false;
        public bool canProgress = true;
        public float totalProgress = 0;

        public bool overtimeTimerStarted = false;

        public Dictionary<Team, int> playerCountInEachTeam = new Dictionary<Team, int>() { { Team.red, 0 }, { Team.blue, 0 } };
        public Interactible(ushort ID, Vector3 pos, Team capturingTeam, Player capturingPlayer, InteractibleType type, Room room)
        {
            this.ID = ID;
            position = pos;
            this.capturingTeam = capturingTeam;
            this.type = type;
            this.room = room;
            this.capturingPlayer = capturingPlayer;

            AddPlayerInZone(capturingPlayer);
        }

        public void AddPlayerInZone(Player player)
        {
            if (playerTriggeredInZone.Contains(player) || captured)
            {
                return;
            }

            if (playerTriggeredInZone.Count == 0)
            {
                if (capturingPlayer != null)
                {
                    if (player.playerTeam != capturingPlayer.playerTeam)
                    {
                        ResetCapture();
                    }
                }

                this.capturingTeam = player.playerTeam;
                this.capturingPlayer = player;
            }

            playerCountInEachTeam[player.playerTeam]++;
            playerTriggeredInZone.Add(player);
            playerTriggeredInZone.RemoveAll(item => item == null);

            CheckCapture(player);
        }
        public void RemovePlayerInZone(Player player)
        {
            if (captured)
            {
                return;
            }

            if (playerTriggeredInZone.Contains(player))
            {
                playerCountInEachTeam[player.playerTeam]--;
                playerTriggeredInZone.Remove(player);
                playerTriggeredInZone.RemoveAll(item => item == null);

                CheckCapture();
            }
        }

        private void CheckCapture(Player player = null)
        {

            if (player != null)
            {
                if (playerTriggeredInZone.Count == 1 && playerTriggeredInZone.Contains(player)) // SI CEST LE SEUL JOUEUR
                {
                    TryCapture(player);
                    return;
                }
                if (playerTriggeredInZone.Count > 1)
                {
                    if (canProgress == true && ContainTwoTeam()) // SI CONFLITS
                    {
                        PauseProgress(true);
                    }
                }
            }
            else
            {
                if (playerTriggeredInZone.Count == 0) // SI AUCUN JOUEUR
                {
                    StopCaptureInServer(false);
                    return;
                }

                if (playerTriggeredInZone.Count > 0)
                {
                    if (canProgress == false && ContainTwoTeam() == false) // SI MIS SUR PAUSE ET QUE UNE SEUL TEAM RESTANTE
                    {
                        if (playerTriggeredInZone.Contains(capturingPlayer)) // si toujours la meme team qui capture
                        {
                            PauseProgress(false);
                        }
                        else // sinon
                        {

                            ResetCapture();
                            capturingPlayer = GetClosestPlayer();

                            TryCapture(capturingPlayer);
                        }

                    }
                    else if (canProgress && ContainTwoTeam() == false)
                    {
                        if (!playerTriggeredInZone.Contains(capturingPlayer)) // si toujours la meme team qui capture
                        {
                            capturingPlayer = GetClosestPlayer();
                            TryCapture(capturingPlayer);
                        }
                    }


                }
            }
        }

        private Player GetClosestPlayer()
        {
            float closestDistance = 10000000;
            Player capturingP = null;

            if (playerTriggeredInZone.Count == 0)
            {
                return capturingP; // null
            }

            foreach (Player p in playerTriggeredInZone)
            {
                float _dist = Vector3.Distance(position, new Vector3(p.X, 0, p.Z));

                if (capturingP == null)
                {
                    capturingP = p;
                    closestDistance = _dist;
                }
                else
                {
                    if (closestDistance > _dist)
                    {
                        capturingP = p;
                        closestDistance = _dist;
                    }
                }
            }

            return capturingP;
        }

        public bool ContainTwoTeam()
        {

            //int red = 0;
            //int blue = 0;

            //foreach (Player p in playerTriggeredInZone)
            //{
            //    switch (p.playerTeam)
            //    {
            //        case Team.none:
            //            throw new Exception("TEAM NOT AVAILABLE");
            //        case Team.red:
            //            red++;
            //            break;
            //        case Team.blue:
            //            blue++;
            //            break;
            //        default: throw new Exception("TEAM NOT EXISTING");
            //    }
            //}

            if (playerCountInEachTeam[Team.red] > 0 && playerCountInEachTeam[Team.blue] > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StopCaptureInServer(bool finalize)
        {
            NetworkInteractibleManager.Instance.StopCapturing(ID, room);

            //if (finalize)
            //{
            //    room.RemoveInteractible(this.ID);
            //}
        }

        private void TryCapture(Player player)
        {
            if (captured)
            {
                return;
            }

            NetworkInteractibleManager.Instance.TryCaptureInteractible(ID, player);
        }

        public void CaptureProgress(float progress)
        {
            if (captured)
            {
                return;
            }
              this.totalProgress = progress;

                if (totalProgress >= 1)
                {
                    totalProgress = 1;
                    Captured();
                } else
                {
                    NetworkInteractibleManager.Instance.SendInteractibleProgress(ID, capturingPlayer.ID, totalProgress, room);
                }          
        }

        public void Captured()
        {
            if (captured)
            {
                return;
            }
            totalProgress = 0;
            captured = true;
            room.RemoveInteractible(this.ID);
            NetworkInteractibleManager.Instance.CaptureInteractible(ID, capturingPlayer, type, room);

        }

        private void ResetCapture()
        {
            canProgress = true;
            totalProgress = 0;
            captured = false;

            NetworkInteractibleManager.Instance.SendInteractibleProgress(ID, capturingPlayer.ID, totalProgress, room);
        }
        private void PauseProgress(bool pause)
        {
            canProgress = !pause;

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(ID);
                Writer.Write(pause);

                using (Message Message = Message.Create(Tags.PauseInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in room.Players)
                    {
                        if (client.Key.ID == capturingPlayer.ID)
                        {
                            client.Key.SendMessage(Message, SendMode.Reliable);
                        }

                    }
                }
            }
        }

    }
}
