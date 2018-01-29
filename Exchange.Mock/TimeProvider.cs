using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Mock
{
    public class TimeProvider : ITimeProvider
    {
        private DateTime currentTime;

        public TimeProvider(DateTime currentTime)
        {
            this.currentTime = currentTime;
        }

        public DateTime Now()
        {
            return currentTime;
        }

        public void SetTime(DateTime time)
        {
            currentTime = time;
        }

        public void ShiftBy(TimeSpan ts)
        {
            currentTime = currentTime + ts;
        }
    }
}
