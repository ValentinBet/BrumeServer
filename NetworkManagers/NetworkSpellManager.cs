using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public sealed class NetworkSpellManager
    {
        public BrumeServer brumeServer;

        private static readonly NetworkSpellManager instance;
        public static NetworkSpellManager Instance { get { return instance; } }

        static NetworkSpellManager()
        {
        }

        public NetworkSpellManager()
        {
        }

        internal void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.CurveSpellLaunch)
                {
                    CurveSpellLaunch(sender, e);
                }
                else if (message.Tag == Tags.CurveSpellLanded)
                {
                    CurveSpellLanded(sender, e);
                }
                else if (message.Tag == Tags.ChangeFowSize)
                {
                    CurveSpellLanded(sender, e);
                }
            }
        }

        private void CurveSpellLaunch(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();

                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);

                        Writer.Write(x);
                        Writer.Write(y);
                        Writer.Write(z);

                        using (Message Message = Message.Create(Tags.CurveSpellLaunch, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                            {
                                if (client.Key != e.Client)
                                {
                                    client.Key.SendMessage(Message, SendMode.Reliable);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CurveSpellLanded(object sender, MessageReceivedEventArgs e)
        { 
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);

                        using (Message Message = Message.Create(Tags.CurveSpellLanded, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                            {
                                if (client.Key != e.Client)
                                {
                                    client.Key.SendMessage(Message, SendMode.Reliable);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ChangeFowSize(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();
                    uint _size = reader.ReadUInt32();
                    bool _reset = reader.ReadBoolean();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);
                        Writer.Write(_size);
                        Writer.Write(_reset);

                        using (Message Message = Message.Create(Tags.CurveSpellLanded, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].GetPlayerListInTeam(brumeServer.players[e.Client].playerTeam))
                            {
                                if (client.Key == e.Client)
                                    continue;

                                client.Key.SendMessage(Message, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }
    }
}
