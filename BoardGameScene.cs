using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaySolarionChronicles
{
    public class BoardGameScene
    {
        public int FadeTimer { get; set; } = 0;
        public int ShakeTimer { get; set; } = 0;
        public int DefaultNext { get; set; }
        public string Image { get; set; }
        public string Question { get; set; }
        public List<DialogueSegment> Dialogue { get; set; }
        public string NewMusic { get; set; }
        public List<DialogueResponse> Answers { get; set; }
    }
}
