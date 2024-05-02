using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Labb3API.Models
{
    public class Link
    {
        [Key]
        public int LinkId { get; set; }
        public string LinkUrl { get; set; }

    }
}
