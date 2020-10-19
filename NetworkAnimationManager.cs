using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public sealed class NetworkAnimationManager
    {
        public BrumeServer brumeServer;

        private static readonly NetworkAnimationManager instance;
        public static NetworkAnimationManager Instance { get { return instance; } }

        static NetworkAnimationManager()
        {
        }

        public NetworkAnimationManager()
        {
        }

        internal void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.SyncTrigger)
                {
                    SyncTrigger(sender, e);
                }
                else if (message.Tag == Tags.SendAnim)
                {
                    SendAnim(sender, e);
                }
            }
        }

        private void SyncTrigger(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _id = reader.ReadUInt16();
                    string trigger = reader.ReadString();

                    brumeServer.rooms[brumeServer.players[e.Client].RoomID].SyncTrigger(_id, trigger);
                }
            }

        }


        private void SendAnim(object sender, MessageReceivedEventArgs e)
        {
            ushort _roomId;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    _roomId = reader.ReadUInt16();
                    float foward = reader.ReadSingle();
                    float right = reader.ReadSingle();

                    brumeServer.rooms[_roomId].SendAnim(sender, e, foward, right);
                }
            }
        }


    }
}
