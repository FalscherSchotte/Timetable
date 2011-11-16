using System;

namespace TimetablePlanner
{
    public class TimetableData
    {
        public Block[] Blocks { get; set; }
        public Course[] Courses { get; set; }
        public Lecturer[] Lecturers { get; set; }
        public Room[] Rooms { get; set; }
        public Group[] Groups { get; set; }

        public TimetableData()
        {
            Course.ResetIndexCounter();
            Lecturer.ResetIndexCounter();
            Room.ResetIndexCounter();
            Group.ResetIndexCounter();
        }

        public TimetableData(Block[] blocks, Course[] courses, Lecturer[] lecturers, Room[] rooms, Group[] groups)
            : base()
        {
            this.Blocks = blocks;
            this.Courses = courses;
            this.Lecturers = lecturers;
            this.Rooms = rooms;
            this.Groups = groups;

            //Array.Sort(courses, SortCoursesByLength);
        }

        private static int SortCoursesByLength(Course x, Course y)
        {
            if (x.NumberOfBlocks > y.NumberOfBlocks)
                return -1;
            if (x.NumberOfBlocks < y.NumberOfBlocks)
                return +1;
            return 0;
        }
    }
}
