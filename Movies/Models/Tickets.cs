using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class Tickets
    {
        [Key]
        public int Id { get; set; }
        public string MovieTitle { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int NumTickets { get; set; }
        [BindProperty, DataType(DataType.Date)]
        public DateTime? Date { get; set; }
        /*[NotMapped]
        public List<string> GenreList { get; set; }*/

    }
}
