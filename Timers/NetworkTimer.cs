using System;
using System.Timers;

namespace BrumeServer
{
    public class NetworkTimer : Timer
    {
        // Compare le temps actuel du serveur pour généré un temps passé

        private DateTime m_dueTime;

        public double TimeLeft => (this.m_dueTime - DateTime.Now).TotalMilliseconds;

        public NetworkTimer() : base() => this.Elapsed += this.ElapsedAction; // crée un Timer de base et abonne l'event Elapsed a une nouvelle action


        protected new void Dispose()
        {
            this.Elapsed -= this.ElapsedAction;
            base.Dispose();
        }

        public new void Start()
        {
            m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
            base.Start();
        }

        public void ElapsedAction(object sender, ElapsedEventArgs e)
        {
            if (this.AutoReset)
                this.m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
        }
    }
}
