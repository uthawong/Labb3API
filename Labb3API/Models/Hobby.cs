using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Labb3API.Models
{
    public class Hobby
    {
        [Key]
        public int HobbyId { get; set; }
        public string HobbyTitle { get; set; }
        public string HobbyDescription { get; set; }
        public List<Link> Links { get; set; }
    }
}