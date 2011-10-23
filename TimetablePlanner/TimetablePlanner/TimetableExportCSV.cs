using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TimetablePlanner
{
    public class TimetableExportCSV
    {
        public static void ExportGroup(int groupIndex, Individual individual, TimetableData ttData, string filePath)
        {
            WriteCSV(filePath, GetGroupExportList(groupIndex, individual, ttData));
        }

        public static void ExportLecturer(int lecturerIndex, Individual individual, TimetableData ttData, string filePath)
        {
            WriteCSV(filePath, GetLecturerExportList(lecturerIndex, individual, ttData));
        }

        public static void ExportRoom(int roomIndex, Individual individual, TimetableData ttData, string filePath)
        {
            WriteCSV(filePath, GetRoomExportList(roomIndex, individual, ttData));
        }

        public static void ExportAllGroups(Individual individual, TimetableData ttData, string filePath)
        {
            List<List<string>> output = new List<List<string>>();
            for (int group = 0; group < ttData.Groups.Length; group++)
            {
                output.AddRange(GetGroupExportList(group, individual, ttData));
            }
            WriteCSV(filePath, output);
        }

        public static void ExportAllLecturers(Individual individual, TimetableData ttData, string filePath)
        {
            List<List<string>> output = new List<List<string>>();
            for (int lecuterer = 0; lecuterer < ttData.Lecturers.Length; lecuterer++)
            {
                output.AddRange(GetLecturerExportList(lecuterer, individual, ttData));
            }
            WriteCSV(filePath, output);
        }

        public static void ExportAllRooms(Individual individual, TimetableData ttData, string filePath)
        {
            List<List<string>> output = new List<List<string>>();
            for (int room = 0; room < ttData.Rooms.Length; room++)
            {
                output.AddRange(GetRoomExportList(room, individual, ttData));
            }
            WriteCSV(filePath, output);
        }

        public static void ExportAll(Individual individual, TimetableData ttData, string filePath)
        {
            List<List<string>> output = new List<List<string>>();
            for (int group = 0; group < ttData.Groups.Length; group++)
            {
                output.AddRange(GetGroupExportList(group, individual, ttData));
            }
            for (int lecuterer = 0; lecuterer < ttData.Lecturers.Length; lecuterer++)
            {
                output.AddRange(GetLecturerExportList(lecuterer, individual, ttData));
            }
            for (int room = 0; room < ttData.Rooms.Length; room++)
            {
                output.AddRange(GetRoomExportList(room, individual, ttData));
            }
            WriteCSV(filePath, output);
        }

        private static void WriteCSV(string filePath, List<List<string>> output)
        {
            StringBuilder sb = new StringBuilder();
            for (int column = 0; column < output.Count; column++)
            {
                sb.AppendLine(string.Join(";", output[column]));
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        private static List<List<string>> GetGroupExportList(int groupIndex, Individual individual, TimetableData ttData)
        {
            //output[row][col]
            List<List<string>> output = InitializeExportList(individual.Groups, ttData);
            //title
            output[0][0] = ttData.Groups[groupIndex].Name;

            //Fill in the data
            for (int day = 0; day < individual.Groups.GetLength(1); day++)
            {
                for (int block = 0; block < individual.Groups.GetLength(2); block++)
                {
                    if (individual.Groups[groupIndex, day, block] != -1)
                    {
                        Course c = ttData.Courses[individual.Groups[groupIndex, day, block]];
                        Room r = ttData.Rooms[individual.Courses[c.Index, day, block]];
                        output[block * 3 + 1][day + 1] = c.Name;
                        output[block * 3 + 2][day + 1] = r.Name;
                        for (int l = 0; l < c.Lecturers.Length; l++)
                        {
                            output[block * 3 + 3][day + 1] = output[block * 3 + 3][day + 1] + c.Lecturers[l].LastName + (l == c.Lecturers.Length - 1 ? "" : ", ");
                        }
                    }
                }
            }
            return output;
        }

        private static List<List<string>> GetLecturerExportList(int lecturerIndex, Individual individual, TimetableData ttData)
        {
            //output[row][col]
            List<List<string>> output = InitializeExportList(individual.Groups, ttData);
            //title
            output[0][0] = ttData.Lecturers[lecturerIndex].Name;

            //Fill in the data
            for (int day = 0; day < individual.Lecturers.GetLength(1); day++)
            {
                for (int block = 0; block < individual.Lecturers.GetLength(2); block++)
                {
                    if (individual.Lecturers[lecturerIndex, day, block] != -1)
                    {
                        Course c = ttData.Courses[individual.Lecturers[lecturerIndex, day, block]];
                        Room r = ttData.Rooms[individual.Courses[c.Index, day, block]];
                        output[block * 3 + 1][day + 1] = c.Name;
                        output[block * 3 + 2][day + 1] = r.Name;
                        for (int l = 0; l < c.Lecturers.Length; l++)
                        {
                            output[block * 3 + 3][day + 1] = output[block * 3 + 3][day + 1] + c.Lecturers[l].LastName + (l == c.Lecturers.Length - 1 ? "" : ", ");
                        }
                    }
                }
            }
            return output;
        }

        private static List<List<string>> GetRoomExportList(int roomIndex, Individual individual, TimetableData ttData)
        {
            //output[row][col]
            List<List<string>> output = InitializeExportList(individual.Groups, ttData);
            //title
            output[0][0] = ttData.Rooms[roomIndex].Name;

            //Fill in the data
            for (int day = 0; day < individual.Lecturers.GetLength(1); day++)
            {
                for (int block = 0; block < individual.Lecturers.GetLength(2); block++)
                {
                    if (individual.Rooms[roomIndex, day, block] != -1)
                    {
                        Course c = ttData.Courses[individual.Rooms[roomIndex, day, block]];
                        output[block * 3 + 1][day + 1] = c.Name;
                        output[block * 3 + 2][day + 1] = c.Group.Name;
                        for (int l = 0; l < c.Lecturers.Length; l++)
                        {
                            output[block * 3 + 3][day + 1] = output[block * 3 + 3][day + 1] + c.Lecturers[l].LastName + (l == c.Lecturers.Length - 1 ? "" : ", ");
                        }
                    }
                }
            }

            return output;
        }

        private static List<List<string>> InitializeExportList(int[, ,] elementData, TimetableData ttData)
        {
            List<List<string>> output = new List<List<string>>();

            //a column for each day and an additional row for the block display
            int colCount = elementData.GetLength(2) + 2;
            //a row for each block * 3 plus a row for day display and two extra rows for seperation to bottom
            int rowCount = (elementData.GetLength(1) + 1) * 3 + 3;
            for (int row = 0; row < rowCount; row++)
            {
                output.Add(new List<string>());
                for (int col = 0; col < colCount; col++)
                {
                    output[row].Add("");
                }
            }
            //blocks left
            for (int block = 0; block < ttData.Blocks.Length; block++)
            {
                output[block * 3 + 1][0] = ttData.Blocks[block].Start.ToShortTimeString() + "-" + ttData.Blocks[block].End.ToShortTimeString();
            }
            //days top
            for (int day = 0; day < elementData.GetLength(1); day++)
            {
                output[0][day + 1] = Enum.GetName(typeof(DayOfWeek), day + 1);
            }

            return output;
        }
    }
}
