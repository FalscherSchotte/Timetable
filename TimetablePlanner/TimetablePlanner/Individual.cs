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
        /// Days\Rooms\Blocks\Courses
        /// </summary>
        public List<List<List<short>>> CourseChromosomes { get; set; }

        /// <summary>
        /// Days\Rooms\Blocks\Lecturers\free|occupied
        /// </summary>
        public List<List<List<List<bool>>>> LecturerChromosomes { get; set; }

        public Individual(List<List<List<short>>> courseChromosomes, List<List<List<List<bool>>>> lecturerChromosomes)
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
                        builder.Append(String.Format("{0:000},", CourseChromosomes[i][j][k]));
                    }
                }
            }
            return builder.ToString();
        }

        internal void SetCourseAt(int day, int room, int block, short courseIndex, short lecturerIndex)
        {
            CourseChromosomes[day][room][block] = courseIndex;
            LecturerChromosomes[day][room][block][lecturerIndex] = true;
        }
    }
}
