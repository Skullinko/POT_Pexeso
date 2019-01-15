using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pexeso.ChatLibrary.Model;

namespace Pexeso.Database.Model
{
    [Table("Pexeso_Matches")]
    public class Match
    {
        [Key, Column(Order = 0)]
        public string PlayerNick { get; set; }

        [Key, Column(Order = 1)]
        public int GameId { get; set; }

        public MatchResult Result { get; set; }

        public int NumberOfMoves { get; set; }

        public virtual Player Player { get; set; }

        public virtual Game Game { get; set; }
    }
}
