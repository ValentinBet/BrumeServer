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
                else if (message.Tag == Tags.Sync2DBlendTree)
                {
                    Sync2DBlendTree(sender, e);
                }                
                else if (message.Tag == Tags.Sync2DBlendTree)
                {
                    SyncBoolean(sender, e);
                }                
                else if (message.Tag == Tags.SyncFloat)
                {
                    SyncFloat(sender, e);
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

                    using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        TeamWriter.Write(_id);
                        TeamWriter.Write(trigger);

                        using (Message Message = Message.Create(Tags.SyncTrigger, TeamWriter))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }

        }

        private void Sync2DBlendTree(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _id = reader.ReadUInt16();
                    byte Xvalue = reader.ReadByte();
                    byte Yvalue = reader.ReadByte();

                    using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        TeamWriter.Write(_id);
                        TeamWriter.Write(Xvalue);
                        TeamWriter.Write(Yvalue);

                        using (Message Message = Message.Create(Tags.Sync2DBlendTree, TeamWriter))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                            {
                                if (Factory.CheckSendPlayerAnim(brumeServer.rooms[brumeServer.players[e.Client].Room.ID], client.Value.ID, _id))
                                {
                                    client.Key.SendMessage(Message, e.SendMode);
                                }
                            }     
                        }
                    }
                }
            }

        }

        private void SyncBoolean(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _id = reader.ReadUInt16();
                    string boolean = reader.ReadString();
                    bool value = reader.ReadBoolean();

                    using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        TeamWriter.Write(_id);
                        TeamWriter.Write(boolean);
                        TeamWriter.Write(value);

                        using (Message Message = Message.Create(Tags.Sync2DBlendTree, TeamWriter))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }

        }
        private void SyncFloat(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _id = reader.ReadUInt16();
                    string floatName = reader.ReadString();
                    float value = reader.ReadSingle();

                    using (DarkRiftWriter TeamWriter = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        TeamWriter.Write(_id);
                        TeamWriter.Write(floatName);
                        TeamWriter.Write(value);

                        using (Message Message = Message.Create(Tags.SyncFloat, TeamWriter))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }

        }

      
    }
}
