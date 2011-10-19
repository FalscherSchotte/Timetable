using TimetablePlanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TimetablePlannerTests
{
    [TestClass()]
    public class TimetableTest
    {
        [TestMethod()]
        public void createTimetableInstanceTest()
        {
            string configurationFile = "E:\\HsKA\\Semester2\\Projektarbeit\\Stundenplangenerator\\TimetablePlanner\\TimetablePlanner\\TimetableData.xml";
            TimetableData data = TimetableDataReader.createTimetableInstance(configurationFile);

            int populationSize = 75;
            int numberOfGenerations = 2000;

            long start = DateTime.Now.Ticks;
            TimetableGenerator generator = new TimetableGenerator(populationSize, data);
            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Time to create population of size " + populationSize + ": " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }


            //Print all groups
            foreach (Group g in data.Groups)
            {
                TimetablePrinter.printGroup(g.Index, generator.Population[0], data);
            }

            start = DateTime.Now.Ticks;
            generator.PerformEvolution(numberOfGenerations);
            end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Evolution time: " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

            //Print all groups
            foreach (Group g in data.Groups)
            {
                TimetablePrinter.printGroup(g.Index, generator.Population[0], data);
            }

            ////Print all lecturers
            //foreach (Lecturer l in data.Lecturers)
            //{
            //    TimetablePrinter.printLecturer(l.Index, generator.Population[0], data);
            //}

            Assert.IsTrue(true);
        }
    }
}
