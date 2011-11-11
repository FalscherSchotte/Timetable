using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TimetablePlanner;
using System.ComponentModel;

namespace TimetablePlannerUI
{
    public class ConfigController : NotifyBase
    {
        private BlockController _BlockContext = new BlockController();
        public BlockController BlockContext
        {
            get { return _BlockContext; }
            set
            {
                _BlockContext = value;
                RaisePropertyChanged(() => BlockContext);
            }
        }

        private RoomController _RoomContext = new RoomController();
        public RoomController RoomContext
        {
            get { return _RoomContext; }
            set
            {
                _RoomContext = value;
                RaisePropertyChanged(() => RoomContext);
            }
        }

        private GroupController _GroupContext = new GroupController();
        public GroupController GroupContext
        {
            get { return _GroupContext; }
            set
            {
                _GroupContext = value;
                RaisePropertyChanged(() => GroupContext);
            }
        }

        private LecturerController _LecturerContext = new LecturerController();
        public LecturerController LecturerContext
        {
            get { return _LecturerContext; }
            set
            {
                _LecturerContext = value;
                RaisePropertyChanged(() => LecturerContext);
            }
        }

        private CourseController _CourseContext = new CourseController();
        public CourseController CourseContext
        {
            get { return _CourseContext; }
            set
            {
                _CourseContext = value;
                RaisePropertyChanged(() => CourseContext);
            }
        }


        internal void LoadCourseData()
        {
            CourseContext = new CourseController(BlockContext, RoomContext, GroupContext, LecturerContext);
        }

    }
}
