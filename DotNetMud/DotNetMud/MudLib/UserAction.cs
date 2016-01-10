using System;
using System.Collections.Generic;
using DotNetMud.Server;

namespace DotNetMud.MudLib
{
    public class UserAction
    {
        public string Verb { get; set; }
        public int Priority { get; set; }  
        public Action<UserActionExecutionContext> Action { get; set; } 
    }
}