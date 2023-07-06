using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }
        public List<string> Role { get; set; }

        public Roles()
        {
            Role = new List<string>{"Admin", "USER"};
        }
    }
}
