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
        public Stopwatch gameTimer;

        public Dictionary<NetworkTimer, ushort> frogTemporaryTimers = new Dictionary<NetworkTimer, ushort>();
        public Dictionary<NetworkTimer, ushort> towerTemporaryTimers = new Dictionary<NetworkTimer, ushort>();

        public RoomTimers(Room room)
        {
            this.room = room;

            // AltarTimer
            altarTimer = new NetworkTimer
            {
                AutoReset = false
            };
            altarTimer.Elapsed += AltarTimerElapsed;

            // gameTimer
            gameTimer = new Stopwatch();
            gameTimer.Stop();

            room.TimerCreated();
        }

        public void StopTimersInstantly(bool finalize = false)
        {
            altarTimer.Elapsed -= AltarTimerElapsed;
            gameTimer.Reset();
            altarTimer.Enabled = false;

            foreach (KeyValuePair<NetworkTimer, ushort> timer in frogTemporaryTimers)
            {
                timer.Key.Elapsed -= (sender, e) => FrogTimerElapsed(sender, e, timer.Value, timer.Key);

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
            altarTimer.Dispose();
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
            newFrogTimer.Elapsed -= (sender, e) => VisionTowerTimerElapsed(sender, e, frogID, newFrogTimer);
            room.FrogTimerElapsed(frogID);
            frogTemporaryTimers.Remove(newFrogTimer);
            newFrogTimer.Dispose();
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
    }
}
