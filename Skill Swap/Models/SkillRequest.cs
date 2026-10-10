using SkillSwap.Models.Enums;

namespace SkillSwap.Models
{

    public class SkillRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        public ProficiencyLevel DesiredLevel { get; set; }
    }
}