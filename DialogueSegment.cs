using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace ReplaySolarionChronicles
{
    public class DialogueSegment
    {
        public string DialogueText { get; set; }
        public string PlaySound { get; set; }
        public int DialogueDelay { get; set; } = 0;
        public List<string> DisplayConditions { get; set; }

        public void DisplayCurrentSegment(HashSet<string> satisfiedConditions)
        {
            if (this.DisplayConditions != null)
                foreach (string condition in this.DisplayConditions)
                {
                    if (!satisfiedConditions.Contains(condition))
                        return;
                }

            if (PlaySound != null)
                Game1.playSound(this.PlaySound);

            if (DialogueText != null)
                Game1.activeClickableMenu = new DialogueBox(this.DialogueText);
        }
    }
}
