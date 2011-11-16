using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Text;

namespace TimetablePlanner
{
    public class TimetableConfigIO
    {
        #region Import

        public static TimetableData ImportTimetableConfig(string configurationFilePath)
        {
            FileStream stream = null;
            XPathDocument document = null;
            XPathNavigator navigator = null;
            try
            {
                stream = new FileStream(configurationFilePath, FileMode.Open);
                document = new XPathDocument(stream);
                navigator = document.CreateNavigator();

                Block[] blocks = ParseBlocks(navigator);
                Lecturer[] lecturers = ParseLecturers(navigator);
                Room[] rooms = ParseRooms(navigator);
                Group[] groups = ParseGroups(navigator);
                Course[] courses = ParseCources(navigator, lecturers, rooms, groups);

                return new TimetableData(blocks, courses, lecturers, rooms, groups);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        private static Block[] ParseBlocks(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/blocks/block");
            List<Block> blocks = new List<Block>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string start = node.GetAttribute("start", "");
                string end = node.GetAttribute("end", "");
                List<DayOfWeek> exceptions = new List<DayOfWeek>();
                if (node.HasChildren)
                {
                    foreach (XPathNavigator subNode in node.SelectChildren("exception", ""))
                    {
                        exceptions.Add(ParseDayOfWeek(subNode.GetAttribute("day", "")));
                    }
                }
                blocks.Add(new Block(DateTime.Parse(start), DateTime.Parse(end), exceptions.ToArray()));
            }
            return blocks.ToArray();
        }

        private static Lecturer[] ParseLecturers(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/lecturers/lecturer");
            List<Lecturer> lecturers = new List<Lecturer>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string id = node.GetAttribute("lId", "");
                string lastName = node.GetAttribute("lastName", "");
                string firstName = node.GetAttribute("firstName", "");
                bool isDummy = ParseBoolFromString(node.GetAttribute("isDummy", ""), false);
                int numberOfReasearchDays = ParseIntFromString(node.GetAttribute("numberOfResearchDays", ""), 1);

                List<DayOfWeek> researchExceptions = new List<DayOfWeek>();
                foreach (XPathNavigator subNode in node.SelectChildren("researchException", ""))
                {
                    researchExceptions.Add(ParseDayOfWeek(subNode.GetAttribute("day", "")));
                }

                lecturers.Add(new Lecturer(id, lastName, firstName, researchExceptions, numberOfReasearchDays, isDummy));
            }
            return lecturers.ToArray();
        }

        private static Room[] ParseRooms(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/rooms/room");
            List<Room> rooms = new List<Room>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string number = node.GetAttribute("number", "");
                string isLab = node.GetAttribute("isLab", "");
                rooms.Add(new Room(number, ParseBoolFromString(isLab, false)));
            }
            return rooms.ToArray();
        }

        private static Group[] ParseGroups(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/groups/group");
            List<Group> groups = new List<Group>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string id = node.GetAttribute("id", "");
                groups.Add(new Group(id, id));
            }
            return groups.ToArray();
        }

        private static Course[] ParseCources(XPathNavigator navigator, Lecturer[] lecturers, Room[] rooms, Group[] groups)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/courses/course");
            List<Course> courses = new List<Course>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string cId = node.GetAttribute("cId", "");
                string name = node.GetAttribute("name", "");
                bool needsLab = ParseBoolFromString(node.GetAttribute("needsLab", ""), false);
                bool isDummy = ParseBoolFromString(node.GetAttribute("isDummy", ""), false);
                int numberOfBlocks = ParseIntFromString(node.GetAttribute("numberOfBlocks", ""), 1);
                int repeatsPerWeek = ParseIntFromString(node.GetAttribute("repeatsPerWeek", ""), 1);

                List<Lecturer> lecturerList = new List<Lecturer>();
                Lecturer l0 = GetLecturerWithID(node.GetAttribute("lId0", ""), lecturers);
                Lecturer l1 = GetLecturerWithID(node.GetAttribute("lId1", ""), lecturers);
                lecturerList.Add(l0);
                if (l1 != null)
                    lecturerList.Add(l1);

                string roomPreference = node.GetAttribute("roomPreference", "");
                Room preference = null;
                foreach (Room room in rooms)
                {
                    if (room.Name.Equals(roomPreference))
                    {
                        preference = room;
                        break;
                    }
                }

                string gId = node.GetAttribute("gId", "");
                Group group = null;
                foreach (Group g in groups)
                {
                    if (g.Id.Equals(gId))
                    {
                        group = g;
                        break;
                    }
                }

                for (int repetition = 0; repetition < repeatsPerWeek; repetition++)
                {
                    courses.Add(new Course(cId, name, lecturerList.ToArray(), preference, group, needsLab, isDummy, numberOfBlocks));
                }

            }
            return courses.ToArray();
        }

        private static Lecturer GetLecturerWithID(string id, Lecturer[] lecturers)
        {
            foreach (Lecturer l in lecturers)
            {
                if (l.Id.Equals(id))
                    return l;
            }
            return null;
        }

        private static DayOfWeek ParseDayOfWeek(string day)
        {
            switch (day.ToLower())
            {
                case "monday": return DayOfWeek.Monday;
                case "tuesday": return DayOfWeek.Tuesday;
                case "wednesday": return DayOfWeek.Wednesday;
                case "thursday": return DayOfWeek.Thursday;
                case "friday": return DayOfWeek.Friday;
            }
            throw new Exception("Day of week not recognized");
        }

        private static bool ParseBoolFromString(string value, bool exceptionValue)
        {
            return value.Length > 0 ? bool.Parse(value) : exceptionValue;
        }

        private static int ParseIntFromString(string data, int exceptionValue)
        {
            if (data != null && data.Length > 0)
                return int.Parse(data);
            return exceptionValue;
        }

        #endregion

        #region Export

        public static bool ExportTimetableConfig(TimetableData ttData, string configurationFilePath)
        {
            try
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                output.AppendLine("<timetableData>");

                AppendBlocks(ttData.Blocks, output);
                AppendLecturers(ttData.Lecturers, output);
                AppendRooms(ttData.Rooms, output);
                AppendGroups(ttData.Groups, output);
                AppendCourses(ttData.Courses, output);

                output.AppendLine("</timetableData>");
                File.WriteAllText(configurationFilePath, output.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Export failed! " + ex.Message);
                return false;
            }
        }

        private static void AppendCourses(Course[] courses, StringBuilder output)
        {
            Dictionary<Course, short> coursesDict = new Dictionary<Course, short>();
            foreach (Course c in courses)
            {
                Course listedCourse = null;
                if (coursesDict.Keys.Count > 0)
                {
                    foreach (Course k in coursesDict.Keys)
                    {
                        if (k.Id.Equals(c.Id))
                        {
                            listedCourse = k;
                            break;
                        }
                    }
                }
                if (listedCourse == null)
                    coursesDict.Add(c, 1);
                else
                    coursesDict[listedCourse]++;
            }

            output.Append("<courses>");
            foreach (Course c in coursesDict.Keys)
            {
                output.Append("<course cId=\"" + c.Id + "\"");
                output.Append(" gId=\"" + c.Group.Id + "\"");
                output.Append(" repeatsPerWeek=\"" + coursesDict[c] + "\"");
                output.Append(" numberOfBlocks=\"" + c.NumberOfBlocks + "\"");
                output.Append(" needsLab=\"" + c.NeedsLab + "\"");
                output.Append(" name=\"" + c.Name + "\"");
                output.Append(" isDummy=\"" + c.IsDummy + "\"");
                if (c.RoomPreference != null)
                    output.Append(" roomPreference=\"" + c.RoomPreference + "\"");
                for (int i = 0; i < c.Lecturers.Length; i++)
                {
                    output.Append(" lId" + i + "=\"" + c.Lecturers[i].Id + "\"");
                }
                output.Append("/>");
            }
            output.Append("</courses>");
        }

        private static void AppendGroups(Group[] groups, StringBuilder output)
        {
            output.Append("<groups>");
            foreach (Group g in groups)
            {
                output.Append("<group id=\"" + g.Id + "\"/>");
            }
            output.Append("</groups>");
        }

        private static void AppendRooms(Room[] rooms, StringBuilder output)
        {
            output.Append("<rooms>");
            foreach (Room r in rooms)
            {
                output.Append("<room");
                output.Append(" number=\"" + r.Name + "\"");
                output.Append(" isLab=\"" + r.IsLab + "\"");
                output.Append("/>");
            }
            output.Append("</rooms>");
        }

        private static void AppendLecturers(Lecturer[] lecturers, StringBuilder output)
        {
            output.Append("<lecturers>");
            foreach (Lecturer l in lecturers)
            {
                output.Append("<lecturer");
                output.Append(" lId=\"" + l.Id + "\"");
                output.Append(" lastName=\"" + l.LastName + "\"");
                if (l.FirstName != null && l.FirstName.Length > 0)
                    output.Append(" firstName=\"" + l.FirstName + "\"");
                if (l.NeededNumberOfResearchDays > 0)
                    output.Append(" numberOfResearchDays=\"" + l.NeededNumberOfResearchDays + "\"");
                if (l.IsDummy)
                    output.Append(" isDummy=\"" + l.IsDummy + "\"");
                output.Append(">");
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if ((int)day > 0 && (int)day < 6 && !l.AvailableResearchDays.Contains((int)day - 1))
                        output.Append("<researchException day=\"" + day.ToString() + "\"/>");
                }
                output.Append("</lecturer>");
            }
            output.Append("</lecturers>");
        }

        private static void AppendBlocks(Block[] blocks, StringBuilder output)
        {
            output.Append("<blocks>");
            foreach (Block b in blocks)
            {
                output.Append("<block ");
                output.Append("start=\"" + CreateTimeString(b.Start) + "\" ");
                output.Append("end=\"" + CreateTimeString(b.End) + "\">");
                foreach (DayOfWeek exception in b.Exceptions)
                {
                    output.Append("<exception day=\"" + exception.ToString() + "\" />");
                }
                output.Append("</block>");
            }
            output.Append("</blocks>");
        }

        private static string CreateTimeString(DateTime time)
        {
            return time.Hour.ToString("00") + ":" + time.Minute.ToString("00");
        }

        #endregion
    }
}