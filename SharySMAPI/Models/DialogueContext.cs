using System;
using System.Collections.Generic;

namespace SharySMAPI.Models
{
    public class DialogueContext
    {
        public string PlayerName { get; set; }
        public int FriendshipLevel { get; set; }
        public string CurrentLocation { get; set; }
        public string TimeOfDay { get; set; }
        public string Season { get; set; }
        public List<string> RecentTopics { get; set; }
        public string Weather { get; set; }
        
        public DialogueContext()
        {
            this.RecentTopics = new List<string>();
        }
    }
}