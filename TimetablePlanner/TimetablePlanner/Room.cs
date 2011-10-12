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
        
        public Room(string number, bool isLab)
        {
            this.Number = number;
            this.IsLab = isLab;
        }

        public override string ToString()
        {
            return Number + (IsLab ? "(Lab)" : "");
        }
    }
}
