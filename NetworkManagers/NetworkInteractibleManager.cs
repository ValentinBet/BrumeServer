using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.GameData;

namespace BrumeServer
{
    class NetworkInteractibleManager
    {
        public BrumeServer brumeServer;

        private static readonly NetworkInteractibleManager instance;
        public static NetworkInteractibleManager Instance { get { return instance; } }

        static NetworkInteractibleManager() { }

        public NetworkInteractibleManager() { }

        internal void MessageReceivedFromClient(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {

                if (message.Tag == Tags.UnlockInteractible)
                {
                    UnlockInteractible(sender, e);
                }
                else if (message.Tag == Tags.TryCaptureInteractible)
                {
                    TryCaptureInteractible(sender, e);
                }
                else if (message.Tag == Tags.CaptureProgressInteractible)
                {
                    CaptureProgressInteractible(sender, e);
                }
                else if (message.Tag == Tags.CaptureInteractible)
                {
                    CaptureInteractible(sender, e);
                }
                else if (message.Tag == Tags.LaunchWard)
                {
                    LaunchWard(sender, e);
                }
                else if (message.Tag == Tags.StartWardLifeTime)
                {
                    StartWardLifeTime(sender, e);
                }
                else if (message.Tag == Tags.ResurectPlayer)
                {
                    ResurectPlayer(sender, e);
                }
                else if (message.Tag == Tags.AltarTrailDebuff)
                {
                    AltarTrailDebuff(sender, e);
                }
                else if (message.Tag == Tags.LaunchSplouch)
                {
                    LaunchSplouch(sender, e);
                }
            }
        }

        private void UnlockInteractible(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort ID = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                        Writer.Write(ID);

                        using (Message Message = Message.Create(Tags.UnlockInteractible, Writer))
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

        private void TryCaptureInteractible(object sender, MessageReceivedEventArgs e)
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

                        using (Message Message = Message.Create(Tags.TryCaptureInteractible, Writer))
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

        private void CaptureProgressInteractible(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();
                    float progress = reader.ReadSingle();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);
                        Writer.Write(progress);

                        using (Message Message = Message.Create(Tags.CaptureProgressInteractible, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[brumeServer.players[e.Client].Room.ID].Players)
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

        private void CaptureInteractible(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();
                    ushort team = reader.ReadUInt16();
                    InteractibleType type = (InteractibleType)reader.ReadUInt16();

                    switch (type)
                    {
                        case InteractibleType.none:
                            Log.Message("Interactible type == none !", MessageType.Warning);
                            break;
                        case InteractibleType.Altar:
                            brumeServer.rooms[brumeServer.players[e.Client].Room.ID].StartAltarTimer();
                            break;
                        case InteractibleType.VisionTower:
                            brumeServer.rooms[brumeServer.players[e.Client].Room.ID].StartNewVisionTowerTimer(_ID);
                            break;
                        case InteractibleType.Frog:
                            brumeServer.rooms[brumeServer.players[e.Client].Room.ID].StartNewFrogTimer(_ID);
                            break;
                        case InteractibleType.ResurectAltar:

                            break;
                        default:
                            throw new Exception("Interactible type not existing");
                    }

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);
                        Writer.Write(team);

                        using (Message Message = Message.Create(Tags.CaptureInteractible, Writer))
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
        private void LaunchWard ( object sender, MessageReceivedEventArgs e )
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

                        using (Message Message = Message.Create(Tags.LaunchWard, Writer))
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
        private void StartWardLifeTime(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(_ID);


                        using (Message Message = Message.Create(Tags.StartWardLifeTime, Writer))
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
        private void ResurectPlayer(object sender, MessageReceivedEventArgs e)
        {
            ushort[] _IDList;
            ushort roomID = 0;
            
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    roomID = reader.ReadUInt16();
                    _IDList = reader.ReadUInt16s();
                }
            }

            for (int i = 0; i < _IDList.Length; i++)
            {
                brumeServer.rooms[roomID].SpawnObjPlayer(_IDList[i], true);
            }
        }

        private void AltarTrailDebuff(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();

                    Room _room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    ushort? _targetId = _room.GetPlayerCharacterInTeam(Factory.GetOtherTeam(brumeServer.players[e.Client].playerTeam), Character.shili);

                    if (_targetId == null)
                    {
                        return;
                    }

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write((ushort)_targetId);

                        using (Message Message = Message.Create(Tags.AltarTrailDebuff, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in _room.Players)
                            {
                                    client.Key.SendMessage(Message, SendMode.Unreliable);
                            }
                        }
                    }
                }
            }
        }

        private void LaunchSplouch ( object sender, MessageReceivedEventArgs e )
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

                        using (Message Message = Message.Create(Tags.LaunchSplouch, Writer))
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
    }
}
