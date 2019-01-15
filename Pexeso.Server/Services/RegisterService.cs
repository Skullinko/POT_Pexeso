using System.Linq;
using Pexeso.ChatLibrary.RegisterService;
using Pexeso.Database.Model;

namespace Pexeso.Server.Services
{
    class RegisterService : IRegisterService
    {
        public PexesoContext Db { get; set; }

        public RegisterService()
        {
            Db = new PexesoContext();
        }

        ~RegisterService()
        {
            Db.Dispose();
        }

        public bool Register(string nick)
        {
            if (Db.Players.Select(player => player.Nick).Contains(nick))
                return true;

            Db.Players.Add(new Player { Nick = nick });
            Db.SaveChangesAsync();

            return true;
        }
    }
}
