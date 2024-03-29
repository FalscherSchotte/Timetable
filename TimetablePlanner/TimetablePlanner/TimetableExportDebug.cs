﻿
namespace TimetablePlanner
{
    public class TimetableExportDebug
    {
        public static void PrintGroup(int group, Individual i, TimetableData ttData)
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
                    {
                        for (int j = 0; j < 44; j++)
                        {
                            System.Diagnostics.Debug.Write("-");
                        }
                    }
                    else
                    {
                        string output = ttData.Courses[course].Name + ", " + ttData.Rooms[i.Courses[course, day, block]].Id;
                        System.Diagnostics.Debug.Write(output.PadRight(44, ' '));
                    }
                    System.Diagnostics.Debug.Write("| ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void PrintLecturer(int lecturer, Individual i, TimetableData ttData)
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
                        System.Diagnostics.Debug.Write(course.ToString().PadLeft(3, ttData.Courses[course].IsDummy ? 'x' : '0') + " ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }
    }
}
