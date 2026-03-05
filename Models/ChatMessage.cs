using System;

namespace BussinessErp.Models
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
