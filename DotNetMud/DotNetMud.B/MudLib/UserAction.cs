using System;

namespace DotNetMud.B.MudLib
{
    public class UserAction
    {
        public string Verb { get; set; }
        public Action<UserActionExecutionContext> Action { get; set; } 
    }
}