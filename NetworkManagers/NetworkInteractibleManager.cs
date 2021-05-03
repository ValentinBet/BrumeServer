using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static BrumeServer.ServerData;
namespace BrumeServer
{
    class NetworkInteractibleManager
    {
        public BrumeServer brumeServer;

        private static NetworkInteractibleManager instance;
        public static NetworkInteractibleManager Instance { get { return instance; } }
        static NetworkInteractibleManager() { }

        public NetworkInteractibleManager()
        {
            instance = this;
        }

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
                    TryCaptureInteractibleReceiver(sender, e);
                }
                else if (message.Tag == Tags.QuitInteractibleZone)
                {
                    QuitInteractibleZone(sender, e);
                }
                else if (message.Tag == Tags.CaptureProgressInteractible)
                {
                    CaptureProgressInteractible(sender, e);
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
                else if (message.Tag == Tags.AltarSpeedBuff)
                {
                    AltarSpeedBuff(sender, e);
                }
                else if (message.Tag == Tags.AltarPoisonBuff)
                {
                    AltarPoisonBuff(sender, e);
                }
                else if (message.Tag == Tags.AltarOutlineBuff)
                {
                    AltarOutlineBuff(sender, e);
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

        private void TryCaptureInteractibleReceiver(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _interID = reader.ReadUInt16();
                    ushort team = reader.ReadUInt16();
                    InteractibleType type = (InteractibleType)reader.ReadUInt16();
                    Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Room room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    room.TryCaptureNewInteractible(_interID, team, pos, e.Client.ID, type);
                }
            }
        }



        public void TryCaptureInteractible(ushort _interID, Player player)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                Writer.Write(_interID);
                Writer.Write(player.ID);

                Room room = brumeServer.rooms[player.Room.ID];

                using (Message Message = Message.Create(Tags.TryCaptureInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in room.Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }

        public void StopCapturing(ushort _interID, Room room)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                // Recu par les joueurs déja présent dans la room SAUF LENVOYEUR

                Writer.Write(_interID);

                using (Message Message = Message.Create(Tags.StopCaptureInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in room.Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
        }



        private void QuitInteractibleZone(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _interID = reader.ReadUInt16();

                    Room room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    room.QuitInteractibleZone(_interID, e.Client.ID);
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

                    Room room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    room.InteractibleCaptureProgress(_ID, progress);


                }
            }
        }



        public void SendInteractibleProgress(ushort _interID, ushort _senderID, float totalProgress, Room room)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(_interID);
                Writer.Write(totalProgress);

                using (Message Message = Message.Create(Tags.CaptureProgressInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in room.Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Unreliable);
                    }
                }
            }
        }


        public void CaptureInteractible(ushort _interID, Player _capturingPlayer, InteractibleType type, Room room)
        {
            switch (type)
            {
                case InteractibleType.none:
                    Log.Message("Interactible type == none !", MessageType.Warning);
                    break;
                case InteractibleType.Altar:
                    room.AltarCaptured(_capturingPlayer.playerTeam, _interID);
                    break;
                case InteractibleType.VisionTower:
                    room.StartNewVisionTowerTimer(_interID);
                    break;
                case InteractibleType.Frog:
                    room.StartNewFrogTimer(_interID);
                    break;
                case InteractibleType.ResurectAltar:
                    break;
                case InteractibleType.HealthPack:
                    room.StartHealthPackTimer(_interID);
                    break;
                case InteractibleType.UltPickup:
                    room.StartUltPickupTimer(_interID);
                    break;
                case InteractibleType.EndZone:
                    room.EndZoneCaptured(_capturingPlayer.playerTeam);
                    break;
                default:
                    throw new Exception("Interactible type not existing");
            }

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(_interID);
                Writer.Write(_capturingPlayer.ID);

                using (Message Message = Message.Create(Tags.CaptureInteractible, Writer))
                {
                    foreach (KeyValuePair<IClient, Player> client in brumeServer.rooms[_capturingPlayer.Room.ID].Players)
                    {
                        client.Key.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }


        }

        private void LaunchWard(object sender, MessageReceivedEventArgs e)
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

                    ushort? _targetId = _room.GetPlayerCharacterInTeam(Factory.GetOtherTeam(brumeServer.players[e.Client].playerTeam), Character.WuXin);

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

        private void AltarOutlineBuff(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort _ID = reader.ReadUInt16();

                    Room _room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    ushort? _targetId = _room.GetPlayerCharacterInTeam(Factory.GetOtherTeam(brumeServer.players[e.Client].playerTeam), Character.WuXin);

                    if (_targetId == null)
                    {
                        return;
                    }

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write((ushort)_targetId);

                        using (Message Message = Message.Create(Tags.AltarOutlineBuff, Writer))
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

        private void AltarSpeedBuff(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Team _team = (Team)reader.ReadUInt16();

                    Room _room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    Dictionary<IClient, Player> _temp = _room.GetPlayerListInTeam(_team);

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        using (Message Message = Message.Create(Tags.AltarSpeedBuff, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in _room.Players)
                            {
                                if (_temp.ContainsKey(client.Key))
                                {
                                    client.Key.SendMessage(Message, SendMode.Reliable);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void AltarPoisonBuff(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Team _team = (Team)reader.ReadUInt16();

                    Room _room = brumeServer.rooms[brumeServer.players[e.Client].Room.ID];

                    Dictionary<IClient, Player> _temp = _room.GetPlayerListInTeam(_team);

                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        using (Message Message = Message.Create(Tags.AltarPoisonBuff, Writer))
                        {
                            foreach (KeyValuePair<IClient, Player> client in _room.Players)
                            {
                                if (_temp.ContainsKey(client.Key))
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
