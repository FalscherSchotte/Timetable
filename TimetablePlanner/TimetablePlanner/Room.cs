using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Room
    {
        public string Number { get; private set; }
        public bool IsLab { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Room(string number, bool isLab)
        {
            this.Number = number;
            this.IsLab = isLab;

            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Number + (IsLab ? "(Lab)" : "");
        }
    }
}
