using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Individual
    {
        private int maxIndex = -1;

        public int Fitness { get; set; }

        /// <summary>
        /// Days\Rooms\Blocks\Courses
        /// </summary>
        public List<List<List<short>>> CourseChromosomes { get; set; }

        /// <summary>
        /// Days\Rooms\Blocks\Lecturers\CourseIndex
        /// </summary>
        public List<List<List<List<short>>>> LecturerChromosomes { get; set; }

        public Individual(List<List<List<short>>> courseChromosomes, List<List<List<List<short>>>> lecturerChromosomes)
        {
            this.CourseChromosomes = courseChromosomes;
            this.LecturerChromosomes = lecturerChromosomes;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("{0:0000}: ", Fitness));
            for (int i = 0; i < CourseChromosomes.Count; i++)
            {
                for (int j = 0; j < CourseChromosomes[i].Count; j++)
                {
                    for (int k = 0; k < CourseChromosomes[i][j].Count; k++)
                    {
                        if (CourseChromosomes[i][j][k] >= 0)
                            builder.Append(String.Format("{0:000}|", CourseChromosomes[i][j][k]));
                        else
                            builder.Append("---|");
                    }
                }
            }
            return builder.ToString();
        }

        internal void SetCourseAt(int day, int room, int block, short courseIndex, short lecturerIndex)
        {
            CourseChromosomes[day][room][block] = courseIndex;
            LecturerChromosomes[day][room][block][lecturerIndex] = courseIndex;
        }

        internal Individual Clone()
        {
            // Days\Rooms\Blocks\Courses
            List<List<List<short>>> courseChromosomesClone = new List<List<List<short>>>(); ;
            for (int day = 0; day < CourseChromosomes.Count; day++)
            {
                courseChromosomesClone.Add(new List<List<short>>());
                for (int room = 0; room < CourseChromosomes[day].Count; room++)
                {
                    courseChromosomesClone[day].Add(new List<short>());
                    for (int block = 0; block < CourseChromosomes[day][room].Count; block++)
                    {
                        courseChromosomesClone[day][room].Add(CourseChromosomes[day][room][block]);
                    }
                }
            }

            // Days\Rooms\Blocks\Lecturers\CourseIndex
            List<List<List<List<short>>>> lecturerChromosomesClone = new List<List<List<List<short>>>>(); ;
            for (int day = 0; day < LecturerChromosomes.Count; day++)
            {
                lecturerChromosomesClone.Add(new List<List<List<short>>>());
                for (int room = 0; room < LecturerChromosomes[day].Count; room++)
                {
                    lecturerChromosomesClone[day].Add(new List<List<short>>());
                    for (int block = 0; block < LecturerChromosomes[day][room].Count; block++)
                    {
                        lecturerChromosomesClone[day][room].Add(new List<short>());
                        for (int lecturer = 0; lecturer < LecturerChromosomes[day][room][block].Count; lecturer++)
                        {
                            lecturerChromosomesClone[day][room][block].Add(LecturerChromosomes[day][room][block][lecturer]);
                        }
                    }
                }
            }

            return new Individual(courseChromosomesClone, lecturerChromosomesClone);
        }
    }
}
