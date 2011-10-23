using System.Collections.Generic;

namespace TimetablePlanner
{
    public class Individual
    {
        public int Fitness { get; set; }

        /// <summary>
        /// [CourseIndex][DayIndex][BlockIndex] -> RoomIndex
        /// </summary>
        public int[, ,] Courses { get; private set; }

        /// <summary>
        /// [LecturerIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public int[, ,] Lecturers { get; private set; }

        /// <summary>
        /// [RoomIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public int[, ,] Rooms { get; private set; }

        /// <summary>
        /// [GroupIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public int[, ,] Groups { get; private set; }

        public Individual(int numberOfDays, int numberOfBlocks, int numberOfCourses, int numberOfLecturers, int numberOfRooms, int numberOfGroups)
        {
            Courses = InitializeChromosome(numberOfCourses, numberOfDays, numberOfBlocks);
            Lecturers = InitializeChromosome(numberOfLecturers, numberOfDays, numberOfBlocks);
            Rooms = InitializeChromosome(numberOfRooms, numberOfDays, numberOfBlocks);
            Groups = InitializeChromosome(numberOfGroups, numberOfDays, numberOfBlocks);
        }

        private Individual()
        { }

        private static int[, ,] InitializeChromosome(int numberOfBase, int numberOfDays, int numberOfBlocks)
        {
            int[, ,] chromosome = new int[numberOfBase, numberOfDays, numberOfBlocks];
            for (int d0 = 0; d0 < chromosome.GetLength(0); d0++)
            {
                for (int d1 = 0; d1 < chromosome.GetLength(1); d1++)
                {
                    for (int d2 = 0; d2 < chromosome.GetLength(2); d2++)
                    {
                        chromosome[d0, d1, d2] = -1;
                    }
                }
            }
            return chromosome;
        }

        public static Individual Clone(Individual source)
        {
            Individual clone = new Individual();
            clone.Courses = Clone(source.Courses);
            clone.Lecturers = Clone(source.Lecturers);
            clone.Rooms = Clone(source.Rooms);
            clone.Groups = Clone(source.Groups);
            return clone;
        }

        private static int[, ,] Clone(int[, ,] source)
        {
            int[, ,] clone = new int[source.GetLength(0), source.GetLength(1), source.GetLength(2)];
            for (int d0 = 0; d0 < source.GetLength(0); d0++)
            {
                for (int d1 = 0; d1 < source.GetLength(1); d1++)
                {
                    for (int d2 = 0; d2 < source.GetLength(2); d2++)
                    {
                        clone[d0, d1, d2] = source[d0, d1, d2];
                    }
                }
            }
            return clone;
        }

        public override string ToString()
        {
            return Fitness.ToString();
        }

        internal void SetCourse(int course, int day, int block, int room)
        {
            Courses[course, day, block] = room;
        }

        internal void SetLecturer(int lecturer, int day, int block, int courseIndex)
        {
            Lecturers[lecturer, day, block] = courseIndex;
        }

        internal void SetLecturer(int[] lecturers, int day, int block, int course)
        {
            foreach (int lecturer in lecturers)
            {
                SetLecturer(lecturer, day, block, course);
            }
        }

        internal void SetLecturer(List<int> lecturers, int day, int block, int course)
        {
            foreach (int lecturer in lecturers)
            {
                SetLecturer(lecturer, day, block, course);
            }
        }

        internal void SetRoom(int room, int day, int block, int courseIndex)
        {
            Rooms[room, day, block] = courseIndex;
        }

        internal void SetGroup(int group, int day, int block, int courseIndex)
        {
            Groups[group, day, block] = courseIndex;
        }

        internal void ClearChromosome(int course, TimetableData ttData)
        {
            int room = GetRoom(course);
            int group = ttData.Courses[course].Group.Index;
            List<int> lecturers = GetLecturers(course, ttData);

            for (int day = 0; day < Courses.GetLength(1); day++)
            {
                for (int block = 0; block < Courses.GetLength(2); block++)
                {
                    SetCourse(course, day, block, -1);
                    if (Rooms[room, day, block] == course)
                        SetRoom(room, day, block, -1);
                    if (Groups[group, day, block] == course)
                        SetGroup(group, day, block, -1);
                    foreach (int lecturer in lecturers)
                    {
                        if (Lecturers[lecturer, day, block] == course)
                            SetLecturer(lecturer, day, block, -1);
                    }
                }
            }
        }

        internal void Clear()
        {
            Courses = InitializeChromosome(Courses.GetLength(0), Courses.GetLength(1), Courses.GetLength(2));
            Lecturers = InitializeChromosome(Lecturers.GetLength(0), Courses.GetLength(1), Courses.GetLength(2));
            Rooms = InitializeChromosome(Rooms.GetLength(0), Courses.GetLength(1), Courses.GetLength(2));
            Groups = InitializeChromosome(Groups.GetLength(0), Courses.GetLength(1), Courses.GetLength(2));
        }

        internal void SetChromosome(int course, int day, int block, int room, int group, List<int> lecturers)
        {
            SetCourse(course, day, block, room);
            SetGroup(group, day, block, course);
            SetLecturer(lecturers, day, block, course);
            SetRoom(room, day, block, course);
        }

        internal void SetChromosome(int course, int day, int block, int room, int group, int[] lecturers)
        {
            SetCourse(course, day, block, room);
            SetGroup(group, day, block, course);
            SetLecturer(lecturers, day, block, course);
            SetRoom(room, day, block, course);
        }

        private List<int> GetLecturers(int course, TimetableData ttData)
        {
            List<int> lecturers = new List<int>();
            for (int lecturer = 0; lecturer < ttData.Courses[course].Lecturers.Length; lecturer++)
            {
                lecturers.Add(ttData.Courses[course].Lecturers[lecturer].Index);
            }
            return lecturers;
        }

        private int GetRoom(int course)
        {
            for (int day = 0; day < Courses.GetLength(1); day++)
            {
                for (int block = 0; block < Courses.GetLength(2); block++)
                {
                    if (Courses[course, day, block] != -1)
                        return Courses[course, day, block];
                }
            }
            return -1;
        }
    }
}
