using System;

namespace DotNetMud.Mudlib
{
    public class UserAction
    {
        public string Verb { get; set; }
        public Action<UserActionExecutionContext> Action { get; set; } 
    }
}