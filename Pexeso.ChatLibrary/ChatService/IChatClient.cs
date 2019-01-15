using System.ServiceModel;

namespace Pexeso.ChatLibrary.ChatService
{
    [ServiceContract]
    public interface IChatClient
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(TextMessage textTextMessage);
    }
}
