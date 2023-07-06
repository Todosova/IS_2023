using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }
        public int TicketId { get; set; }
        public virtual Tickets FromCart { get; set; }
    }
}
