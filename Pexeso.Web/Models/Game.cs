using System;
using Pexeso.ChatLibrary.Model;

namespace Pexeso.Web.Models
{
    public class Game
    {
        public int Id { get; set; }

        public GameSize GameSize { get; set; }

        public TimeSpan Duration { get; set; }
    }
}