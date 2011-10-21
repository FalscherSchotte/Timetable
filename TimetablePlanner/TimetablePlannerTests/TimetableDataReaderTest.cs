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

        //private bool TimeAndExecute(Delegate method, object[] methodParameters)
        //{
        //    try
        //    {
        //        long start = DateTime.Now.Ticks;
        //        method.DynamicInvoke(methodParameters);
        //        long end = DateTime.Now.Ticks;
        //        System.Diagnostics.Debug.WriteLine(String.Format("{0} finished after {1}ms", method.ToString(), (end - start) / 10000));
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //        return false;
        //    }
        //}

        [TestMethod()]
        public void TimetableManualTester()
        {
            String configurationFile = basePath + "TimetableData.xml";
            TimetableData ttData = TimetableDataReader.createTimetableInstance(configurationFile);
            if (ttData == null)
            {
                Assert.IsTrue(false, "Data could not be loaded.");
                return;
            }

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

            for (int pIndex = 0; pIndex < generator.Population.Length; pIndex++)
            {
                Assert.AreEqual(0, GetCourseDiff(ttData, generator.Population[pIndex]), 
                    "Not the expected number of courses found for individual " + pIndex + "!");
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

            Assert.IsTrue(true);
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
    }
}
