using DarkRift.Server;
using System;
using System.Collections.Generic;
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
        public NetworkTimer gameTimer;

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
            gameTimer = new NetworkTimer
            {
                AutoReset = false
            };
            gameTimer.Elapsed += GameTimerElapsed;

            room.TimerCreated();
        }

        public void StopTimersInstantly()
        {
            altarTimer.Elapsed -= AltarTimerElapsed;
            gameTimer.Elapsed -= GameTimerElapsed;
            altarTimer.Enabled = false;
            gameTimer.Enabled = false;
            altarTimer.Dispose();
            gameTimer.Dispose();
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

        public void StartNewGameTimer(float time = 1000)
        {
            if (gameTimer.Enabled)
            {
                throw new Exception("DEMANDE DE CREATION DE GAMETIMER AVANT LA FIN DU PRECEDENT");
            }

            gameTimer.Interval = time;
            gameTimer.Start();

        }

        public double GetGameTimerRemainingTime()
        {
            return gameTimer.TimeLeft;
        }

        public void GameTimerElapsed(Object source, ElapsedEventArgs e)
        {
            room.GameTimerElapsed();
            // + Event dans le NetworkTimer
        }

        internal void StartNewFrogTimer(ushort frogID, float time = 1000)
        {
            NetworkTimer newFrogTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = time,
                Enabled = true
            };

            newFrogTimer.Elapsed += (sender, e) => FrogTimerElapsed(sender, e, frogID, ref newFrogTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void FrogTimerElapsed(Object source, ElapsedEventArgs e, ushort frogID, ref NetworkTimer newFrogTime)
        {
            room.FrogTimerElapsed(frogID);
            newFrogTime.Dispose();
        }

        internal void StartNewVisionTowerTimer(ushort iD, int time)
        {
            NetworkTimer newTowerTimer = new NetworkTimer
            {
                AutoReset = false,
                Interval = time,
                Enabled = true
            };

            newTowerTimer.Elapsed += (sender, e) => VisionTowerTimerElapsed(sender, e, iD, ref newTowerTimer); // https://stackoverflow.com/questions/9977393/how-do-i-pass-an-object-into-a-timer-event
        }

        private void VisionTowerTimerElapsed(Object source, ElapsedEventArgs e, ushort frogID, ref NetworkTimer newFrogTime)
        {
            room.VisionTowerTimerElapsed(frogID);
            newFrogTime.Dispose();
        }
    }
}
