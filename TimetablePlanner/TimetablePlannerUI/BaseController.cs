using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TimetablePlanner;

namespace TimetablePlannerUI
{
    public abstract class BaseController : NotifyBase
    {
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand NewCommand { get; private set; }

        public BaseController()
        {
            SaveCommand = new ActionCommand(new Action(Save));
            DeleteCommand = new ActionCommand(new Action(Delete));
            NewCommand = new ActionCommand(new Action(New));
        }

        public abstract void Save();
        public abstract void Delete();
        public abstract void New();
    }
}
