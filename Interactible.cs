using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.GameData;

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

        public bool canProgress = true;
        public float totalProgress = 0;

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
            playerTriggeredInZone.Add(player);

            CheckCapture(player);
        }
        public void RemovePlayerInZone(Player player)
        {
            playerTriggeredInZone.Remove(player);
            playerTriggeredInZone.RemoveAll(item => item == null);

            CheckCapture();
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
                    StopCaptureInServer(true);
                    return;
                }

                if (playerTriggeredInZone.Count > 0)
                {
                    if (canProgress == false && ContainTwoTeam() == false) // SI MIS SUR PAUSE ET QUE UNE SEUL TEAM RESTANTE
                    {
                        if (playerTriggeredInZone.Contains(capturingPlayer))
                        {
                            PauseProgress(false);
                        }
                        else
                        {
                            capturingPlayer = GetClosestPlayer();
                            TryCapture(capturingPlayer);
                            ResetCapture();
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

            if (finalize)
            {
                room.RemoveInteractible(this.ID);
            }
        }

        private void TryCapture(Player player)
        {
            NetworkInteractibleManager.Instance.TryCaptureInteractible(ID, player);
        }

        public void CaptureProgress(float progress)
        {
            if (canProgress)
            {
                this.totalProgress += progress;

                NetworkInteractibleManager.Instance.SendInteractibleProgress(ID, capturingPlayer.ID, totalProgress, room);

                if (totalProgress >= 1)
                {
                    Captured();
                }
            }
        }

        public void Captured()
        {
            NetworkInteractibleManager.Instance.CaptureInteractible(ID, capturingPlayer, type, room);

            room.RemoveInteractible(this.ID);
        }

        private void ResetCapture()
        {
            canProgress = true;
            totalProgress = 0;
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
