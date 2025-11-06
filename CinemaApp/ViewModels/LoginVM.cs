
namespace CinemaApp.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string UserNameOREmail { get; set; }=string.Empty;
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
