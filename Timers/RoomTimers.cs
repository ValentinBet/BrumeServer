using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BrumeServer
{
    public class RoomTimers
    {
        private Room room;

        public NetworkTimer altarTimer;
        public NetworkTimer gameInitTimer;
        public NetworkTimer wallTimer;
        public NetworkTimer soulTimer;
      //  public NetworkTimer endZoneTimer;
     //   public NetworkTimer endZoneOvertime;


        public Stopwatch gameTimer;

        public Dictionary<NetworkTimer, ushort> frogTemporaryTimers = new Dictionary<NetworkTimer, ushort>();
        public Dictionary<NetworkTimer, ushort> healthPackTemporaryTimers = new Dictionary<NetworkTimer, ushort>();
        public Dictionary<NetworkTimer, ushort> ultPickupTemporaryTimers = new Dictionary<NetworkTimer, ushort>();
        public Dictionary<NetworkTimer, ushort> towerTemporaryTimers = new Dictionary<NetworkTimer, ushort>();

        public RoomTimers(Room room)
        {
            this.room = room;

            // GameInitTimer
            gameInitTimer = new NetworkTimer
            {
                AutoReset = false
            };
            gameInitTimer.Elapsed += GameInitTimerElapsed;

            // endZoneTimer
            //endZoneTimer = new NetworkTimer
            //{
            //    AutoReset = false
            //};
            //endZoneTimer.Elapsed += EndZoneTimerElapsed;

            //endZoneOvertime = new NetworkTimer
            //{
            //    AutoReset = false
            //};
            //endZoneOvertime.Elapsed += EndZoneOvertimeElapsed;

            // wallTimer
            wallTimer = new NetworkTimer
            {
                AutoReset = false
            };
            wallTimer.Elapsed += WallTimerElapsed;

            // AltarTimer
            altarTimer = new NetworkTimer
            {
                AutoReset = false
            };
            altarTimer.Elapsed += AltarTimerElapsed;

            // SoulTimer
            soulTimer = new NetworkTimer
            {
                AutoReset = false
            };
            soulTimer.Elapsed += SoulTimerElapsed;

            // gameTimer
            gameTimer = new Stopwatch();
            gameTimer.Stop();

            room.TimerCreated();
        }

        public void StopTimersInstantly(bool finalize = false)
        {
            gameInitTimer.Enabled = false;
           // endZoneTimer.Enabled = false;
           // endZoneOvertime.Enabled = false;
            wallTimer.Enabled = false;
            altarTimer.Enabled = false;
            soulTimer.Enabled = false;

            gameTimer.Reset();

            foreach (KeyValuePair<NetworkTimer, ushort> timer in frogTemporaryTimers)
            {
                timer.Key.Elapsed -= (sender, e) => FrogTimerElapsed(sender, e, timer.Value, timer.Key);

                timer.Key.Stop();
                timer.Key.Dispose();
            }

            foreach (KeyValuePair<NetworkTimer, ushort> timer in healthPackTemporaryTimers)
            {
                timer.Key.Elapsed -= (sender, e) => HealthPackTimerElapsed(sender, e, timer.Value, timer.Key);

                timer.Key.Stop();
                timer.Key.Dispose();
            }
            foreach (KeyValuePair<NetworkTimer, ushort> timer in ultPickupTemporaryTimers)
            {
                timer.Key.Elapsed -= (sender, e) => UltPickupTimerElapsed(sender, e, timer.Value, timer.Key);

                timer.Key.Stop();
                timer.Key.Dispose();
            }
            foreach (KeyValuePair<NetworkTimer, ushort> timer in towerTemporaryTimers)
            {
                timer.Key.Elapsed -= (sender, e) => VisionTowerTimerElapsed(sender, e, timer.Value, timer.Key);

                timer.Key.Stop();
                timer.Key.Dispose();
            }

            if (finalize)
            {
                FinalizeTimer();
            }
        }

        private void FinalizeTimer()
        {
            gameInitTimer.Elapsed -= GameInitTimerElapsed;
            wallTimer.Elapsed -= GameInitTimerElapsed;
            altarTimer.Elapsed -= AltarTimerElapsed;
            soulTimer.Elapsed -= SoulTimerElapsed;
          //  endZoneTimer.Elapsed -= EndZoneTimerElapsed;
           // endZoneOvertime.Elapsed -= EndZoneOvertimeElapsed;

            gameInitTimer.Dispose();
          // endZoneTimer.Dispose();
           // endZoneOvertime.Dispose();
            wallTimer.Dispose();
            altarTimer.Dispose();
            soulTimer.Dispose();
        }

        public void StartGameInitTimer(float time = 1000, float wallTime = 1000)
        {
            if (gameInitTimer.Enabled)
            {
                throw new Exception("DEMANDE DE CREATION DE GameInitTimer AVANT LA FIN DU PRECEDENT");
            }

            gameInitTimer.Interval = time;

            gameInitTimer.Enabled = true;

            if (wallTimer.Enabled)
            {
                throw new Exception("DEMANDE DE CREATION DE GameInitTimer AVANT LA FIN DU PRECEDENT");
            }

            wallTimer.Interval = wallTime;

            wallTimer.Enabled = true;
        }

        public double GetGameInitTimerRemainingTime()
        {
            return gameInitTimer.TimeLeft;
        }

        public void GameInitTimerElapsed(Object source, ElapsedEventArgs e)
        {
            room.GameInitTimerElapsed();
            // + Event dans le NetworkTimer
        }


        public double GetWallTimerRemainingTime()
        {
            return wallTimer.TimeLeft;
        }

        public void WallTimerElapsed(Object source, ElapsedEventArgs e)
        {
            room.WallTimerElapsed();
            // + Event dans le NetworkTimer
        }
        //public void StarEndZoneTimer(float time = 60000)
        //{
        //    if (endZoneTimer.Enabled)
        //    {
        //        throw new Exception("DEMANDE DE CREATION DE EndZone AVANT LA FIN DU PRECEDENT");
        //    }

        //    endZoneTimer.Interval = time;

        //    endZoneTimer.Enabled = true;

        //}

        //public double GetEndZoneTimerRemainingTime()
        //{
        //    return endZoneTimer.TimeLeft;
        //}

        //public void EndZoneTimerElapsed(Object source, ElapsedEventArgs e)
        //{

        //    room.EndZoneTimerElapsed();
        //    // + Event dans le NetworkTimer
        //}

        //public void StartEndZoneOvertime(float time = 5000)
        //{
        //    endZoneOvertime.Interval = time;

        //    endZoneOvertime.Enabled = true;


        //}
        //public void PauseEndZoneOvertime(float time = 5000)
        //{
        //    endZoneOvertime.Interval = time;
        //    endZoneOvertime.Enabled = false;

        //}
        //public double GetEndZoneOvertimeRemainingTime()
        //{
        //    return endZoneOvertime.TimeLeft;
        //}

        //public void EndZoneOvertimeElapsed(Object source, ElapsedEventArgs e)
        //{
        //    room.EndZoneOvertimeElapsed();
        //    // + Event dans le NetworkTimer
        //}

        public void StartNewSoulTimer(float time = 1000)
        {
            if (soulTimer.Enabled)
            {
                throw new Exception("DEMANDE DE CREATION DE ALTARTIMER AVANT LA FIN DU PRECEDENT");
            }

            soulTimer.Interval = time;

            soulTimer.Enabled = true;
        }

        public double GetSoulTimerRemainingTime()
        {
            return soulTimer.TimeLeft;
        }

        public void SoulTimerElapsed(Object source, ElapsedEventArgs e)
        {
            room.SoulTimerElapsed();
            // + Event dans le NetworkTimer
        }

        public void StartNewAltarTimer(float time = 1000)
        {
            if (altarTimer.Enabled)
            {
                throw new Exception("DEMANDE DE CREATION DE ALTARTIMER AVANT LA FIN DU PRECEDENT");
            }

            altarTimer.Interval = time;

            altarTimer.Enabled = true;
        }

        public double GetAltarTimerRemainingTime()
        {
            return altarTimer.TimeLeft;
        }

        public void AltarTimerElapsed(Object source, ElapsedEventArgs e)
        {
            room.AltarTimerElapsed();
            // + Event dans le NetworkTimer
        }



        public void StartNewGameStopWatch()
        {
            if (gameTimer.IsRunning)
            {
                throw new Exception("DEMANDE DE CREATION DE GAMETIMER AVANT LA FIN DU PRECEDENT");
            }

            gameTimer.Start();
        }

        public void ResetGameTimer()
        {
            gameTimer.Reset();
        }

        public TimeSpan GetGameStopWatchRemainingTime()
        {
            return gameTimer.Elapsed;
        }


        internal void StartNewFrogTimer(ushort frogID, float time = 1000)
        {
            NetworkTimer newFrogTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = time,
                Enabled = true
            };
            frogTemporaryTimers.Add(newFrogTimer, frogID);

            newFrogTimer.Elapsed += (sender, e) => FrogTimerElapsed(sender, e, frogID,  newFrogTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void FrogTimerElapsed(Object source, ElapsedEventArgs ee, ushort frogID, NetworkTimer newFrogTimer)
        {
            newFrogTimer.Elapsed -= (sender, e) => FrogTimerElapsed(sender, e, frogID, newFrogTimer);
            room.FrogTimerElapsed(frogID);
            frogTemporaryTimers.Remove(newFrogTimer);
            newFrogTimer.Dispose();
        }

        internal void StartNewHealthPackTimer(ushort healthPackID, float time = 1000)
        {
            NetworkTimer newHealthPackTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = time,
                Enabled = true
            };
            healthPackTemporaryTimers.Add(newHealthPackTimer, healthPackID);

            newHealthPackTimer.Elapsed += (sender, e) => HealthPackTimerElapsed(sender, e, healthPackID, newHealthPackTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void HealthPackTimerElapsed(Object source, ElapsedEventArgs ee, ushort healthPackID, NetworkTimer newHealthPackTimer)
        {
            newHealthPackTimer.Elapsed -= (sender, e) => HealthPackTimerElapsed(sender, e, healthPackID, newHealthPackTimer);
            room.HealthPackTimerElapsed(healthPackID);
            healthPackTemporaryTimers.Remove(newHealthPackTimer);
            newHealthPackTimer.Dispose();
        }



        internal void StartNewVisionTowerTimer(ushort iD, int time)
        {
            NetworkTimer newTowerTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = time,
                Enabled = true
            };
            towerTemporaryTimers.Add(newTowerTimer, iD);
            newTowerTimer.Elapsed += (sender, e) => VisionTowerTimerElapsed(sender, e, iD,  newTowerTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void VisionTowerTimerElapsed(Object source, ElapsedEventArgs ee, ushort ID,  NetworkTimer newTowerTimer)
        {
            newTowerTimer.Elapsed -= (sender, e) => VisionTowerTimerElapsed(sender, e, ID, newTowerTimer);
            room.VisionTowerTimerElapsed(ID);
            towerTemporaryTimers.Remove(newTowerTimer);
            newTowerTimer.Dispose();
        }

        internal void StartNewUltPickupTimer(ushort interID, int ultPickupRespawnTime)
        {
            NetworkTimer newUltPickupTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = ultPickupRespawnTime,
                Enabled = true
            };
            ultPickupTemporaryTimers.Add(newUltPickupTimer, interID);

            newUltPickupTimer.Elapsed += (sender, e) => UltPickupTimerElapsed(sender, e, interID, newUltPickupTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void UltPickupTimerElapsed(Object source, ElapsedEventArgs ee, ushort ultPickupID, NetworkTimer newUltPickupTimer)
        {
            newUltPickupTimer.Elapsed -= (sender, e) => UltPickupTimerElapsed(sender, e, ultPickupID, newUltPickupTimer);
            room.UltPickupTimerElapsed(ultPickupID);
            ultPickupTemporaryTimers.Remove(newUltPickupTimer);
            newUltPickupTimer.Dispose();
        }
    }
}
