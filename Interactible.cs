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

        public bool captured = false;
        public bool canProgress = true;
        public float totalProgress = 0;

        public bool endZoneTimerEnd = false;
        public bool overtimeTimerStarted = false;
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
            if (playerTriggeredInZone.Contains(player))
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

            playerTriggeredInZone.Add(player);
            playerTriggeredInZone.RemoveAll(item => item == null);

            CheckCapture(player);
        }
        public void RemovePlayerInZone(Player player)
        {
            if (playerTriggeredInZone.Contains(player))
            {
                playerTriggeredInZone.Remove(player);
                playerTriggeredInZone.RemoveAll(item => item == null);

                CheckCapture();
            }
        }

        private void CheckCapture(Player player = null)
        {
            //CheckForEndZoneOvertime(player); 

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
                }
            }
        }

        //public void CheckForEndZoneOvertime(Player player = null)
        //{
        //    if (type == InteractibleType.EndZone && endZoneTimerEnd)
        //    {
        //        if (playerTriggeredInZone.Count > 0)
        //        {
        //            if (ContainTwoTeam() == false) // SI UNE SEUL TEAM RESTANTE
        //            {
        //                if (GetClosestPlayer().playerTeam == room.defendingEndZoneTeam)
        //                {
        //                    if (overtimeTimerStarted)
        //                    {
        //                        room.SetEndZoneOvertime(true);
        //                    } else
        //                    {
        //                        room.EndZoneCaptured(room.defendingEndZoneTeam);
        //                    }

        //                }
        //            } else
        //            {
        //                overtimeTimerStarted = true;
        //                room.SetEndZoneOvertime(false); // Si deux team alors contest
        //            }
        //        }
        //        else
        //        {
        //            if (overtimeTimerStarted)
        //            {
        //                room.SetEndZoneOvertime(true);
        //            }
        //            else
        //            {
        //                room.EndZoneCaptured(room.defendingEndZoneTeam);
        //            }
        //        }

        //    }

        //}

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
                } else
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
            int red = 0;
            int blue = 0;

            foreach (Player p in playerTriggeredInZone)
            {
                switch (p.playerTeam)
                {
                    case Team.none:
                        throw new Exception("TEAM NOT AVAILABLE");
                    case Team.red:
                        red++;
                        break;
                    case Team.blue:
                        blue++;
                        break;
                    default: throw new Exception("TEAM NOT EXISTING");
                }
            }

            if (blue > 0 && red > 0)
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
            NetworkInteractibleManager.Instance.TryCaptureInteractible(ID, player);
        }

        public void CaptureProgress(float progress)
        {
            if (canProgress)
            {
                this.totalProgress = progress;
                NetworkInteractibleManager.Instance.SendInteractibleProgress(ID, capturingPlayer.ID, totalProgress, room);

                if (totalProgress >= 1)
                {
                    Captured();
                }
            }
        }

        public void Captured()
        {
            if (captured)
            {
                return;
            }

            captured = true;

            ResetCapture();

            NetworkInteractibleManager.Instance.CaptureInteractible(ID, capturingPlayer, type, room);

           // room.RemoveInteractible(this.ID);
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
