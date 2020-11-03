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



    }
}
