using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    class NetworkInteractibleManager
    {

        public BrumeServer brumeServer;

        private static readonly NetworkInteractibleManager instance;
        public static NetworkInteractibleManager Instance { get { return instance; } }

        static NetworkInteractibleManager()
        {
        }

        public NetworkInteractibleManager()
        {
        }

        internal void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {

                if (message.Tag == Tags.UnlockAltar)
                {
                    UnlockAltar(sender, e);
                }               
                else if (message.Tag == Tags.TryCaptureAltar)
                {
                    TryCaptureAltar(sender, e);
                }
                else if (message.Tag == Tags.CaptureProgressAltar)
                {
                    CaptureProgressAltar(sender, e);
                }
                else if (message.Tag == Tags.CaptureAltar)
                {
                    CaptureAltar(sender, e);
                }
            }
        }

        private void UnlockAltar(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _altarID = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        Writer.Write(_altarID);

                        using (Message Message = Message.Create(Tags.UnlockAltar, Writer))
                        {
                            foreach (KeyValuePair<IClient, PlayerData> client in brumeServer.rooms[brumeServer.players[e.Client].RoomID].Players)
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

        private void TryCaptureAltar(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _altarID = reader.ReadUInt16();
                    ushort team = reader.ReadUInt16();
                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        Writer.Write(_altarID);
                        Writer.Write(team);

                        using (Message Message = Message.Create(Tags.TryCaptureAltar, Writer))
                        {
                            foreach (KeyValuePair<IClient, PlayerData> client in brumeServer.rooms[brumeServer.players[e.Client].RoomID].Players)
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

        private void CaptureProgressAltar(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _altarID = reader.ReadUInt16();
                    float progress = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_altarID);
                        Writer.Write(progress);

                        using (Message Message = Message.Create(Tags.CaptureProgressAltar, Writer))
                        {
                            foreach (KeyValuePair<IClient, PlayerData> client in brumeServer.rooms[brumeServer.players[e.Client].RoomID].Players)
                            {
                                if (client.Key != e.Client)
                                {
                                    client.Key.SendMessage(Message, SendMode.Unreliable);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CaptureAltar(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _altarID = reader.ReadUInt16();
                    ushort team = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_altarID);
                        Writer.Write(team);

                        using (Message Message = Message.Create(Tags.CaptureAltar, Writer))
                        {
                            foreach (KeyValuePair<IClient, PlayerData> client in brumeServer.rooms[brumeServer.players[e.Client].RoomID].Players)
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
    }
}
