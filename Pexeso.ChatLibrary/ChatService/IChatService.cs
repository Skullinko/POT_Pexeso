using System.ServiceModel;

namespace Pexeso.ChatLibrary.ChatService
{
    [ServiceContract(CallbackContract = typeof(IChatClient), SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        [OperationContract]
        bool Register(string nick);

        [OperationContract(IsOneWay = true)]
        void Unregister(string nick);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string nick, TextMessage message);

        [OperationContract(IsOneWay = true)]
        void SendMessageToAll(TextMessage message);
    }
}
