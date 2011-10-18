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

            int populationSize = 50;
            int numberOfGenerations = 1000;

            long start = DateTime.Now.Ticks;
            TimetableGenerator generator = new TimetableGenerator(numberOfGenerations, populationSize, data);
            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Time to create population of size " + populationSize + ": " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

            //Population complete?
            Assert.IsTrue(generator.Population.Length == populationSize);


            start = DateTime.Now.Ticks;
            generator.PerformEvolution();
            end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Evolution time: " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

        }
    }
}
