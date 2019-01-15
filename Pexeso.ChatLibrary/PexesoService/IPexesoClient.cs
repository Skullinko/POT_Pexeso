using System.ServiceModel;

namespace Pexeso.ChatLibrary.PexesoService
{
    [ServiceContract]
    public interface IPexesoClient
    {
        [OperationContract]
        bool AcceptInvitation(InvitationMessage message);
        
        [OperationContract(IsOneWay = true)]
        void ReceiveGame(InvitationMessage message, bool turn);
        
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(TextMessage textTextMessage);
        
        [OperationContract(IsOneWay = true)]
        void RevealCard(GameMessage message);

        [OperationContract(IsOneWay = true)]
        void GameCancel();
    }
}
