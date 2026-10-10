

namespace SkillSwap.Models
{

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<SkillOffer> Offers { get; set; } = new List<SkillOffer>();
        public ICollection<SkillRequest> Requests { get; set; } = new List<SkillRequest>();

        public ICollection<SwapRequest> SentSwaps { get; set; } = new List<SwapRequest>();
        public ICollection<SwapRequest> ReceivedSwaps { get; set; } = new List<SwapRequest>();

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}