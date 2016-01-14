using System.Collections.Generic;

namespace DotNetMud.MudLib
{
    public class UserActionExecutionContext
    {
        public User Player { get; set; }
        public string Verb { get; set; }
        public List<string> Parameters { get; set; }
        public UserAction UserAction { get; set; } 
    }
}