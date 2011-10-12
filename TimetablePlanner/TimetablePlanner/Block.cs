using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Block
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public DayOfWeek[] Exceptions { get; set; }

        public Block(DateTime start, DateTime end, DayOfWeek[] exceptions)
        {
            this.Start = start;
            this.End = end;
            this.Exceptions = exceptions;
        }

        public override string ToString()
        {
            return Start.TimeOfDay.ToString() + " - " + End.TimeOfDay.ToString() + " - Exceptions: " + Exceptions.Length;
        }
    }
}
