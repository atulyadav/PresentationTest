using Alchemy.Classes;

namespace AsyncSocketServer
{
    /// <summary>
    /// Holds the name and context instance for an online user
    /// </summary>
    public class User
    {
        public string Name { get; set; }

        public bool IsHostServer { get; set; }

        public UserContext Context { get; set; }
    }
}