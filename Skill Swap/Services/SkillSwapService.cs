
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.Models.Enums;

namespace SkillSwap.Services
{
    public class SkillSwapService
    {
        private readonly SkillSwapDbContext _context;

        public SkillSwapService(SkillSwapDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginOrCreateUserAsync(string name)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == name);

            if (user == null)
            {
                user = new User { Name = name };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public async Task<Skill> GetOrCreateSkillAsync(string skillName)
        {
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Name == skillName);

            if (skill == null)
            {
                skill = new Skill { Name = skillName };
                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
            }

            return skill;
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            return await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task AddSkillOfferAsync(
            int userId,
            int skillId,
            ProficiencyLevel level)
        {
            var existing = await _context.SkillOffers
                .FirstOrDefaultAsync(o =>
                    o.UserId == userId &&
                    o.SkillId == skillId);

            if (existing != null)
            {
                existing.Level = level;
            }
            else
            {
                var offer = new SkillOffer
                {
                    UserId = userId,
                    SkillId = skillId,
                    Level = level
                };

                _context.SkillOffers.Add(offer);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddSkillRequestAsync(
            int userId,
            int skillId,
            ProficiencyLevel desiredLevel)
        {
            var existing = await _context.SkillRequests
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.SkillId == skillId);

            if (existing != null)
            {
                existing.DesiredLevel = desiredLevel;
            }
            else
            {
                var request = new SkillRequest
                {
                    UserId = userId,
                    SkillId = skillId,
                    DesiredLevel = desiredLevel
                };

                _context.SkillRequests.Add(request);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<SwapMatch>> FindMutualMatchesAsync(int userId)
        {
            var myOffers = await _context.SkillOffers
                .Where(o => o.UserId == userId)
                .Include(o => o.Skill)
                .ToListAsync();

            var myRequests = await _context.SkillRequests
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (myOffers.Count == 0 || myRequests.Count == 0)
            {
                return new List<SwapMatch>();
            }

            var otherUserIds = await _context.SkillOffers
                .Where(o => o.UserId != userId)
                .Select(o => o.UserId)
                .Distinct()
                .ToListAsync();

            if (otherUserIds.Count == 0)
            {
                return new List<SwapMatch>();
            }

            var otherOffers = await _context.SkillOffers
                .Where(o => otherUserIds.Contains(o.UserId))
                .Include(o => o.Skill)
                .Include(o => o.User)
                .ToListAsync();

            
            var otherRequests = await _context.SkillRequests
                .Where(r => otherUserIds.Contains(r.UserId))
                .Include(r => r.Skill)
                .Include(r => r.User)
                .ToListAsync();

            var matches = new List<SwapMatch>();

            foreach (var otherId in otherUserIds)
            {
                var theirOffers = otherOffers
                    .Where(o => o.UserId == otherId)
                    .ToList();

                var theirRequests = otherRequests
                    .Where(r => r.UserId == otherId)
                    .ToList();

                foreach (var myOffer in myOffers)
                {
                    var matchRequest = theirRequests
                        .FirstOrDefault(r => r.SkillId == myOffer.SkillId);

                    if (matchRequest == null)
                        continue;

                    foreach (var theirOffer in theirOffers)
                    {
                        var myMatchRequest = myRequests
                            .FirstOrDefault(r => r.SkillId == theirOffer.SkillId);

                        if (myMatchRequest == null)
                            continue;

                        var otherUser = theirOffers.FirstOrDefault()?.User;

                        if (otherUser == null)
                            continue;

                        matches.Add(new SwapMatch
                        {
                            OtherUserId = otherId,
                            OtherUserName = otherUser.Name,

                            ICanTeachSkillId = myOffer.SkillId,
                            ICanTeach = myOffer.Skill.Name,
                            TeachLevel = myOffer.Level,

                            ICanLearnSkillId = theirOffer.SkillId,
                            ICanLearn = theirOffer.Skill.Name,
                            LearnDesiredLevel = myMatchRequest.DesiredLevel
                        });
                    }
                }
            }

            return matches;
        }

        public async Task<SwapRequest> CreateSwapRequestAsync(
            int senderId,
            int receiverId,
            int offeredSkillId,
            int requestedSkillId)
        {
            var swap = new SwapRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                OfferedSkillId = offeredSkillId,
                RequestedSkillId = requestedSkillId,
                Status = SwapStatus.Pending
            };

            _context.SwapRequests.Add(swap);
            await _context.SaveChangesAsync();

            return swap;
        }

        public async Task<List<SwapRequest>> GetIncomingSwapRequestsAsync(int receiverId)
        {
            return await _context.SwapRequests
                .Where(s =>
                    s.ReceiverId == receiverId &&
                    s.Status == SwapStatus.Pending)
                .Include(s => s.Sender)
                .Include(s => s.OfferedSkill)
                .Include(s => s.RequestedSkill)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<SwapRequest> AcceptSwapAsync(
            int swapId,
            int currentUserId)
        {
            var swap = await _context.SwapRequests
                .Include(s => s.Sender)
                .Include(s => s.Receiver)
                .FirstOrDefaultAsync(s => s.Id == swapId);

            if (swap == null)
                throw new Exception("Swap request not found.");

            if (swap.ReceiverId != currentUserId)
                throw new Exception("Only the receiver can accept this swap.");

            if (swap.Status != SwapStatus.Pending)
                throw new Exception($"This swap is not pending. Current status: {swap.Status}");

            swap.Status = SwapStatus.Accepted;
            await _context.SaveChangesAsync();

            return swap;
        }

        
public async Task<SwapRequest> RejectSwapAsync(
            int swapId,
            int currentUserId)
        {
            var swap = await _context.SwapRequests
                .FirstOrDefaultAsync(s => s.Id == swapId);

            if (swap == null)
                throw new Exception("Swap request not found.");

            if (swap.ReceiverId != currentUserId)
                throw new Exception("Only the receiver can reject this swap.");

            if (swap.Status != SwapStatus.Pending)
                throw new Exception("This swap is not pending.");

            swap.Status = SwapStatus.Rejected;
            await _context.SaveChangesAsync();

            return swap;
        }

        public async Task<SwapRequest> CompleteSwapAsync(
            int swapId,
            int currentUserId)
        {
            var swap = await _context.SwapRequests
                .FirstOrDefaultAsync(s => s.Id == swapId);

            if (swap == null)
                throw new Exception("Swap request not found.");

            if (swap.SenderId != currentUserId &&
                swap.ReceiverId != currentUserId)
                throw new Exception("Only swap participants can complete this swap.");

            if (swap.Status != SwapStatus.Accepted)
                throw new Exception($"Only accepted swaps can be completed. Current status: {swap.Status}");

            swap.Status = SwapStatus.Completed;
            await _context.SaveChangesAsync();

            return swap;
        }

        public async Task<Session> CreateSessionAsync(
            int swapId,
            string notes)
        {
            var swap = await _context.SwapRequests
                .Include(s => s.Sender)
                .Include(s => s.Receiver)
                .FirstOrDefaultAsync(s => s.Id == swapId);

            if (swap == null)
                throw new Exception("Swap request not found.");

            if (swap.Status != SwapStatus.Accepted)
                throw new Exception("Sessions can only be created for accepted swaps.");

            var session = new Session
            {
                SwapRequestId = swapId,
                Notes = notes
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<List<Session>> GetUserSessionsAsync(int currentUserId)
        {
            var sessions = await _context.Sessions
                .Include(s => s.SwapRequest)
                    .ThenInclude(sr => sr.Sender)
                .Include(s => s.SwapRequest)
                    .ThenInclude(sr => sr.Receiver)
                .Include(s => s.SwapRequest.OfferedSkill)
                .Include(s => s.SwapRequest.RequestedSkill)
                .Where(s =>
                    s.SwapRequest.SenderId == currentUserId ||
                    s.SwapRequest.ReceiverId == currentUserId)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return sessions;
        }
    }
}