using Pexeso.ChatLibrary.PexesoService;

namespace Pexeso.Server
{
    public class PlayingClient
    {
        public IPexesoClient Client { get; set; }

        public bool IsPlaying { get; set; }

        public PlayingClient(IPexesoClient client)
        {
            Client = client;
            IsPlaying = false;
        }
    }
}
