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
            Assert.IsTrue(generator.Population.Count == populationSize);

            //All courses set?
            List<short> courses = new List<short>();
            foreach (String element in generator.Population[0].ToString().Substring(generator.Population[0].ToString().IndexOf(":") + 1).Split(','))
            {
                short courseNumber;
                if (element.Length == 3 && short.TryParse(element.Substring(0, 3), out courseNumber))
                {
                    if (!courses.Contains(courseNumber) && courseNumber != 0)
                        courses.Add(courseNumber);
                }
            }
            Assert.IsTrue(courses.Count == data.Courses.Length);


            start = DateTime.Now.Ticks;
            generator.PerformEvolution();
            end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine(numberOfGenerations + " Generations finished after " + (end - start) / 10000 + "ms");

        }
    }
}
