using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    public class Group
    {
        public string Id { get; private set; }
        
        public Group(string id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
