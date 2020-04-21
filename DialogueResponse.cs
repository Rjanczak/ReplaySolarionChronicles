using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaySolarionChronicles
{
    public class DialogueResponse
    {
        public string ResponseText { get; set; }
        public List<DialogueSegment> ResponseReaction { get; set; }
        public List<string> DisplayConditions { get; set; }
        public List<string> SetFlags { get; set; }
        public int AddScore { get; set; } = 0;
        public int AddWound { get; set; } = 0;
        public int ChangeScene { get; set; } = -1;
    }
}
