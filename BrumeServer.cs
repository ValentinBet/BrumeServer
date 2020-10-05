using DarkRift.Server;
using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public class BrumeServer : Plugin
    {
        public BrumeServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {

        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);
    }
}
