namespace CinemaApp.ViewModels
{
    public class ResendEmailConfirmationVM
    {
        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
    }
}
