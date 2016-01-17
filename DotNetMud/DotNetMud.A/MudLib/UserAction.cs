using System;

namespace DotNetMud.A.MudLib
{
    public class UserAction
    {
        public string Verb { get; set; }
        public Action<UserActionExecutionContext> Action { get; set; } 
    }
}