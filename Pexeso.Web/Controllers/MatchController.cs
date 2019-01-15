using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Pexeso.Database.Model;
using Game = Pexeso.Web.Models.Game;
using Match = Pexeso.Web.Models.Match;
using Player = Pexeso.Web.Models.Player;

namespace Pexeso.Web.Controllers
{
    public class MatchController : Controller
    {
        // GET: Match
        public ActionResult Index(string sortOrder, string gameIdFilter, string gameSizeFilter, string nickFilter,
            string resultFilter)
        {
            ViewBag.GameIdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.GameSizeSortParm = sortOrder == "GameSize" ? "gameSize_desc" : "GameSize";
            ViewBag.PlayerNickSortParm = sortOrder == "PlayerNick" ? "playerNick_desc" : "PlayerNick";
            ViewBag.ResultSortParm = sortOrder == "Result" ? "result_desc" : "Result";
            ViewBag.NumberOfMovesSortParm = sortOrder == "NumberOfMoves" ? "numberOfMoves_desc" : "NumberOfMoves";
            ViewBag.DurationSortParm = sortOrder == "Duration" ? "duration_desc" : "Duration";

            var matches = new List<Match>();

            using (var context = new PexesoContext())
            {
                var dbMatches = context.Matches.Select(match => match).Include(match => match.Game).ToList();

                foreach (var dbMatch in dbMatches)
                {
                    matches.Add(new Match
                    {
                        Player = new Player(dbMatch.PlayerNick),
                        Game = new Game
                        {
                            Id = dbMatch.Game.Id,
                            GameSize = dbMatch.Game.GameSize,
                            Duration = dbMatch.Game.Duration
                        },
                        Result = dbMatch.Result,
                        NumberOfMoves = dbMatch.NumberOfMoves
                    });
                }
            }

            matches = matches.Where(m =>
                (string.IsNullOrEmpty(gameIdFilter) ||
                 (m.Game.Id.ToString().ToLower() == gameIdFilter.Trim().ToLower())) &&
                (string.IsNullOrEmpty(gameSizeFilter) ||
                 m.Game.GameSize.ToString().ToLower().Contains(gameSizeFilter.Trim().ToLower())) &&
                (string.IsNullOrEmpty(nickFilter) || m.Player.Nick.ToLower().Contains(nickFilter.Trim().ToLower())) &&
                (string.IsNullOrEmpty(resultFilter) ||
                 m.Result.ToString().ToLower().Contains(resultFilter.Trim().ToLower()))).ToList();

            switch (sortOrder)
            {
                case "id_desc":
                    matches = matches.OrderByDescending(m => m.Game.Id).ToList();
                    break;
                case "GameSize":
                    matches = matches.OrderBy(m => m.Game.GameSize).ToList();
                    break;
                case "gameSize_desc":
                    matches = matches.OrderByDescending(m => m.Game.GameSize).ToList();
                    break;
                case "PlayerNick":
                    matches = matches.OrderBy(m => m.Player.Nick).ToList();
                    break;
                case "playerNick_desc":
                    matches = matches.OrderByDescending(m => m.Player.Nick).ToList();
                    break;
                case "Result":
                    matches = matches.OrderBy(m => m.Result).ToList();
                    break;
                case "result_desc":
                    matches = matches.OrderByDescending(m => m.Result).ToList();
                    break;
                case "NumberOfMoves":
                    matches = matches.OrderBy(m => m.NumberOfMoves).ToList();
                    break;
                case "numberOfMoves_desc":
                    matches = matches.OrderByDescending(m => m.NumberOfMoves).ToList();
                    break;
                case "Duration":
                    matches = matches.OrderBy(m => m.Game.Duration).ToList();
                    break;
                case "duration_desc":
                    matches = matches.OrderByDescending(m => m.Game.Duration).ToList();
                    break;
                default:
                    matches = matches.OrderBy(m => m.Game.Id).ToList();
                    break;
            }

            return View(matches);
        }
    }
}