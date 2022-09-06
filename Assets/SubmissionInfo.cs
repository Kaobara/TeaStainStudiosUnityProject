
// This class contains metadata for your submission. It plugs into some of our
// grading tools to extract your game/team details. Ensure all Gradescope tests
// pass when submitting, as these do some basic checks of this file.
public static class SubmissionInfo
{
    // TASK: Fill out all team + team member details below by replacing the
    // content of the strings. Also ensure you read the specification carefully
    // for extra details related to use of this file.

    // URL to your group's project 2 repository on GitHub.
    public static readonly string RepoURL = "https://github.com/COMP30019/project-2-tea-stain-studios";
    
    // Come up with a team name below (plain text, no more than 50 chars).
    public static readonly string TeamName = "Tea Stain Studios";
    
    // List every team member below. Ensure student names/emails match official
    // UniMelb records exactly (e.g. avoid nicknames or aliases).
    public static readonly TeamMember[] Team = new[]
    {
        new TeamMember("Mukul Chodhary", "mchodhary@student.unimelb.edu.au"),
        new TeamMember("Peh Ni Tan", "pehnit@student.unimelb.edu.au"),
        new TeamMember("Mohamad Danielsyah Mahmud", "mohamaddanie@student.unimelb.edu.au"),        
        new TeamMember("Sebastian Thomas Tobin-Couzens", "stobincouzen@student.unimelb.edu.au"), 
    };

    // This may be a "working title" to begin with, but ensure it is final by
    // the video milestone deadline (plain text, no more than 50 chars).
    public static readonly string GameName = "Chicken Delivery Service";

    // Write a brief blurb of your game, no more than 200 words. Again, ensure
    // this is final by the video milestone deadline.
    public static readonly string GameBlurb =
@"Chiki’s Delivery Service is a Casual Simulation RPG featuring ragdoll physics delivered by Tea Stain Studios. In Chiki’s Delivery Service, the player takes on the role of Chiki, an aspiring young chick 🐤 who has just started her employment in the esteemed 🐔Chicken Delivery Service company. 

 
As Chiki, the player will tackle various levels involving bypassing several obstacles to successfully deliver packages to the designated area. Each level will feature procedurally generated scenery and clouds, with certain parts of the level containing manual set components. However, the goal remains consistent across all levels: deliver the package successfully. 


Upon delivering the package successfully, the player clears the level and unlocks the next one. Additionally, players will also be awarded Bronze/Silver/Gold medals based on their completion time on that level. Each level features different thresholds of completion time for the medals, adjusted accordingly with the expectations of the time taken to complete the level.


In traversing levels, players use standard WASD movement to control the movement of Chiki. Players can also use the spacebar to make Chiki jump and hold the spacebar when Chiki is in the air to glide. Players will need to utilise these mechanics effectively to obtain completion medals.
";
    
    // By the gameplay video milestone deadline this should be a direct link
    // to a YouTube video upload containing your video. Ensure "Made for kids"
    // is turned off in the video settings. 
    public static readonly string GameplayVideo = "https://youtube.com/...";
    
    // No more info to fill out!
    // Please don't modify anything below here.
    public readonly struct TeamMember
    {
        public TeamMember(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; }
        public string Email { get; }
    }
}
