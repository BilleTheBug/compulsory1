using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class SuggestionReplyMessage
    {
        public String Word { get; set; }
        public float ServerLoad { get; set; }
        public int ServiceID { get; set; }
    }
}
