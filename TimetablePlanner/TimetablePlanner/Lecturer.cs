using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Lecturer
    {
        public string Id { get; private set; }
        public string LastName { get; private set; }

        public Lecturer(string id, string lastName)
        {
            this.Id = id;
            this.LastName = lastName;
        }

        public override string ToString()
        {
            return Id + ": " + LastName;
        }
    }
}
