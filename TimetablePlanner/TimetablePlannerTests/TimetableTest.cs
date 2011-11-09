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
        public void TimetableInputExportTest()
        {
            TimetableData ttData1 = null;
            Assert.IsNotNull((ttData1 = TimetableConfigIO.ImportTimetableConfig(basePath + "TimetableDataExtended.xml")));

            Assert.IsTrue(TimetableConfigIO.ExportTimetableConfig(ttData1, basePath + "TimetableTestExport.xml"));

            TimetableData ttData2 = null;
            Assert.IsNotNull((ttData2 = TimetableConfigIO.ImportTimetableConfig(basePath + "TimetableTestExport.xml")));

            Assert.AreEqual(ttData1.Blocks.Length, ttData2.Blocks.Length);
            Assert.AreEqual(ttData1.Groups.Length, ttData2.Groups.Length);
            Assert.AreEqual(ttData1.Lecturers.Length, ttData2.Lecturers.Length);
            Assert.AreEqual(ttData1.Rooms.Length, ttData2.Rooms.Length);
            Assert.AreEqual(ttData1.Courses.Length, ttData2.Courses.Length);
        }

        [TestMethod()]
        public void TimetableManualTester()
        {
            String configurationFile = basePath + "TimetableDataExtended.xml";
            TimetableData ttData = TimetableConfigIO.ImportTimetableConfig(configurationFile);
            Assert.IsNotNull(ttData, "Data could not be loaded.");

            TimetableGenerator generator = new TimetableGenerator(ttData);
            generator.IndividualCreated += new Action<int>(generator_IndividualCreated);
            generator.GenerationTick += new Action<TimetableGenerator.HistoryEntry>(generator_GenerationTick);

            DateTime start = DateTime.Now;

            generator.CreatePopulation(50, 10);
            TestRequirements(ttData, generator);
            System.Diagnostics.Debug.WriteLine("Population created after " + (DateTime.Now - start).TotalSeconds + " seconds.");

            generator.PerformEvolution(1000);
            TestRequirements(ttData, generator);
            System.Diagnostics.Debug.WriteLine("Evolution finished after " + (DateTime.Now - start).TotalSeconds + " seconds.");

            TimetableExportCSV.ExportAll(generator.Population[0], ttData, basePath + "outputtest.csv");
            Assert.IsTrue(true);
        }

        private void generator_GenerationTick(TimetableGenerator.HistoryEntry generationData)
        {
            System.Diagnostics.Debug.WriteLine("Generation " + generationData.Index + " finished with best fitness of " + generationData.BestFitness + ".");
        }

        private void generator_IndividualCreated(int individualIndex)
        {
            System.Diagnostics.Debug.WriteLine("Individual " + individualIndex + " created.");
        }

        private static void TestRequirements(TimetableData ttData, TimetableGenerator generator)
        {
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
                    //Every course must occupy the specified room (course -> room -> course) and must be a room (room != -1)
                    Assert.IsTrue(CourseHasRoom(cIndex, generator.Population[pIndex].Courses, generator.Population[pIndex].Rooms, ttData),
                        "Course " + cIndex + " has no room!");

                    int ctr = CountRoomsForCourse(generator, pIndex, cIndex);
                    Assert.AreEqual(ttData.Courses[cIndex].NumberOfBlocks, ctr, "Not enough rooms specified for course " + cIndex);
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
        }

        private static int CountRoomsForCourse(TimetableGenerator generator, int pIndex, int cIndex)
        {
            int ctr = 0;
            for (int d = 0; d < generator.Population[pIndex].Courses.GetLength(1); d++)
            {
                for (int b = 0; b < generator.Population[pIndex].Courses.GetLength(2); b++)
                {
                    if (generator.Population[pIndex].Courses[cIndex, d, b] != -1)
                        ctr++;
                }
            }
            return ctr;
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
