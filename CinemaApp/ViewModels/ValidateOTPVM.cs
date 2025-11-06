namespace CinemaApp.ViewModels
{
    public class ValidateOTPVM
    {
        [Required]
        public string OTP { get; set; }=string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
