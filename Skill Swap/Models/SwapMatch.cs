using SkillSwap.Models.Enums;

namespace SkillSwap.Models
{
    public class SwapMatch
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;

        public int ICanTeachSkillId { get; set; }
        public string ICanTeach { get; set; } = string.Empty;
        public ProficiencyLevel TeachLevel { get; set; }

        public int ICanLearnSkillId { get; set; }
        public string ICanLearn { get; set; } = string.Empty;
        public ProficiencyLevel LearnDesiredLevel { get; set; }
    }
}