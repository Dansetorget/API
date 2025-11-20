namespace QuizApi.DTOs
{
    public class UserInfoDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public int Streak { get; set; } = 0;
        public DateTime? CreatedAt { get; set; }
    }
}