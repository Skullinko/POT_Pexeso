using System.Data.Entity;

namespace Pexeso.Database.Model
{
    public class PexesoContext : DbContext
    {
        public virtual DbSet<Player> Players { get; set; }

        public virtual DbSet<Game> Games { get; set; }

        public virtual DbSet<Match> Matches { get; set; }
    }

}
