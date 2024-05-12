using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Account
    {

        [Key]
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? FotoUrl { get; set; }
        public string? AssociateE {  get; set; }
    }
}
