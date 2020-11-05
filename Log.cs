using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 public enum MessageType
{
    None,
    Info,
    Warning,
    Error
}


namespace BrumeServer
{
    public class Log
    {
        /// <summary>
        /// Simpliest Log method
        /// </summary>
        /// <param name="content">Message content of the log</param>
        /// <param name="messageType">Message type</param>
        /// <param name="memberName">Calling class name</param>
        public static void Message(string content, MessageType messageType = MessageType.Info, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {

            switch (messageType)
            {
                case MessageType.None:
                    Console.WriteLine(content);
                    break;
                case MessageType.Info:
                    Console.WriteLine("[Info - " + memberName + " ] |  " + content);
                    break;
                case MessageType.Warning:
                    Console.WriteLine("[Warning - " + memberName + " ] |  " + content);
                    break;
                case MessageType.Error:
                    Console.WriteLine("[ERROR - " + memberName + " ] |  " + content);
                    break;
                default:
                    throw new Exception("Unknown Log type");
            }
        }
    }
}
