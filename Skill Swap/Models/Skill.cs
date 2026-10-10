namespace SkillSwap.Models
{

    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<SkillOffer> Offers { get; set; } = new List<SkillOffer>();
        public ICollection<SkillRequest> Requests { get; set; } = new List<SkillRequest>();

        public ICollection<SwapRequest> OfferedInSwaps { get; set; } = new List<SwapRequest>();
        public ICollection<SwapRequest> RequestedInSwaps { get; set; } = new List<SwapRequest>();
    }
}