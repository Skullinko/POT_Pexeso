using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Pexeso.Database.Model;
using Player = Pexeso.Web.Models.Player;

namespace Pexeso.Web.Controllers
{
    public class PlayerController : Controller
    {
        // GET: Player
        public ActionResult Index(string sortOrder, string nickFilter)
        {
            ViewBag.NickSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var players = new List<Player>();

            using (var context = new PexesoContext())
            {
                var dbPlayerNicks = context.Players.Select(player => player.Nick).ToList();

                foreach (var dbPlayerNick in dbPlayerNicks)
                {
                    players.Add(new Player(dbPlayerNick));
                }
            }

            if (!string.IsNullOrEmpty(nickFilter))
            {
                players = players.Where(s => s.Nick.Contains(nickFilter)).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    players = players.OrderByDescending(p => p.Nick).ToList();
                    break;
                default:
                    players = players.OrderBy(p => p.Nick).ToList();
                    break;
            }

            return View(players);
        }
    }
}