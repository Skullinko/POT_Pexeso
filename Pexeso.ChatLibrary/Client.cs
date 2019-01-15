using System.ServiceModel;
using Pexeso.ChatLibrary.ChatService;
using Pexeso.ChatLibrary.Model;
using Pexeso.ChatLibrary.PexesoService;
using Pexeso.ChatLibrary.RegisterService;

namespace Pexeso.ChatLibrary
{
    public class Client : IChatClient, IPexesoClient
    {
        public string Nick { get; set; }

        private readonly IRegisterService _registerChannel;
        private readonly IChatService _chatChannel;
        private readonly IPexesoService _pexesoChannel;

        public delegate void RefreshIcomeHandler(string nick);

        public delegate void TextMessageIcomeHandler(TextMessage msg);

        public delegate bool InvitationMessageIcomeHandler(InvitationMessage msg);

        public delegate void ReceiveGameHandler(InvitationMessage message, bool turn);

        public delegate void RevealCardHandler(GameMessage message);

        public delegate void GameOverHandler();

        public event RefreshIcomeHandler UserConnectedEvent;
        public event RefreshIcomeHandler UserDisconnectedEvent;
        public event RefreshIcomeHandler PlayerStartedEvent;
        public event RefreshIcomeHandler PlayerFinishedEvent;

        public event TextMessageIcomeHandler TextMessageIncomeEvent;

        public event InvitationMessageIcomeHandler InvitationMessageIncomeEvent;
        public event ReceiveGameHandler ReceiveGameEvent;
        public event RevealCardHandler RevealCardEvent;
        public event GameOverHandler GameCancelEvent;

        public Client()
        {
            var registerChannelFactory = new ChannelFactory<IRegisterService>("MyRegisterTcpEndpoint");
            _registerChannel = registerChannelFactory.CreateChannel();

            var chatChannelFactory = new DuplexChannelFactory<IChatService>(this, "MyChatTcpEndpoint");
            _chatChannel = chatChannelFactory.CreateChannel();

            var pexesoChannelFactory = new DuplexChannelFactory<IPexesoService>(this, "MyPexesoTcpEndpoint");
            _pexesoChannel = pexesoChannelFactory.CreateChannel();
        }

        public bool Register(string nick)
        {
            Nick = nick;

            return _registerChannel.Register(nick) && _chatChannel.Register(nick) && _pexesoChannel.Register(nick);
        }

        public void Unregister()
        {
            _chatChannel.Unregister(Nick);
            _pexesoChannel.Unregister(Nick);
        }

        public void SendRegisterMessage()
        {
            _chatChannel.SendMessageToAll(new TextMessage {SenderNick = Nick, Type = MessageType.ConnectMessage});
        }

        public void SendMessage(string otherNick, string message)
        {
            var msg = new TextMessage()
            {
                Type = MessageType.TextMessage,
                SenderNick = Nick,
                ReceiverNick = otherNick,
                Content = message,
            };

            _chatChannel.SendMessage(otherNick, msg);
        }

        public void ReceiveMessage(TextMessage textTextMessage)
        {
            switch (textTextMessage.Type)
            {
                case MessageType.TextMessage:
                    TextMessageIncomeEvent?.Invoke(textTextMessage);
                    break;
                case MessageType.ConnectMessage:
                    if (textTextMessage.SenderNick != Nick)
                    {
                        UserConnectedEvent?.Invoke(textTextMessage.SenderNick);
                        _chatChannel.SendMessage(textTextMessage.SenderNick,
                            new TextMessage() {SenderNick = Nick, Type = MessageType.LoadMessage});
                    }

                    break;
                case MessageType.LoadMessage:
                    UserConnectedEvent?.Invoke(textTextMessage.SenderNick);
                    break;
                case MessageType.DisconnectMessage:
                    if (textTextMessage.SenderNick != Nick)
                    {
                        UserDisconnectedEvent?.Invoke(textTextMessage.SenderNick);
                    }
                    break;
                case MessageType.PlayerStartedMessage:
                    if (textTextMessage.SenderNick != Nick)
                    {
                        PlayerStartedEvent?.Invoke(textTextMessage.SenderNick);
                    }
                    break;
                case MessageType.PlayerFinishedMessage:
                    if (textTextMessage.SenderNick != Nick)
                    {
                        PlayerFinishedEvent?.Invoke(textTextMessage.SenderNick);
                    }
                    break;
            }
        }

        public bool InvitePlayer(GameSize gameSize, string otherNick)
        {
            if (Nick == otherNick)
                return false;

            var msg = new InvitationMessage()
            {
                SenderNick = Nick,
                ReceiverNick = otherNick,
                GameSize = gameSize
            };

            _pexesoChannel.InvitePlayer(msg);
            return true;
        }

        public bool InviteRandomPlayer(GameSize gameSize)
        {
            return _pexesoChannel.GetRandomAvailablePlayer(Nick, out var otherNick) && InvitePlayer(gameSize, otherNick);
        }

        public bool AcceptInvitation(InvitationMessage message)
        {
            return InvitationMessageIncomeEvent?.Invoke(message) ?? false;
        }

        public void ReceiveGame(InvitationMessage message, bool turn)
        {
            ReceiveGameEvent?.Invoke(message, turn);
        }

        public void CardRevealed(GameMessage message)
        {
            _pexesoChannel.CardRevealed(message);
        }

        public void RevealCard(GameMessage message)
        {
            RevealCardEvent?.Invoke(message);
        }

        public void SendGameData(GameMessage message)
        {
            _pexesoChannel.ReceiveGameData(message);
        }

        public void SendGameCancel(GameMessage message)
        {
            _pexesoChannel.ReceiveGameCancel(message);
        }

        public void GameCancel()
        {
            GameCancelEvent?.Invoke();
        }
    }
}