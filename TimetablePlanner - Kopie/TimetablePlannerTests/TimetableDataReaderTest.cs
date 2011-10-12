using TimetablePlanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TimetablePlannerTests
{
    [TestClass()]
    public class TimetableTest
    {
        [TestMethod()]
        public void createTimetableInstanceTest()
        {
            string configurationFile = "E:\\HsKA\\Semester2\\Projektarbeit\\Studenplaner\\TimetablePlanner\\TimetablePlanner\\TimetableData.xml";
            TimetableData data = TimetableDataReader.createTimetableInstance(configurationFile);
            TimetableGenerator generator = new TimetableGenerator(1000, 50, data);
        }
    }
}
