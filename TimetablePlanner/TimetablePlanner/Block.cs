using System;

namespace TimetablePlanner
{
    public class Block
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DayOfWeek[] Exceptions { get; set; }

        public Block(DateTime start, DateTime end, DayOfWeek[] exceptions)
        {
            this.Start = start;
            this.End = end;
            this.Exceptions = exceptions;
        }

        public Block() { }

        public override string ToString()
        {
            return Start.TimeOfDay.ToString() + " - " + End.TimeOfDay.ToString() + " - Exceptions: " + Exceptions.Length;
        }
    }
}
