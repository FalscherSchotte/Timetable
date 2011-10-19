using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class TimetablePrinter
    {
        public static void printGroup(int group, Individual i, TimetableData ttData)
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Group " + group);
            for (int block = 0; block < i.Courses.GetLength(2); block++)
            {
                System.Diagnostics.Debug.Write("Block " + block + ": ");
                for (int day = 0; day < 5; day++)
                {
                    int course = i.Groups[group, day, block];
                    if (course == -1)
                        System.Diagnostics.Debug.Write("--- ");
                    else
                        System.Diagnostics.Debug.Write(course.ToString().PadLeft(3, '0') + " ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void printLecturer(int lecturer, Individual i, TimetableData ttData)
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Lecturer " + lecturer);
            for (int block = 0; block < i.Courses.GetLength(2); block++)
            {
                System.Diagnostics.Debug.Write("Block " + block + ": ");
                for (int day = 0; day < 5; day++)
                {
                    int course = i.Lecturers[lecturer, day, block];
                    if (course == -1)
                        System.Diagnostics.Debug.Write("--- ");
                    else
                        System.Diagnostics.Debug.Write(course.ToString().PadLeft(3, '0') + " ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }
    }
}
