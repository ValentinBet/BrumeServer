using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrumeServer
{
    public sealed class NetworkObjectsManager
    {
        private ushort LastNetworkedObjectID = 0;

        public BrumeServer brumeServer;

        private static readonly NetworkObjectsManager instance;
        public static NetworkObjectsManager Instance { get { return instance; } }

        static NetworkObjectsManager()
        {
        }

        public NetworkObjectsManager()
        {
        }

        internal void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.InstantiateObject)
                {
                    InstantiateObject(sender, e);
                }
                else if (message.Tag == Tags.SynchroniseObject)
                {
                    SynchroniseObject(sender, e);
                }
                else if (message.Tag == Tags.DestroyObject)
                {
                    DestroyObject(sender, e);
                }

            }
        }

        private void DestroyObject(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _objID = reader.ReadUInt16();

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(_objID);

                        using (Message MessageW = Message.Create(Tags.DestroyObject, writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                                client.Key.SendMessage(MessageW, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        private void InstantiateObject(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ownerID = reader.ReadUInt16();
                    ushort _objectID = reader.ReadUInt16();

                    float _ObjectPosx = reader.ReadSingle();
                    float _ObjectPosy = reader.ReadSingle();
                    float _ObjectPosz = reader.ReadSingle();

                    float _ObjectRotationx = reader.ReadSingle();
                    float _ObjectRotationy = reader.ReadSingle();
                    float _ObjectRotationz = reader.ReadSingle();

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(_ownerID);
                        writer.Write(_objectID);

                        writer.Write(_ObjectPosx);
                        writer.Write(_ObjectPosy);
                        writer.Write(_ObjectPosz);

                        writer.Write(_ObjectRotationx);
                        writer.Write(_ObjectRotationy);
                        writer.Write(_ObjectRotationz);

                        LastNetworkedObjectID += 1; // UNIQUE ID
                        writer.Write(LastNetworkedObjectID);

                        message.Serialize(writer);
                        using (Message MessageW = Message.Create(Tags.InstantiateObject, writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                                client.Key.SendMessage(MessageW, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        private void SynchroniseObject(object sender, MessageReceivedEventArgs e)
        {
            float _ObjectRotationx = 0;
            float _ObjectRotationy = 0;
            float _ObjectRotationz = 0;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _serverID = reader.ReadUInt16();

                    float _ObjectPosx = reader.ReadSingle();
                    float _ObjectPosy = reader.ReadSingle();
                    float _ObjectPosz = reader.ReadSingle();

                    bool _syncRot = reader.ReadBoolean();

                    if (_syncRot)
                    {
                        _ObjectRotationx = reader.ReadSingle();
                        _ObjectRotationy = reader.ReadSingle();
                        _ObjectRotationz = reader.ReadSingle();
                    }

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(_serverID);

                        writer.Write(_ObjectPosx);
                        writer.Write(_ObjectPosy);
                        writer.Write(_ObjectPosz);

                        writer.Write(_syncRot);

                        if (_syncRot)
                        {
                            writer.Write(_ObjectRotationx);
                            writer.Write(_ObjectRotationy);
                            writer.Write(_ObjectRotationz);
                        }
                        message.Serialize(writer);
                        using (Message MessageW = Message.Create(Tags.SynchroniseObject, writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(MessageW, SendMode.Unreliable);
                        }
                    }
                }
            }
        }


    }
}
