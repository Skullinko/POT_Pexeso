using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pexeso.Web.Models
{
    public class Player
    {
        public string Nick { get; set; }

        public Player(string nick)
        {
            Nick = nick;
        }
    }
}