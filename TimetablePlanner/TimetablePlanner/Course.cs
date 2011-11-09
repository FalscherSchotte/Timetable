
using System.Xml.Serialization;
namespace TimetablePlanner
{
    public class Course
    {
        public bool NeedsLab { get; set; }
        public bool IsDummy { get; set; }
        public int NumberOfBlocks { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Room RoomPreference { get; set; }
        public Lecturer[] Lecturers { get; set; }
        public Group Group { get; set; }

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

        public Course() { }

        public override string ToString()
        {
            return Id + ": " + Name;
        }
    }
}
