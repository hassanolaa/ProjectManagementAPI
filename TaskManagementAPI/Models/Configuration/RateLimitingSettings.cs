namespace TaskManagementAPI.Models.Configuration
{
    public class RateLimitingSettings
    {
        public int GlobalLimitPerMinute { get; set; } = 100;
        public int AuthLimitPer15Minutes { get; set; } = 10;
        public int PasswordResetLimitPerHour { get; set; } = 3;
        public int TaskCreationLimitPerMinute { get; set; } = 50;
        public int FileUploadLimitPerMinute { get; set; } = 10;
    }
}
