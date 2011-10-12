using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace TimetablePlanner
{
    public class TimetableDataReader
    {
        public static TimetableData createTimetableInstance(string configurationFile)
        {
            FileStream stream = null;
            XPathDocument document = null;
            XPathNavigator navigator = null;

            try
            {
                stream = new FileStream(configurationFile, FileMode.Open);
                document = new XPathDocument(stream);
                navigator = document.CreateNavigator();

                Block[] blocks = parseBlocks(navigator);
                Lecturer[] lecturers = parseLecturers(navigator);
                Room[] rooms = parseRooms(navigator);
                Group[] groups = parseGroups(navigator);
                Course[] courses = parseCources(navigator, lecturers, rooms, groups);

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

        private static Block[] parseBlocks(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/parameters/blocks/block");
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
                        switch (subNode.GetAttribute("day", "").ToLower())
                        {
                            case "monday": exceptions.Add(DayOfWeek.Monday); break;
                            case "tuesday": exceptions.Add(DayOfWeek.Tuesday); break;
                            case "wednesday": exceptions.Add(DayOfWeek.Wednesday); break;
                            case "thursday": exceptions.Add(DayOfWeek.Thursday); break;
                            case "friday": exceptions.Add(DayOfWeek.Friday); break;
                        }
                    }
                }
                blocks.Add(new Block(DateTime.Parse(start), DateTime.Parse(end), exceptions.ToArray()));
            }
            return blocks.ToArray();
        }

        private static Lecturer[] parseLecturers(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/lecturers/lecturer");
            List<Lecturer> lecturers = new List<Lecturer>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string id = node.GetAttribute("pId", "");
                string lastName = node.GetAttribute("lastName", "");
                lecturers.Add(new Lecturer(id, lastName));
            }
            return lecturers.ToArray();
        }

        private static Room[] parseRooms(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/rooms/room");
            List<Room> rooms = new List<Room>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string number = node.GetAttribute("number", "");
                string isLab = node.GetAttribute("isLab", "");
                rooms.Add(new Room(number, isLab.Length > 0 ? bool.Parse(isLab) : false));
            }
            return rooms.ToArray();
        }

        private static Group[] parseGroups(XPathNavigator navigator)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/groups/group");
            List<Group> groups = new List<Group>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string id = node.GetAttribute("id", "");
                groups.Add(new Group(id));
            }
            return groups.ToArray();
        }

        private static Course[] parseCources(XPathNavigator navigator, Lecturer[] lecturers, Room[] rooms, Group[] groups)
        {
            XPathNodeIterator nodeIterator = navigator.Select("timetableData/courses/course");
            List<Course> courses = new List<Course>();
            foreach (XPathNavigator node in nodeIterator)
            {
                string cId = node.GetAttribute("cId", "");

                string name = node.GetAttribute("name", "");
                string needsLab = node.GetAttribute("needsLab", "");
                string isDummy = node.GetAttribute("isDummy", "");
                string numberOfBlocks = node.GetAttribute("numberOfBlocks", "");
                string repeatsPerWeek = node.GetAttribute("repeatsPerWeek", "");

                string pId = node.GetAttribute("pId", "");
                string pId2 = node.GetAttribute("pId2", "");
                List<Lecturer> courseLecturers = new List<Lecturer>();
                bool p1Found = false;
                bool p2Found = pId2.Length == 0;
                foreach (Lecturer lecturer in lecturers)
                {
                    if (lecturer.Id.Equals(pId))
                    {
                        courseLecturers.Add(lecturer);
                        p1Found = true;
                    }
                    if (lecturer.Id.Equals(pId2))
                    {
                        courseLecturers.Add(lecturer);
                        p2Found = true;
                    }
                    if (p1Found && p2Found)
                        break;
                }
                
                string roomPreference = node.GetAttribute("roomPreference", "");
                Room preference = null;
                foreach (Room room in rooms)
                {
                    if (room.Number.Equals(roomPreference))
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

                courses.Add(new Course(cId, name, courseLecturers.ToArray(), preference, group,
                    needsLab.Length > 0 ? bool.Parse(needsLab) : false, isDummy.Length > 0 ? bool.Parse(isDummy) : false,
                    repeatsPerWeek.Length > 0 ? int.Parse(repeatsPerWeek) : 1, numberOfBlocks.Length > 0 ? int.Parse(numberOfBlocks) : 1));
            }
            return courses.ToArray();
        }

    }
}