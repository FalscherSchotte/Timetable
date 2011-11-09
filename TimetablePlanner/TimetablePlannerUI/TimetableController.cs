using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimetablePlanner;

namespace TimetablePlannerUI
{
    public class TimetableController
    {
        public TimetableGenerator ttGenerator { get; private set; }
        public TimetableConfigIO ttIO { get; private set; }

        public TimetableController()
        {
            
        }
    }
}
