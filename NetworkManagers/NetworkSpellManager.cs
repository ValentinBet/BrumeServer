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

        private static NetworkSpellManager instance;
        public static NetworkSpellManager Instance { get { return instance; } }

        static NetworkSpellManager()
        {
        }

        public NetworkSpellManager()
        {
            instance = this;
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
                    ChangeFowSize(sender, e);
                }
                else if (message.Tag == Tags.ForceFowSize)
                {
                    ForceFowSize(sender, e);
                }                
                else if (message.Tag == Tags.SpellStep)
                {
                    SpellStep(sender, e);
                }
                else if (message.Tag == Tags.Tp)
                {
                    TpInServer(sender, e);
                }
            }
        }

        private void TpInServer(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    bool _tp = reader.ReadBoolean();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(e.Client.ID);
                        Writer.Write(_tp);

                        using (Message Message = Message.Create(Tags.Tp, Writer))
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

        private void SpellStep(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _spellIndex = reader.ReadUInt16();
                    ushort _spellStep = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(e.Client.ID);
                        Writer.Write(_spellIndex);
                        Writer.Write(_spellStep);

                        using (Message Message = Message.Create(Tags.SpellStep, Writer))
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
                    ushort _ID = e.Client.ID;
                    uint _size = reader.ReadUInt32();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);
                        Writer.Write(_size);

                        using (Message Message = Message.Create(Tags.ChangeFowSize, Writer))
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

        private void ForceFowSize(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = e.Client.ID;
                    uint _size = reader.ReadUInt32();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);
                        Writer.Write(_size);

                        using (Message Message = Message.Create(Tags.ForceFowSize, Writer))
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
