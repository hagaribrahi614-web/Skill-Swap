namespace SkillSwap.Models
{

    public class Session
    {
        public int Id { get; set; }

        public int SwapRequestId { get; set; }
        public SwapRequest SwapRequest { get; set; } = null!;

        public string Notes { get; set; } = string.Empty;
    }
}