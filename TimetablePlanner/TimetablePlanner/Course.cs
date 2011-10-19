using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Course
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Room RoomPreference { get; private set; }
        public Lecturer[] Lecturers { get; private set; }
        public LectureTime LectureTimes { get; private set; }
        public Group Group { get; private set; }
        //public int RepeatsPerWeek { get; private set; }
        public int NumberOfBlocks { get; private set; }
        public bool NeedsLab { get; private set; }
        public bool IsDummy { get; private set; }

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
            //this.RepeatsPerWeek = repeatsPerWeek;
            this.NumberOfBlocks = numberOfBlocks;
            this.Group = group;

            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Id + ": " + Name;
        }

        public class LectureTime
        {
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }
            public DayOfWeek Day { get; private set; }
            public Room Room { get; private set; }

            public LectureTime(DateTime start, DateTime end, DayOfWeek day, Room room)
            {
                this.Start = start;
                this.End = end;
                this.Day = day;
                this.Room = room;
            }
        }


    }
}
