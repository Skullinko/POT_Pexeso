using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pexeso.Database.Model
{
    [Table("Pexeso_Players")]
    public class Player
    {
        [Key]
        public string Nick { get; set; }

        public virtual ICollection<Match> Matches { get; set; }
    }
}
