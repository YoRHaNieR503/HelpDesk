using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Account
    {

        [Key]
        public int Id { get; set; }

        [Display(Name ="Nombre Usuario")]
        public string? Username { get; set; }
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }
        [Display(Name = "Nombre Completo")]
        public string? FullName { get; set; }
        [Display(Name = "Estado")]
        public string? Status { get; set; }
        [Display(Name = "Correo Electronico")]
        public string? Email { get; set; }
        [Display(Name = "Rol")]
        public int? RoleId { get; set; }
        [Display(Name = "Url Foto")]
        public string? FotoUrl { get; set; }
        [Display(Name = "Empresa Asociada")]
        public string? AssociateE {  get; set; }
    }
}
