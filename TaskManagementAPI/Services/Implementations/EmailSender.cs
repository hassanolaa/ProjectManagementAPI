using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using TaskManagementAPI.Models.Configuration;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations
{


    public class EmailSender : IEmailSender
    {

        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new SmtpClient();
                
                // Connect to SMTP server
                await smtp.ConnectAsync(
                    _emailSettings.SmtpServer, 
                    _emailSettings.SmtpPort, 
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate if credentials provided
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }

          public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            var subject = "Password Reset Request - Task Management Platform";
            var htmlMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333; text-align: center;'>Password Reset Request</h2>
                        <p>Hello {userName},</p>
                        <p>You requested to reset your password for your Task Management Platform account.</p>
                        <p>Click the button below to reset your password:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                        </div>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
                        <p style='color: #666; font-size: 14px;'>
                            This link will expire in 1 hour for security reasons.<br>
                            If you didn't request this password reset, please ignore this email.
                        </p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='color: #999; font-size: 12px; text-align: center;'>
                            Task Management Platform Team
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

         public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Task Management Platform";
            var htmlMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333; text-align: center;'>Welcome to Task Management Platform!</h2>
                        <p>Hello {userName},</p>
                        <p>Welcome to our Task Management Platform! Your account has been successfully created.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Create and manage projects</li>
                            <li>Collaborate with your team</li>
                            <li>Track time and progress</li>
                            <li>Receive real-time notifications</li>
                        </ul>
                        <p>Get started by logging into your account and exploring the features.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='color: #999; font-size: 12px; text-align: center;'>
                            Task Management Platform Team
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }
    }


}