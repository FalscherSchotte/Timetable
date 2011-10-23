
namespace TimetablePlanner
{
    public class Group : IExportable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Group(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
