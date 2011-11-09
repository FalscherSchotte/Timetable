
using System.Xml.Serialization;
namespace TimetablePlanner
{
    public class Room : IExportable
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsLab { get; set; }

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

        public Room() { }

        public override string ToString()
        {
            return Name + (IsLab ? "(Lab)" : "");
        }
    }
}
