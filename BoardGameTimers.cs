using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ReplaySolarionChronicles
{
    public class BoardGameTimers
    {
        private List<int> timers = new List<int>();
        private List<bool> timersEnded = new List<bool>();
        public BoardGameTimers()
        {
            for(int i=0; i<Enum.GetNames(typeof(TimerType)).Length; i++)
            {
                timers.Add(0);
                timersEnded.Add(false);
            }
        }
        
        public void SetTimer(TimerType whichTimer, int newTimerValue)
        {
            timers[(int)whichTimer] = newTimerValue;
            timersEnded[(int)whichTimer] = false;
        }

        public void CountDownTimers(GameTime time)
        {
            TimeSpan elapsedGameTime;
            for (int i = 0; i < timers.Count; i++)
            {
                int currentTimer = timers[i];
                if (currentTimer > 0)
                {
                    elapsedGameTime = time.ElapsedGameTime;
                    int milliseconds = elapsedGameTime.Milliseconds;
                    timers[i] = currentTimer - milliseconds;
                    if(timers[i] <= 0)
                    {
                        timersEnded[i] = true;
                    }
                }
            }
        }

        public int GetTimer(TimerType whichTimer)
        {
            return timers[(int)whichTimer];
        }

        public bool HasTimerEnded(TimerType whichTimer)
        {
            return timersEnded[(int)whichTimer];
        }

    }
    public enum TimerType { APPEAR_TIMER, END_TIMER, SHAKE_TIMER }
}
