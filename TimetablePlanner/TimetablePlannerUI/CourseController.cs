using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TimetablePlanner;
using Microsoft.Windows.Controls;

namespace TimetablePlannerUI
{
    public class CourseController : BaseController
    {
        private BlockController BlockContext;
        private RoomController RoomContext;
        private GroupController GroupContext;
        private LecturerController LecturerContext;

        public ObservableCollection<Course> CourseList { get; set; }

        private int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                _SelectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
                UpdateSelection(value);
            }
        }

        private string _CourseName;
        public string CourseName
        {
            get { return _CourseName; }
            set
            {
                _CourseName = value;
                RaisePropertyChanged(() => CourseName);
            }
        }

        private string _CourseId;
        public string CourseId
        {
            get { return _CourseId; }
            set
            {
                _CourseId = value;
                RaisePropertyChanged(() => CourseId);
            }
        }

        private int _RepetitionCount;
        public int RepetitionCount
        {
            get { return _RepetitionCount; }
            set
            {
                _RepetitionCount = value;
                RaisePropertyChanged(() => RepetitionCount);
            }
        }

        private int _BlockCount;
        public int BlockCount
        {
            get { return _BlockCount; }
            set
            {
                _BlockCount = value;
                RaisePropertyChanged(() => BlockCount);
            }
        }

        private bool _IsDummy;
        public bool IsDummy
        {
            get { return _IsDummy; }
            set
            {
                _IsDummy = value;
                RaisePropertyChanged(() => IsDummy);
            }
        }

        private bool _NeedsLab;
        public bool NeedsLab
        {
            get { return _NeedsLab; }
            set
            {
                _NeedsLab = value;
                RaisePropertyChanged(() => NeedsLab);
            }
        }

        private int _SelectedLecturer1Index = -1;
        public int SelectedLecturer1Index
        {
            get { return _SelectedLecturer1Index; }
            set
            {
                _SelectedLecturer1Index = value;
                RaisePropertyChanged(() => SelectedLecturer1Index);
            }
        }

        private int _SelectedLecturer2Index = -1;
        public int SelectedLecturer2Index
        {
            get { return _SelectedLecturer2Index; }
            set
            {
                _SelectedLecturer2Index = value;
                RaisePropertyChanged(() => SelectedLecturer2Index);
            }
        }

        private int _SelectedRoomPreferenceIndex = -1;
        public int SelectedRoomPreferenceIndex
        {
            get { return _SelectedRoomPreferenceIndex; }
            set
            {
                _SelectedRoomPreferenceIndex = value;
                RaisePropertyChanged(() => SelectedRoomPreferenceIndex);
            }
        }

        private int _SelectedGroupIndex = -1;
        public int SelectedGroupIndex
        {
            get { return _SelectedGroupIndex; }
            set
            {
                _SelectedGroupIndex = value;
                RaisePropertyChanged(() => SelectedGroupIndex);
            }
        }

        private bool _HasPreference;
        public bool HasPreference
        {
            get { return _HasPreference; }
            set
            {
                _HasPreference = value;
                RaisePropertyChanged(() => HasPreference);
            }
        }

        private bool _HasSecondLecturer;
        public bool HasSecondLecturer
        {
            get { return _HasSecondLecturer; }
            set
            {
                _HasSecondLecturer = value;
                RaisePropertyChanged(() => HasSecondLecturer);
            }
        }

        public ObservableCollection<Group> GroupList { get; set; }
        public ObservableCollection<Room> RoomPreferenceList { get; set; }
        public ObservableCollection<Lecturer> LecturerList { get; set; }

        public CourseController(BlockController BlockContext, RoomController RoomContext, GroupController GroupContext, LecturerController LecturerContext)
            : base()
        {
            this.BlockContext = BlockContext;
            this.RoomContext = RoomContext;
            this.GroupContext = GroupContext;
            this.LecturerContext = LecturerContext;

            GroupList = GroupContext.GroupList;
            RoomPreferenceList = RoomContext.RoomList;
            LecturerList = LecturerContext.LecturersList;

            Repetitions = new Dictionary<Course, int>();

            CourseList = new ObservableCollection<Course>();
        }

        public CourseController() { }

        public Dictionary<Course, int> Repetitions { get; private set; }

        public override void Save()
        {
            if (SelectedIndex < 0)
                return;
            if (!DataIsValid())
                return;

            if (HasPreference)
                CourseList[SelectedIndex].RoomPreference = RoomPreferenceList[SelectedRoomPreferenceIndex];

            if (!HasSecondLecturer)
                CourseList[SelectedIndex].Lecturers = new Lecturer[] { LecturerList[SelectedLecturer1Index] };
            else
                CourseList[SelectedIndex].Lecturers = new Lecturer[] { LecturerList[SelectedLecturer1Index], LecturerList[SelectedLecturer2Index] };

            CourseList[SelectedIndex].Group = GroupList[SelectedGroupIndex];
            CourseList[SelectedIndex].NumberOfBlocks = BlockCount;

            if (!Repetitions.ContainsKey(CourseList[SelectedIndex]))
                Repetitions.Add(CourseList[SelectedIndex], RepetitionCount);

            CourseList[SelectedIndex].Name = CourseName;
            CourseList[SelectedIndex].Id = CourseId;

            CourseList[SelectedIndex].NeedsLab = NeedsLab;
            CourseList[SelectedIndex].IsDummy = IsDummy;

            Course tmpCourse = CourseList[SelectedIndex];
            int tmpIndex = SelectedIndex;
            CourseList.RemoveAt(tmpIndex);
            CourseList.Insert(tmpIndex, tmpCourse);
            SelectedIndex = tmpIndex;
        }

        public override void Delete()
        {
            if (SelectedIndex < 0)
                return;
            CourseList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            if (!DataIsValid())
                return;

            Course newCourse = new Course();

            if (HasPreference)
                newCourse.RoomPreference = RoomPreferenceList[SelectedRoomPreferenceIndex];

            if (!HasSecondLecturer)
                newCourse.Lecturers = new Lecturer[] { LecturerList[SelectedLecturer1Index] };
            else if (SelectedLecturer2Index > 0)
                newCourse.Lecturers = new Lecturer[] { LecturerList[SelectedLecturer1Index], LecturerList[SelectedLecturer2Index] };

            newCourse.Group = GroupList[SelectedGroupIndex];
            newCourse.NumberOfBlocks = BlockCount;

            if (!Repetitions.ContainsKey(newCourse))
                Repetitions.Add(newCourse, RepetitionCount);

            newCourse.Name = CourseName;
            newCourse.Id = CourseId;

            newCourse.NeedsLab = NeedsLab;
            newCourse.IsDummy = IsDummy;

            CourseList.Add(newCourse);
        }

        public void UpdateSelection(int newValue)
        {
            if (newValue < 0)
            {
                SelectedRoomPreferenceIndex = -1;
                SelectedLecturer1Index = -1;
                SelectedLecturer2Index = -1;
                SelectedGroupIndex = -1;
                RepetitionCount = 1;
                BlockCount = 1;
                CourseName = "";
                CourseId = "";
                NeedsLab = false;
                IsDummy = false;
                HasSecondLecturer = false;
                HasPreference = false;
            }
            else
            {
                SelectedRoomPreferenceIndex = RoomPreferenceList.IndexOf(CourseList[SelectedIndex].RoomPreference);
                SelectedLecturer1Index = LecturerList.IndexOf(CourseList[SelectedIndex].Lecturers[0]);
                SelectedLecturer2Index = CourseList[SelectedIndex].Lecturers.Length > 1 ? LecturerList.IndexOf(CourseList[SelectedIndex].Lecturers[1]) : -1;
                SelectedGroupIndex = GroupList.IndexOf(CourseList[SelectedIndex].Group); ;
                RepetitionCount = Repetitions[CourseList[SelectedIndex]];
                BlockCount = CourseList[SelectedIndex].NumberOfBlocks;
                CourseName = CourseList[SelectedIndex].Name;
                CourseId = CourseList[SelectedIndex].Id;
                NeedsLab = CourseList[SelectedIndex].NeedsLab;
                IsDummy = CourseList[SelectedIndex].IsDummy;

                HasSecondLecturer = SelectedLecturer2Index >= 0;
                HasPreference = SelectedRoomPreferenceIndex >= 0;
            }
        }

        private bool DataIsValid()
        {
            if (SelectedLecturer1Index == -1 || SelectedGroupIndex == -1)
            {
                MessageBox.Show("Lecturer 1 and Group must be assigned.", "Invalid data", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        internal Course[] GetCourseExportArray()
        {
            List<Course> cList = new List<Course>();
            foreach (var course in CourseList)
            {
                for (int i = 0; i < Repetitions[course]; i++)
                {
                    Course c = new Course(course.Id, course.Name, course.Lecturers, course.RoomPreference,
                        course.Group, course.NeedsLab, course.IsDummy, course.NumberOfBlocks);
                    cList.Add(c);
                }
            }
            return cList.ToArray();
        }
    }
}
