using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimetablePlanner
{
    interface IExportable
    {
        string Name { get; }
        string Id { get; }
    }
}
