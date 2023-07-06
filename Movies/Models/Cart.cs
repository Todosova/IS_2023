using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int TicketId { get; set; }
        public virtual Tickets Ticket { get; set; }
    }
}
