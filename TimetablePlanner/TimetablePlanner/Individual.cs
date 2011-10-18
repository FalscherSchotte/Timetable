using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Individual
    {
        public int Fitness { get; set; }

        /// <summary>
        /// [CourseIndex][DayIndex][BlockIndex] -> RoomIndex
        /// </summary>
        public short[, ,] Courses { get; private set; }

        /// <summary>
        /// [LecturerIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public short[, ,] Lecturers { get; private set; }

        /// <summary>
        /// [RoomIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public short[, ,] Rooms { get; private set; }

        /// <summary>
        /// [GroupIndex][DayIndex][BlockIndex] -> CourseIndex
        /// </summary>
        public short[, ,] Groups { get; private set; }

        public Individual(short numberOfDays, short numberOfBlocks, short numberOfCourses, short numberOfLecturers, short numberOfRooms, short numberOfGroups)
        {
            Courses = InitializeChromosome(numberOfCourses, numberOfDays, numberOfBlocks);
            Lecturers = InitializeChromosome(numberOfLecturers, numberOfDays, numberOfBlocks);
            Rooms = InitializeChromosome(numberOfRooms, numberOfDays, numberOfBlocks);
            Groups = InitializeChromosome(numberOfGroups, numberOfDays, numberOfBlocks);
        }

        private Individual()
        { }

        private static short[, ,] InitializeChromosome(short numberOfBase, short numberOfDays, short numberOfBlocks)
        {
            short[, ,] chromosome = new short[numberOfBase, numberOfDays, numberOfBlocks];
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

        private static short[, ,] Clone(short[, ,] source)
        {
            short[, ,] clone = new short[source.GetLength(0), source.GetLength(1), source.GetLength(2)];
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
            //StringBuilder builder = new StringBuilder();
            //builder.Append(String.Format("{0:0000}: ", Fitness));
            //for (int i = 0; i < CourseChromosomes.Count; i++)
            //{
            //    for (int j = 0; j < CourseChromosomes[i].Count; j++)
            //    {
            //        for (int k = 0; k < CourseChromosomes[i][j].Count; k++)
            //        {
            //            if (CourseChromosomes[i][j][k] >= 0)
            //                builder.Append(String.Format("{0:000}|", CourseChromosomes[i][j][k]));
            //            else
            //                builder.Append("---|");
            //        }
            //    }
            //}
            //return builder.ToString();
        }

        internal void SetCourse(short course, short day, short block, short room)
        {
            Courses[course, day, block] = room;
        }

        internal void SetLecturer(short lecturer, short day, short block, short courseIndex)
        {
            Lecturers[lecturer, day, block] = courseIndex;
        }

        internal void SetRoom(short room, short day, short block, short courseIndex)
        {
            Rooms[room, day, block] = courseIndex;
        }

        internal void SetGroup(short group, short day, short block, short courseIndex)
        {
            Groups[group, day, block] = courseIndex;
        }
    }
}
