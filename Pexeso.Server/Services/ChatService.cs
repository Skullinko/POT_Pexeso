using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Pexeso.ChatLibrary;
using Pexeso.ChatLibrary.ChatService;

namespace Pexeso.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class ChatService : IChatService
    {
        private static IChatClient ClientCallback => OperationContext.Current.GetCallbackChannel<IChatClient>();
        public Dictionary<string, IChatClient> ConnectedUsers { get; set; } = new Dictionary<string, IChatClient>();

        public bool Register(string nick)
        {
            if (ConnectedUsers.ContainsKey(nick)) return false;

            ConnectedUsers.Add(nick, ClientCallback);
            return true;
        }

        public void Unregister(string nick)
        {
            SendMessageToAll(new TextMessage {Type = MessageType.DisconnectMessage, SenderNick = nick});
            ConnectedUsers.Remove(nick);
        }

        public void SendMessage(string nick, TextMessage message)
        {
            try
            {
                ConnectedUsers[nick].ReceiveMessage(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);

                if (e is KeyNotFoundException)
                    return;

                Unregister(nick);
            }
        }

        public void SendMessageToAll(TextMessage message)
        {
            var listOfUsers = ConnectedUsers.Values.ToList();

            foreach (var user in listOfUsers)
            {
                try
                {
                    user.ReceiveMessage(message);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);

                    var item = ConnectedUsers.First(pair => pair.Value == user);
                    Unregister(item.Key);
                }
            }
        }
    }
}