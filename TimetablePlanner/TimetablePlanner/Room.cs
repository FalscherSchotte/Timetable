using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Room : IExportable
    {
        public string Name { get; private set; }
        public string Id { get; private set; }
        public bool IsLab { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Room(string name, bool isLab)
        {
            this.Name = name;
            this.Id = name;
            this.IsLab = isLab;

            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Name + (IsLab ? "(Lab)" : "");
        }

    }
}
