using System;

namespace BlaineRP.Server.Game.Management.Cooldowns
{
    public class Cooldown
    {
        public DateTime StartDate { get; private set; }

        public TimeSpan Time { get; private set; }

        public Guid Guid { get; private set; }

        public Cooldown(DateTime startDate, TimeSpan time)
        {
            this.StartDate = startDate;
            this.Time = time;

            this.Guid = Guid.NewGuid();
        }

        public Cooldown(DateTime startDate, TimeSpan time, Guid guid)
        {
            this.StartDate = startDate;
            this.Time = time;
            this.Guid = guid;
        }

        public void Update(DateTime startDate, TimeSpan time)
        {
            StartDate = startDate;
            Time = time;
        }

        public bool IsActive(DateTime currentDate, out TimeSpan timePassed, out TimeSpan timeLeft, out DateTime startDate, double timeFactor = 1d)
        {
            var cdTime = Time * timeFactor;

            startDate = StartDate;
            timePassed = currentDate.Subtract(startDate);
            timeLeft = cdTime.Subtract(timePassed);

            if (timeLeft <= timePassed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
