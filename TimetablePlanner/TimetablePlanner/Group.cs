using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Group
    {
        public string Id { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Group(string id)
        {
            this.Id = id;
            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
