using System;
using System.Linq;
using System.Windows;
using TimetablePlanner;
using System.Windows.Input;
using Microsoft.Win32;
using System.Threading;

namespace TimetablePlannerUI
{
    public class ConfigController : NotifyBase
    {
        #region Properties

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


        private int _ProgressbarMax = 100;
        public int ProgressbarMax
        {
            get { return _ProgressbarMax; }
            set
            {
                _ProgressbarMax = value;
                RaisePropertyChanged(() => ProgressbarMax);
            }
        }

        private int _ProgressbarValue = 0;
        public int ProgressbarValue
        {
            get { return _ProgressbarValue; }
            set
            {
                _ProgressbarValue = value;
                RaisePropertyChanged(() => ProgressbarValue);
            }
        }

        #endregion

        #region Variables

        private int _populationSize = 50;
        private int _numberOfGenerations = 1000;
        private string _currentConfigFilepath = "";

        #endregion

        #region Commands

        public ICommand SaveCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand GenerateCommand { get; set; }

        #endregion

        #region Constructor

        public ConfigController()
        {
            SaveCommand = new ActionCommand(new Action(Save));
            SaveAsCommand = new ActionCommand(new Action(SaveAs));
            LoadCommand = new ActionCommand(new Action(Load));
            NewCommand = new ActionCommand(new Action(New));
            GenerateCommand = new ActionCommand(new Action(Generate));
        }

        #endregion

        #region Init

        internal void LoadCourseData()
        {
            CourseContext = new CourseController(BlockContext, RoomContext, GroupContext, LecturerContext);
        }

        #endregion

        #region Save config

        public void Save()
        {
            if (_currentConfigFilepath == null || _currentConfigFilepath.Length == 0)
            {
                SaveAs();
                return;
            }
            TimetableConfigIO.ExportTimetableConfig(GetTimetableData(), _currentConfigFilepath);
        }

        public void SaveAs()
        {
            SaveFileDialog svDiag = new SaveFileDialog();
            svDiag.FileName = "TimetableConfig.xml";
            svDiag.Filter = "XML-Files (*.xml)|*.xml";
            svDiag.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (svDiag.ShowDialog() != true)
                return;
            _currentConfigFilepath = svDiag.FileName;
            TimetableConfigIO.ExportTimetableConfig(GetTimetableData(), _currentConfigFilepath);
        }

        #endregion

        #region Load config

        public void Load()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "xml-Files (*.xml)|*.xml";
            fileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            fileDialog.ShowDialog();

            TimetableData ttData = TimetableConfigIO.ImportTimetableConfig(fileDialog.FileName);
            if (ttData == null)
            {
                MessageBox.Show("Data could not be loaded.", "Loading error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BlockContext = new BlockController();
            foreach (var block in ttData.Blocks)
            {
                BlockContext.BlockList.Add(block);
            }
            if (ttData.Blocks.Length > 0)
                BlockContext.SelectedIndex = 0;

            RoomContext = new RoomController();
            foreach (var room in ttData.Rooms)
            {
                RoomContext.RoomList.Add(room);
            }
            if (ttData.Rooms.Length > 0)
                RoomContext.SelectedIndex = 0;

            GroupContext = new GroupController();
            foreach (var group in ttData.Groups)
            {
                GroupContext.GroupList.Add(group);
            }
            if (ttData.Groups.Length > 0)
                GroupContext.SelectedIndex = 0;

            LecturerContext = new LecturerController();
            foreach (var lecturer in ttData.Lecturers)
            {
                LecturerContext.LecturersList.Add(lecturer);
            }
            if (ttData.Lecturers.Length > 0)
                LecturerContext.SelectedIndex = 0;

            CourseContext = new CourseController(BlockContext, RoomContext, GroupContext, LecturerContext);
            AddCourses(CourseContext, ttData.Courses);
            if (ttData.Courses.Length > 0)
                CourseContext.SelectedIndex = 0;
        }

        private void AddCourses(CourseController CourseContext, Course[] courses)
        {
            foreach (Course course in courses)
            {
                bool isListed = false;
                foreach (Course storedCourse in CourseContext.Repetitions.Keys)
                {
                    if (storedCourse.Id.Equals(course.Id))
                    {
                        isListed = true;
                        CourseContext.Repetitions[storedCourse]++;
                        break;
                    }
                }
                if (!isListed)
                {
                    CourseContext.CourseList.Add(course);
                    CourseContext.Repetitions.Add(course, 1);
                }
            }
        }

        #endregion

        #region New config

        public void New()
        {
            TimetableData ttData = new TimetableData();
            BlockContext = new BlockController();
            RoomContext = new RoomController();
            LecturerContext = new LecturerController();
            GroupContext = new GroupController();
            CourseContext = new CourseController(BlockContext, RoomContext, GroupContext, LecturerContext);
        }

        #endregion

        #region Evolution

        public void Generate()
        {
            Thread t = new Thread(new ThreadStart(RunGeneration));
            t.Start();
        }

        private void RunGeneration()
        {
            SaveFileDialog svDiag = new SaveFileDialog();
            svDiag.FileName = "Export.csv";
            svDiag.Filter = "CSV-Files (*.csv)|*.csv";
            svDiag.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (svDiag.ShowDialog() != true)
                return;

            TimetableData ttData = GetTimetableData();

            TimetableGenerator generator = new TimetableGenerator(ttData);
            generator.GenerationTick += new Action<TimetableGenerator.HistoryEntry>(generator_GenerationTick);
            generator.IndividualCreated += new Action<int>(generator_IndividualCreated);

            generator.CreatePopulation(_populationSize, 10);
            generator.PerformEvolution(_numberOfGenerations);
            ProgressbarValue = 0;

            TimetableExportCSV.ExportAll(generator.Population[0], ttData, svDiag.FileName);
            System.Diagnostics.Process.Start(svDiag.FileName);
        }

        private void generator_IndividualCreated(int obj)
        {
            ProgressbarValue += (ProgressbarMax / _populationSize) / 2;
        }

        private void generator_GenerationTick(TimetableGenerator.HistoryEntry historyEntry)
        {
            ProgressbarValue = 50 + (int)((double)historyEntry.Index * ((double)ProgressbarMax / (double)_numberOfGenerations) / 2);
        }

        private TimetableData GetTimetableData()
        {
            TimetableData ttData = new TimetableData();
            ttData.Blocks = BlockContext.BlockList.ToArray();
            ttData.Groups = GroupContext.GroupList.ToArray();
            ttData.Lecturers = LecturerContext.LecturersList.ToArray();
            ttData.Rooms = RoomContext.RoomList.ToArray();
            ttData.Courses = CourseContext.GetCourseExportArray();
            return ttData;
        }

        #endregion
    }
}
