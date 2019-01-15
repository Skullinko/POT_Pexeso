using System.ServiceModel;

namespace Pexeso.ChatLibrary.PexesoService
{
    [ServiceContract(CallbackContract = typeof(IPexesoClient), SessionMode = SessionMode.Required)]
    public interface IPexesoService
    {
        [OperationContract]
        bool Register(string nick);

        [OperationContract(IsOneWay = true)]
        void Unregister(string nick);
        
        [OperationContract]
        bool GetRandomAvailablePlayer(string myNick, out string otherNick);

        [OperationContract(IsOneWay = true)]
        void InvitePlayer(InvitationMessage message);
        
        [OperationContract(IsOneWay = true)]
        void CardRevealed(GameMessage message);
        
        [OperationContract(IsOneWay = true)]
        void ReceiveGameData(GameMessage message);

        [OperationContract(IsOneWay = true)]
        void ReceiveGameCancel(GameMessage message);
    }
}
