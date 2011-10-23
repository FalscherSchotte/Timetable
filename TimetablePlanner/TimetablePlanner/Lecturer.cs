using System;
using System.Collections.Generic;

namespace TimetablePlanner
{
    public class Lecturer : IExportable
    {
        public bool IsDummy { get; private set; }
        public int NeededNumberOfResearchDays { get; private set; }
        public string Id { get; private set; }
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public string Name { get; private set; }
        public List<int> AvailableResearchDays { get; private set; }

        private static short globalIndex = 0;
        public short Index { get; private set; }

        public Lecturer(string id, string lastName, string firstName, List<DayOfWeek> researchExceptions, int numberOfResearchDays, bool isDummy)
        {
            this.Id = id;
            this.LastName = lastName;
            this.FirstName = firstName;
            this.Name = firstName + lastName;
            this.NeededNumberOfResearchDays = numberOfResearchDays;
            this.IsDummy = isDummy;

            this.AvailableResearchDays = new List<int>();
            if (!researchExceptions.Contains(DayOfWeek.Monday))
                this.AvailableResearchDays.Add(0);
            if (!researchExceptions.Contains(DayOfWeek.Tuesday))
                this.AvailableResearchDays.Add(1);
            if (!researchExceptions.Contains(DayOfWeek.Wednesday))
                this.AvailableResearchDays.Add(2);
            if (!researchExceptions.Contains(DayOfWeek.Thursday))
                this.AvailableResearchDays.Add(3);
            if (!researchExceptions.Contains(DayOfWeek.Friday))
                this.AvailableResearchDays.Add(4);

            Index = globalIndex;
            globalIndex++;
        }

        public override string ToString()
        {
            return Id + ": " + LastName;
        }
    }
}
