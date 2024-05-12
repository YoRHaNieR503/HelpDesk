using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models
{
    public class Status
    {
        public int Id { get; set; }

        public string? name { get; set; }

        [Column("Status")]
        public string? StatusText { get; set; }

    }
}
