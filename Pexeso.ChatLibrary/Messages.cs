using System.Collections.Generic;
using System.Runtime.Serialization;
using Pexeso.ChatLibrary.Model;

namespace Pexeso.ChatLibrary
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string SenderNick { get; set; }

        [DataMember]
        public string ReceiverNick { get; set; }
    }
    
    [DataContract]
    public class TextMessage : Message                  
    {
        [DataMember]
        public MessageType Type { get; set; }

        [DataMember]
        public string Content { get; set; }
    }

    [DataContract]
    public enum MessageType
    {
        [EnumMember] TextMessage,
        [EnumMember] ConnectMessage,
        [EnumMember] LoadMessage,
        [EnumMember] DisconnectMessage,
        [EnumMember] PlayerStartedMessage,
        [EnumMember] PlayerFinishedMessage
    }

    [DataContract]
    public class InvitationMessage : Message
    {
        [DataMember]
        public int GameId { get; set; }

        [DataMember]
        public GameSize GameSize { get; set; }

        [DataMember]
        public List<char> GameCards { get; set; }
    }

    [DataContract]
    public class GameMessage : Message
    {
        [DataMember]
        public int GameId { get; set; }
        [DataMember]
        public int Row { get; set; }
        [DataMember]
        public int Column { get; set; }
        [DataMember]
        public string WinnerNick { get; set; }
    }
}