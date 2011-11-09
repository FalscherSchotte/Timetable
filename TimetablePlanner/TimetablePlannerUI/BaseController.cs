using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TimetablePlannerUI
{
    public abstract class BaseController : DependencyObject
    {
        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(BaseController), new UIPropertyMetadata(null));

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(BaseController), new UIPropertyMetadata(null));

        public ICommand NewCommand
        {
            get { return (ICommand)GetValue(NewCommandProperty); }
            set { SetValue(NewCommandProperty, value); }
        }

        public static readonly DependencyProperty NewCommandProperty =
            DependencyProperty.Register("NewCommand", typeof(ICommand), typeof(BaseController), new UIPropertyMetadata(null));

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
