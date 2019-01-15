using System;
using System.ServiceModel;
using Pexeso.Server.Services;

namespace Pexeso.Server
{
    class Program
    {
        static void Main()
        {
            var registerServiceHost = new ServiceHost(typeof(RegisterService));
            var chatService = new ServiceHost(typeof(ChatService));
            var pexesoServiceHost = new ServiceHost(typeof(PexesoService));

            try
            {
                registerServiceHost.Open();
                chatService.Open();
                pexesoServiceHost.Open();

                Console.WriteLine("The services are ready. Press <ENTER> to terminate services.");
                Console.ReadLine();

                registerServiceHost.Close();
                chatService.Close();
                pexesoServiceHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);

                registerServiceHost.Abort();
                chatService.Abort();
                pexesoServiceHost.Abort();
                throw new Exception();
            }
        }
    }
}
