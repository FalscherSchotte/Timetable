
using System.Xml.Serialization;
namespace TimetablePlanner
{
    public class Group : IExportable
    {
        public string Id { get; set; }
        public string Name { get; set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Group(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            Index = globalIndex;
            globalIndex++;
        }

        public Group() { }

        public override string ToString()
        {
            return Id;
        }
    }
}
