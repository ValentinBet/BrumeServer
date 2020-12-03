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
    public sealed class NetworkObjectsManager
    {
        public BrumeServer brumeServer;

        private static readonly NetworkObjectsManager instance;
        public static NetworkObjectsManager Instance { get { return instance; } }

        static NetworkObjectsManager()
        {
        }

        public NetworkObjectsManager()
        {
        }

        public ushort lastObjUniqueID = 0;

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
                else if (message.Tag == Tags.SpawnGenericFx)
                {
                    SpawnGenericFx(sender, e);
                }
                else if (message.Tag == Tags.SpawnAOEFx)
                {
                    SpawnAOEFx(sender, e);
                }
                else if (message.Tag == Tags.Play2DSound)
                {
                    Play2DSound(sender, e);
                }
                else if (message.Tag == Tags.Play3DSound)
                {
                    Play3DSound(sender, e);
                }
            }
        }

        private void Play2DSound(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _index = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_index);

                        using (Message Message = Message.Create(Tags.Play2DSound, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }
        }

        private void Play3DSound(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _index = reader.ReadUInt16();

                    float _posX = reader.ReadSingle();
                    float _posY = reader.ReadSingle();
                    float _posZ = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_index);

                        Writer.Write(_posX);
                        Writer.Write(_posY);
                        Writer.Write(_posZ);

                        using (Message Message = Message.Create(Tags.Play3DSound, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }
        }

        private void SpawnGenericFx(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _index = reader.ReadUInt16();
                    float _posX = reader.ReadSingle();
                    float _posZ = reader.ReadSingle();

                    float _rota= reader.ReadSingle();
                    float _scale = reader.ReadSingle();
                    float _time = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_index);
                        Writer.Write(_posX);
                        Writer.Write(_posZ);
                        Writer.Write(_rota);
                        Writer.Write(_scale);
                        Writer.Write(_time);

                        using (Message Message = Message.Create(Tags.SpawnGenericFx, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
                }
            }

        }

        private void SpawnAOEFx(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _id = reader.ReadUInt16();
                    float _posX = reader.ReadSingle();
                    float _posZ = reader.ReadSingle();

                    float _rota = reader.ReadSingle();
                    float _scale = reader.ReadSingle();
                    float _time = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_id);
                        Writer.Write(_posX);
                        Writer.Write(_posZ);
                        Writer.Write(_rota);
                        Writer.Write(_scale);
                        Writer.Write(_time);

                        using (Message Message = Message.Create(Tags.SpawnAOEFx, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players.Where(x => x.Key != e.Client))
                                client.Key.SendMessage(Message, e.SendMode);
                        }
                    }
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
                    ushort _uniqueID = reader.ReadUInt16();

                    float _ObjectPosx = reader.ReadSingle();
                    float _ObjectPosz = reader.ReadSingle();

                    float _ObjectRotationx = reader.ReadSingle();
                    float _ObjectRotationy = reader.ReadSingle();
                    float _ObjectRotationz = reader.ReadSingle();

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(_ownerID);
                        writer.Write(_objectID);
                        writer.Write(_uniqueID);

                        writer.Write(_ObjectPosx);
                        writer.Write(_ObjectPosz);

                        writer.Write(_ObjectRotationx);
                        writer.Write(_ObjectRotationy);
                        writer.Write(_ObjectRotationz);


                        message.Serialize(writer);
                        using (Message MessageW = Message.Create(Tags.InstantiateObject, writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                            {
                                if (client.Key.ID == e.Client.ID)
                                {
                                    continue;
                                }
                                client.Key.SendMessage(MessageW, SendMode.Reliable);
                                
                            }

                        }
                    }
                }
            }
        }

        public ushort GenerateUniqueObjID()
        {
            int _temp = 0;
            lastObjUniqueID++;
            _temp = (lastObjUniqueID * 10);

            return (ushort)_temp;
        }

        public void ServerInstantiateObject(IClient Iclient, ushort objectID, Vector3 objPos, Vector3 objRot)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(Iclient.ID);
                writer.Write(objectID);
                writer.Write(GenerateUniqueObjID());

                writer.Write(objPos.X);
                writer.Write(objPos.Z);

                writer.Write(objRot.X);
                writer.Write(objRot.Y);
                writer.Write(objRot.Z);

                using (Message MessageW = Message.Create(Tags.InstantiateObject, writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[Iclient].Room.ID].Players)
                    {
 
                        client.Key.SendMessage(MessageW, SendMode.Reliable);
                    }

                }
            }
        }

        private void SynchroniseObject(object sender, MessageReceivedEventArgs e)
        {
            float _ObjectPosx = 0;
            float _ObjectPosz = 0;
            float _ObjectRotationy = 0;

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _serverID = reader.ReadUInt16();

                    bool _syncPos = reader.ReadBoolean();

                    if (_syncPos)
                    {
                        _ObjectPosx = reader.ReadSingle();
                        _ObjectPosz = reader.ReadSingle();
                    }

                    bool _syncRot = reader.ReadBoolean();

                    if (_syncRot)
                    {
                        _ObjectRotationy = reader.ReadSingle();
                    }

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(_serverID);

                        writer.Write(_syncPos);

                        if (_syncPos)
                        {
                            writer.Write(_ObjectPosx);
                            writer.Write(_ObjectPosz);
                        }

                        writer.Write(_syncRot);

                        if (_syncRot)
                        {
                            writer.Write(_ObjectRotationy);
    
                        }
                        message.Serialize(writer);
                        using (Message MessageW = Message.Create(Tags.SynchroniseObject, writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
                            {
                                if (client.Key.ID == e.Client.ID)
                                {
                                    continue;
                                }

                                client.Key.SendMessage(MessageW, SendMode.Unreliable);
                            }
 
                        }
                    }
                }
            }
        }


    }
}
