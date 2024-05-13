using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Comentarios
    {
        [Key]
        public int Id { get; set; }
        public string? Comentario { get; set; }
        public int TicketId { get; set; }
    }

}
