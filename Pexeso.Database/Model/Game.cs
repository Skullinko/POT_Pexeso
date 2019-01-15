using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Pexeso.ChatLibrary.Model;

namespace Pexeso.Database.Model
{
    [Table("Pexeso_Games")]
    public class Game
    {
        public int Id { get; set; }

        public GameSize GameSize { get; set; }

        public TimeSpan Duration { get; set; }

        public virtual ICollection<Match> Matches { get; set; }

        [NotMapped]
        public Dictionary<string, Match> MyMatches { get; set; }  

        public Game()
        {
            MyMatches = new Dictionary<string, Match>();
        }
    }
}