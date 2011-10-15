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

            int populationSize = 20;
            int numberOfGenerations = 5000;

            long start = DateTime.Now.Ticks;

            TimetableGenerator generator = null;
            try
            {
                generator = new TimetableGenerator(numberOfGenerations, populationSize, data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.IsTrue(false);
                return;
            }
            
            
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
            String[] pop0Elements = generator.Population[0].ToString().Substring(6).Split('|');
            foreach (String element in pop0Elements)
            {
                short courseNumber;
                if (element.Length == 3 && short.TryParse(element.Substring(0, 3), out courseNumber))
                {
                    if (!courses.Contains(courseNumber))
                        courses.Add(courseNumber);
                }
            }
            courses.Sort();
            Assert.IsTrue(courses.Count == data.Courses.Length);


            start = DateTime.Now.Ticks;
            generator.PerformEvolution();
            end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine(numberOfGenerations + " Generations finished after " + (end - start) / 10000 + "ms");
            foreach (Individual i in generator.Population)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString());
            }

            TimetablePrinter.PrintTable(generator.Population[0], data);
        }
    }
}
