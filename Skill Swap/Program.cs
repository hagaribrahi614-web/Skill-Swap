using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.Models.Enums;
using SkillSwap.Services;



namespace SkillSwap
{

    internal class Program
    {
        private static SkillSwapService _service = null!;
        private static User? _currentUser;

        static async Task Main()
        {
            string connectionString =
                "Server=.;Database=SkillSwapConsoleDb;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<SkillSwapDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            using var dbContext = new SkillSwapDbContext(optionsBuilder.Options);

            _service = new SkillSwapService(dbContext);

            Console.Title = "Skill Swap Console Application";

            await RunApplicationAsync();
        }

        private static async Task RunApplicationAsync()
        {
            bool isRunning = true;

            while (isRunning)
            {
                PrintHeader("SKILL SWAP");

                if (_currentUser == null)
                {
                    Console.WriteLine("You are not logged in.");
                }
                else
                {
                    Console.WriteLine($"Logged in as: {_currentUser.Name} | User ID: {_currentUser.Id}");
                }

                Console.WriteLine();
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Add Skill");
                Console.WriteLine("3. Add Skill Offer");
                Console.WriteLine("4. Add Skill Request");
                Console.WriteLine("5. Find Mutual Matches");
                Console.WriteLine("6. Send Swap Request");
                Console.WriteLine("7. View Incoming Swap Requests");
                Console.WriteLine("8. Accept Swap Request");
                Console.WriteLine("9. Reject Swap Request");
                Console.WriteLine("10. Create Session");
                Console.WriteLine("11. View My Sessions");
                Console.WriteLine("12. Complete Swap");
                Console.WriteLine("0. Exit");

                Console.WriteLine();
                Console.Write("Choose an option: ");

                string? choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await LoginAsync();
                            break;

                        case "2":
                            await AddSkillAsync();
                            break;

                        case "3":
                            await AddSkillOfferAsync();
                            break;

                        case "4":
                            await AddSkillRequestAsync();
                            break;

                        case "5":
                            await FindMatchesAsync();
                            break;

                        case "6":
                            await SendSwapRequestAsync();
                            break;

                        case "7":
                            await ViewIncomingRequestsAsync();
                            break;

                        case "8":
                            await AcceptSwapRequestAsync();
                            break;

                        case "9":
                            await RejectSwapRequestAsync();
                            break;

                        case "10":
                            await CreateSessionAsync();
                            break;

                        case "11":
                            await ViewMySessionsAsync();
                            break;

                        case "12":
                            await CompleteSwapAsync();
                            break;

                        case "0":
                            isRunning = false;
                            Console.WriteLine("Goodbye!");
                            break;

                           
                            default:
                            ShowMessage("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine();
                }
            }
        }

        private static async Task LoginAsync()
        {
            PrintHeader("LOGIN");

            Console.Write("Enter your name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowMessage("Name is required.");
                return;
            }

            _currentUser = await _service.LoginOrCreateUserAsync(name);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Login successful.");
            Console.WriteLine($"Welcome, {_currentUser.Name}.");
            Console.WriteLine($"Your User ID is: {_currentUser.Id}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task AddSkillAsync()
        {
            PrintHeader("ADD SKILL");

            Console.Write("Enter skill name: ");
            string skillName = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(skillName))
            {
                ShowMessage("Skill name is required.");
                return;
            }

            Skill skill = await _service.GetOrCreateSkillAsync(skillName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Skill saved successfully.");
            Console.WriteLine($"Skill ID: {skill.Id}");
            Console.WriteLine($"Skill Name: {skill.Name}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task AddSkillOfferAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("ADD SKILL OFFER");

            List<Skill> skills = await ShowSkillsAndGetAsync();

            if (skills.Count == 0)
            {
                return;
            }

            int skillId = ReadInt("Enter the Skill ID you can teach: ");

            ProficiencyLevel level = ReadLevel("Select your level in this skill");

            await _service.AddSkillOfferAsync(_currentUser!.Id, skillId, level);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Skill offer added successfully.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task AddSkillRequestAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("ADD SKILL REQUEST");

            List<Skill> skills = await ShowSkillsAndGetAsync();

            if (skills.Count == 0)
            {
                return;
            }

            int skillId = ReadInt("Enter the Skill ID you want to learn: ");

            ProficiencyLevel desiredLevel = ReadLevel("Select the desired learning level");

            await _service.AddSkillRequestAsync(_currentUser!.Id, skillId, desiredLevel);

            Console.ForegroundColor = ConsoleColor.Green;

            
             Console.WriteLine();
            Console.WriteLine("Skill request added successfully.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task FindMatchesAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("MUTUAL MATCHES");

            List<SwapMatch> matches = await _service.FindMutualMatchesAsync(_currentUser!.Id);

            if (matches.Count == 0)
            {
                Console.WriteLine("No matches are available right now.");
                Console.WriteLine();
                Console.WriteLine("Make sure you added Offers and Requests.");
                Console.WriteLine("Make sure another user has compatible skills and levels.");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();
                return;
            }

            foreach (SwapMatch match in matches)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"Matched User: {match.OtherUserName}");
                Console.ResetColor();

                Console.WriteLine($"User ID: {match.OtherUserId}");
                Console.WriteLine($"You can teach: {match.ICanTeach} ({match.TeachLevel})");
                Console.WriteLine($"You can learn: {match.ICanLearn} (Desired Level: {match.LearnDesiredLevel})");
                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task SendSwapRequestAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("SEND SWAP REQUEST");

            Console.WriteLine("Find matches first to know the correct receiver and skills.");
            Console.WriteLine();

            int receiverId = ReadInt("Enter Receiver User ID: ");

            List<Skill> skills = await ShowSkillsAndGetAsync();

            if (skills.Count == 0)
            {
                return;
            }

            int offeredSkillId = ReadInt("Enter the Skill ID you will teach: ");
            int requestedSkillId = ReadInt("Enter the Skill ID you want to learn: ");

            SwapRequest swap = await _service.CreateSwapRequestAsync(
                _currentUser!.Id,
                receiverId,
                offeredSkillId,
                requestedSkillId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Swap request sent successfully.");
            Console.WriteLine($"Swap ID: {swap.Id}");
            Console.WriteLine($"Status: {swap.Status}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task ViewIncomingRequestsAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("INCOMING SWAP REQUESTS");

            List<SwapRequest> requests = await _service.GetIncomingSwapRequestsAsync(_currentUser!.Id);

            if (requests.Count == 0)
            {
                Console.WriteLine("No incoming pending requests.");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();
                return;
            }

            foreach (SwapRequest request in requests)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"Swap ID: {request.Id}");
                Console.ResetColor();

                
                Console.WriteLine($"From: {request.Sender.Name}");
                Console.WriteLine($"They can teach you: {request.OfferedSkill.Name}");
                Console.WriteLine($"They want to learn: {request.RequestedSkill.Name}");
                Console.WriteLine($"Status: {request.Status}");
                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task AcceptSwapRequestAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("ACCEPT SWAP REQUEST");

            Console.WriteLine("Only the receiver can accept a pending swap request.");
            Console.WriteLine();

            int swapId = ReadInt("Enter Swap ID: ");

            SwapRequest swap = await _service.AcceptSwapAsync(swapId, _currentUser!.Id);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Swap request accepted successfully.");
            Console.WriteLine($"Status: {swap.Status}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task RejectSwapRequestAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("REJECT SWAP REQUEST");

            Console.WriteLine("Only the receiver can reject a pending swap request.");
            Console.WriteLine();

            int swapId = ReadInt("Enter Swap ID: ");

            SwapRequest swap = await _service.RejectSwapAsync(swapId, _currentUser!.Id);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Swap request rejected successfully.");
            Console.WriteLine($"Status: {swap.Status}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task CreateSessionAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("CREATE SESSION");

            Console.WriteLine("A session can only be created after the swap is accepted.");
            Console.WriteLine();

            int swapId = ReadInt("Enter Accepted Swap ID: ");

            Console.Write("Enter session notes: ");
            string notes = Console.ReadLine()?.Trim() ?? "";

            Session session = await _service.CreateSessionAsync(swapId, notes);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Session created successfully.");
            Console.WriteLine($"Session ID: {session.Id}");
            Console.WriteLine($"Swap ID: {session.SwapRequestId}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task ViewMySessionsAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("MY SESSIONS");

            List<Session> sessions = await _service.GetUserSessionsAsync(_currentUser!.Id);

            if (sessions.Count == 0)
            {
                Console.WriteLine("No sessions have been created yet.");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();
                return;
            }

            foreach (Session session in sessions)
            {
                SwapRequest swap = session.SwapRequest;

                string otherUserName =
                    swap.SenderId == _currentUser!.Id
                        ? swap.Receiver.Name
                        : swap.Sender.Name;

                string teachingSkill =
                    swap.SenderId == _currentUser.Id
                        ? swap.OfferedSkill.Name
                        : swap.RequestedSkill.Name;

                
                string learningSkill =
                            swap.SenderId == _currentUser.Id
                                ? swap.RequestedSkill.Name
                                : swap.OfferedSkill.Name;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"Session ID: {session.Id}");
                Console.ResetColor();

                Console.WriteLine($"Swap ID: {swap.Id}");
                Console.WriteLine($"With: {otherUserName}");
                Console.WriteLine($"You teach: {teachingSkill}");
                Console.WriteLine($"You learn: {learningSkill}");
                Console.WriteLine($"Notes: {session.Notes}");
                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task CompleteSwapAsync()
        {
            if (!RequireLogin())
            {
                return;
            }

            PrintHeader("COMPLETE SWAP");

            Console.WriteLine("Only swap participants can complete an accepted swap.");
            Console.WriteLine();

            int swapId = ReadInt("Enter Swap ID: ");

            SwapRequest swap = await _service.CompleteSwapAsync(swapId, _currentUser!.Id);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Swap completed successfully.");
            Console.WriteLine($"Status: {swap.Status}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }

        private static async Task<List<Skill>> ShowSkillsAndGetAsync()
        {
            List<Skill> skills = await _service.GetAllSkillsAsync();

            if (skills.Count == 0)
            {
                Console.WriteLine("No skills are available.");
                Console.WriteLine("Please add at least one skill first.");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();
                return skills;
            }

            Console.WriteLine();
            Console.WriteLine("Available Skills:");

            foreach (Skill skill in skills)
            {
                Console.WriteLine($"{skill.Id}. {skill.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();

            return skills;
        }

        private static ProficiencyLevel ReadLevel(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("1. Beginner");
            Console.WriteLine("2. Intermediate");
            Console.WriteLine("3. Advanced");
            Console.WriteLine("4. Expert");

            while (true)
            {
                Console.Write("Choose level (1-4): ");

                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) &&
                Enum.IsDefined(typeof(ProficiencyLevel), value))
                {
                    return (ProficiencyLevel)value;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid level. Please choose a number from 1 to 4.");
                Console.ResetColor();
            }
        }

        private static int ReadInt(string message)
        {
            while (true)
            {
                Console.Write(message);

                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value > 0)
                {
                    return value;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a valid positive number.");
                Console.ResetColor();
            }
        }

        private static bool RequireLogin()
        {
            if (_currentUser != null)
            {
                return true;
            }

            
            ShowMessage("Please login first.");
            return false;
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine($"                 {title}");
            Console.WriteLine("==============================================");
            Console.WriteLine();
        }

        private static void ShowMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
        }
    }

}