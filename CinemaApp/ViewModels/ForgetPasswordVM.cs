namespace CinemaApp.ViewModels
{
    public class ForgetPasswordVM
    {
        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
    }
}
