using TimetablePlanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace TimetablePlannerTests
{
    [TestClass()]
    public class TimetableTest
    {
        private readonly String basePath = "E:\\HsKA\\Semester2\\Projektarbeit\\Stundenplangenerator\\TimetablePlanner\\TimetablePlannerTests\\";

        [TestMethod()]
        public void TimetableManualTester()
        {
            String configurationFile = basePath + "TimetableData.xml";
            TimetableData ttData = TimetableDataReader.createTimetableInstance(configurationFile);
            Assert.IsNotNull(ttData, "Data could not be loaded.");

            int populationSize = 75;
            int numberOfGenerations = 2000;

            long start = DateTime.Now.Ticks;
            TimetableGenerator generator = new TimetableGenerator(populationSize, ttData);
            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Time to create population of size " + populationSize + ": " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

            start = DateTime.Now.Ticks;
            generator.PerformEvolution(numberOfGenerations);
            end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Evolution time: " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

            //Inspect every result population
            for (int pIndex = 0; pIndex < generator.Population.Length; pIndex++)
            {
                //Result must contain all defined courses
                Assert.AreEqual(0, GetCourseDiff(ttData, generator.Population[pIndex]),
                    "Not the expected number of courses found for individual " + pIndex + "!");

                for (int lIndex = 0; lIndex < ttData.Lecturers.Length; lIndex++)
                {
                    if (!ttData.Lecturers[lIndex].IsDummy)
                    {
                        //Every lecturer must have the defined number of courses
                        int expectedCourseCount = GetExpectedCourseCountForLecturer(ttData.Lecturers[lIndex], ttData);
                        int foundCourseCount = GetCourseCountForLecturer(generator.Population[pIndex], lIndex);
                        Assert.AreEqual(expectedCourseCount, foundCourseCount, "Not the expected number of courses found for lecturer " + lIndex + "!");

                        //Free days for research for lecturers
                        int freeDaysForLecturer = GetFreeDaysForLecturer(generator.Population[pIndex], lIndex, ttData);
                        Assert.IsTrue(ttData.Lecturers[lIndex].NeededNumberOfResearchDays <= freeDaysForLecturer,
                            "Not enough free days for research for lecturer " + lIndex);
                    }
                }

                for (int cIndex = 0; cIndex < ttData.Courses.Length; cIndex++)
                {
                    //Every course must occupy the specified room (course -> room -> course)
                    Assert.IsTrue(CourseHasRoom(cIndex, generator.Population[pIndex].Courses, generator.Population[pIndex].Rooms, ttData),
                        "Course " + cIndex + " has no room!");
                }

                for (int gIndex = 0; gIndex < ttData.Groups.Length; gIndex++)
                {
                    //Number of courses for each group must be as defined
                    int expectedCourseCount = GetExpectedCourseCountForGroup(ttData.Groups[gIndex], ttData);
                    int foundCourseCount = GetCourseCountForGroup(gIndex, generator.Population[pIndex]);
                    Assert.AreEqual(expectedCourseCount, foundCourseCount, "Not the expected number of courses for group " + gIndex + "!");
                }

                for (int b = 0; b < ttData.Blocks.Length; b++)
                {
                    //A defined exception block must not be occupied
                    foreach (var exceptionDay in ttData.Blocks[b].Exceptions)
                    {
                        for (int course = 0; course < generator.Population[pIndex].Courses.GetLength(0); course++)
                        {
                            Assert.AreEqual(-1, generator.Population[pIndex].Courses[course, (int)exceptionDay - 1, b],
                                "Course found for blockexception on day " + exceptionDay + ", block " + b + " !");
                        }
                    }
                }
            }


            //Print all groups
            foreach (Group g in ttData.Groups)
            {
                TimetablePrinter.printGroup(g.Index, generator.Population[0], ttData);
            }

            //Print all lecturers
            //foreach (Lecturer l in data.Lecturers)
            //{
            //    TimetablePrinter.printLecturer(l.Index, generator.Population[0], data);
            //}

            TimetableCSVExporter.ExportAll(generator.Population[0], ttData, basePath + "outputtest.csv");

            Assert.IsTrue(true);
        }

        private static int GetFreeDaysForLecturer(Individual individual, int lIndex, TimetableData ttData)
        {
            int freeDayCount = 0;
            for (int d = 0; d < individual.Lecturers.GetLength(1); d++)
            {
                if (ttData.Lecturers[lIndex].AvailableResearchDays.Contains(d))
                {
                    bool isFree = true;
                    for (int b = 0; b < individual.Lecturers.GetLength(2); b++)
                    {
                        if (individual.Lecturers[lIndex, d, b] != -1)
                        {
                            isFree = false;
                        }
                    }
                    if (isFree)
                        freeDayCount++;
                }
            }
            return freeDayCount;
        }

        private static int GetCourseCountForGroup(int gIndex, Individual individual)
        {
            int count = 0;
            for (int d = 0; d < individual.Groups.GetLength(1); d++)
            {
                for (int b = 0; b < individual.Groups.GetLength(2); b++)
                {
                    if (individual.Groups[gIndex, d, b] != -1)
                        count++;
                }
            }
            return count;
        }

        private static int GetExpectedCourseCountForGroup(Group group, TimetableData ttData)
        {
            int count = 0;
            for (int cIndex = 0; cIndex < ttData.Courses.Length; cIndex++)
            {
                if (ttData.Courses[cIndex].Group.Index == group.Index)
                    count += ttData.Courses[cIndex].NumberOfBlocks;
            }
            return count;
        }

        private static bool CourseHasRoom(int cIndex, int[, ,] courses, int[, ,] rooms, TimetableData ttData)
        {
            for (int d = 0; d < courses.GetLength(1); d++)
            {
                for (int b = 0; b < courses.GetLength(2); b++)
                {
                    if (courses[cIndex, d, b] != -1)
                    {
                        if (rooms[courses[cIndex, d, b], d, b] != cIndex)
                            return false;
                        if (ttData.Rooms[courses[cIndex, d, b]].IsLab != ttData.Courses[cIndex].NeedsLab)
                            return false;
                    }
                }
            }
            return true;
        }

        private static int GetCourseDiff(TimetableData ttData, Individual individual)
        {
            int numberOfCoursesExpected = 0;
            for (int c = 0; c < ttData.Courses.Length; c++)
            {
                numberOfCoursesExpected += ttData.Courses[c].NumberOfBlocks;
            }
            int numberOfCoursesFound = 0;
            for (int c = 0; c < individual.Courses.GetLength(0); c++)
            {
                for (int d = 0; d < individual.Courses.GetLength(1); d++)
                {
                    for (int b = 0; b < individual.Courses.GetLength(2); b++)
                    {
                        if (individual.Courses[c, d, b] != -1)
                            numberOfCoursesFound++;
                    }
                }
            }
            return numberOfCoursesExpected - numberOfCoursesFound;
        }

        private static int GetExpectedCourseCountForLecturer(Lecturer lecturer, TimetableData ttData)
        {
            int count = 0;
            foreach (Course c in ttData.Courses)
            {
                for (int lIndex = 0; lIndex < c.Lecturers.Length; lIndex++)
                {
                    if (c.Lecturers[lIndex].Index == lecturer.Index)
                        count += c.NumberOfBlocks;
                }
            }
            return count;
        }

        private static int GetCourseCountForLecturer(Individual individual, int lIndex)
        {
            int count = 0;
            for (int d = 0; d < individual.Lecturers.GetLength(1); d++)
            {
                for (int b = 0; b < individual.Lecturers.GetLength(2); b++)
                {
                    if (individual.Lecturers[lIndex, d, b] != -1)
                        count++;
                }
            }
            return count;
        }

    }
}
