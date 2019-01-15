using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pexeso.ChatLibrary;
using Pexeso.ChatLibrary.Model;

namespace Pexeso.Web.Models
{
    public class Match
    {
        public Player Player { get; set; }
        public Game Game { get; set; }
        public MatchResult Result { get; set; }
        public int NumberOfMoves { get; set; }
    }
}