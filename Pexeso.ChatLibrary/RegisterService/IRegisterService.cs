using System.ServiceModel;

namespace Pexeso.ChatLibrary.RegisterService
{
    [ServiceContract]
    public interface IRegisterService
    {
        [OperationContract]
        bool Register(string nick);
    }
}
