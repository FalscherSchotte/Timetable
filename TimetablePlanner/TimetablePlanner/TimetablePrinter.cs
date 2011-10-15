using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class TimetablePrinter
    {
        public static void PrintTable(Individual individual, TimetableData ttData)
        {
            foreach (Lecturer lecturer in ttData.Lecturers)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("############");
                builder.AppendLine(lecturer.LastName);
                for (int day = 0; day < 5; day++)
                {
                    builder.AppendLine("Tag " + day);
                    for (int room = 0; room < ttData.Rooms.Length; room++)
                    {
                        for (int block = 0; block < ttData.Blocks.Length; block++)
                        {
                            for (int lecturerIndex = 0; lecturerIndex < ttData.Lecturers.Length; lecturerIndex++)
                            {
                                if (individual.LecturerChromosomes[day][room][block][lecturerIndex] == lecturer.Index)
                                {
                                    builder.AppendLine("Block " + block + " in Raum " + ttData.Rooms[room].ToString() + " Kurs " + 
                                        ttData.Courses[individual.CourseChromosomes[day][room][block]].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine(builder.ToString());
            }
        }
    }
}
