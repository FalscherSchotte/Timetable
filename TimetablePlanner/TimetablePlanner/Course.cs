
namespace TimetablePlanner
{
    public class Course
    {
        public bool NeedsLab { get; private set; }
        public bool IsDummy { get; private set; }
        public int NumberOfBlocks { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Room RoomPreference { get; private set; }
        public Lecturer[] Lecturers { get; private set; }
        public Group Group { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Course(string id, string name, Lecturer[] lecturers, Room roomPreference, Group group, bool needsLab, bool isDummy, /*int repeatsPerWeek,*/ int numberOfBlocks)
        {
            this.Id = id;
            this.Name = name;
            this.Lecturers = lecturers;
            this.RoomPreference = roomPreference;
            this.NeedsLab = needsLab;
            this.IsDummy = isDummy;
            this.NumberOfBlocks = numberOfBlocks;
            this.Group = group;

            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Id + ": " + Name;
        }
    }
}
