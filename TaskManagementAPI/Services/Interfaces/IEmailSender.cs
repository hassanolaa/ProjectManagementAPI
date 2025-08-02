namespace TaskManagementAPI.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
}
