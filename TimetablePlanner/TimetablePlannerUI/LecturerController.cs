using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TimetablePlanner;
using Microsoft.Windows.Controls;

namespace TimetablePlannerUI
{
    public class LecturerController : BaseController
    {
        public ObservableCollection<Lecturer> LecturersList { get; set; }

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

        private bool _ExceptMonday;
        public bool ExceptMonday
        {
            get { return _ExceptMonday; }
            set
            {
                _ExceptMonday = value;
                RaisePropertyChanged(() => ExceptMonday);
            }
        }

        private bool _ExceptTuesday;
        public bool ExceptTuesday
        {
            get { return _ExceptTuesday; }
            set
            {
                _ExceptTuesday = value;
                RaisePropertyChanged(() => ExceptTuesday);
            }
        }

        private bool _ExceptWednesday;
        public bool ExceptWednesday
        {
            get { return _ExceptWednesday; }
            set
            {
                _ExceptWednesday = value;
                RaisePropertyChanged(() => ExceptWednesday);
            }
        }

        private bool _ExceptThursday;
        public bool ExceptThursday
        {
            get { return _ExceptThursday; }
            set
            {
                _ExceptThursday = value;
                RaisePropertyChanged(() => ExceptThursday);
            }
        }

        private bool _ExceptFriday;
        public bool ExceptFriday
        {
            get { return _ExceptFriday; }
            set
            {
                _ExceptFriday = value;
                RaisePropertyChanged(() => ExceptFriday);
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

        private string _Lastname = "";
        public string Lastname
        {
            get { return _Lastname; }
            set
            {
                _Lastname = value;
                RaisePropertyChanged(() => Lastname);
            }
        }

        private string _Firstname = "";
        public string Firstname
        {
            get { return _Firstname; }
            set
            {
                _Firstname = value;
                RaisePropertyChanged(() => Firstname);
            }
        }

        private string _LecturerId = "";
        public string LecturerId
        {
            get { return _LecturerId; }
            set
            {
                _LecturerId = value;
                RaisePropertyChanged(() => LecturerId);
            }
        }

        private string _ResearchDays = "0";
        public string ResearchDays
        {
            get { return _ResearchDays; }
            set
            {
                _ResearchDays = value;
                RaisePropertyChanged(() => ResearchDays);
            }
        }

        public LecturerController()
        {
            LecturersList = new ObservableCollection<Lecturer>();
        }

        public override void Save()
        {
            if (SelectedIndex < 0)
                return;

            if (!DataIsValid())
                return;

            LecturersList[SelectedIndex].FirstName = Firstname;
            LecturersList[SelectedIndex].LastName = Lastname;
            LecturersList[SelectedIndex].Id = LecturerId;
            LecturersList[SelectedIndex].NeededNumberOfResearchDays = int.Parse(ResearchDays);
            LecturersList[SelectedIndex].IsDummy = IsDummy;

            LecturersList[SelectedIndex].AvailableResearchDays.Clear();
            if (!ExceptMonday)
                LecturersList[SelectedIndex].AvailableResearchDays.Add((int)DayOfWeek.Monday - 1);
            if (!ExceptTuesday)
                LecturersList[SelectedIndex].AvailableResearchDays.Add((int)DayOfWeek.Tuesday - 1);
            if (!ExceptWednesday)
                LecturersList[SelectedIndex].AvailableResearchDays.Add((int)DayOfWeek.Wednesday - 1);
            if (!ExceptThursday)
                LecturersList[SelectedIndex].AvailableResearchDays.Add((int)DayOfWeek.Thursday - 1);
            if (!ExceptFriday)
                LecturersList[SelectedIndex].AvailableResearchDays.Add((int)DayOfWeek.Friday - 1);

            int tmpIndex = SelectedIndex;
            Lecturer tmpLecturer = LecturersList[SelectedIndex];
            LecturersList.RemoveAt(tmpIndex);
            LecturersList.Insert(tmpIndex, tmpLecturer);
            SelectedIndex = tmpIndex;
        }

        public override void Delete()
        {
            if (SelectedIndex > 0)
                LecturersList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            if (!DataIsValid())
                return;

            List<DayOfWeek> exceptions = new List<DayOfWeek>();
            if (ExceptMonday)
                exceptions.Add(DayOfWeek.Monday);
            if (ExceptTuesday)
                exceptions.Add(DayOfWeek.Tuesday);
            if (ExceptWednesday)
                exceptions.Add(DayOfWeek.Wednesday);
            if (ExceptThursday)
                exceptions.Add(DayOfWeek.Thursday);
            if (ExceptFriday)
                exceptions.Add(DayOfWeek.Friday);

            int researchDays = int.Parse(ResearchDays);

            LecturersList.Add(new Lecturer(LecturerId, Lastname, Firstname, exceptions, researchDays, IsDummy));
        }

        private bool DataIsValid()
        {
            List<String> invalid = new List<string>();
            if (Lastname == null || Lastname.Length <= 0)
                invalid.Add("Lastname must be specified.");
            if (LecturerId == null || LecturerId.Length <= 0)
                invalid.Add("Id must be specified.");
            if (invalid.Count == 0)
                return true;
            if (invalid.Count == 1)
                MessageBox.Show(invalid[0], "Input is missing.", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            if (invalid.Count == 2)
                MessageBox.Show(invalid[0] + "\n" + invalid[1], "Input is missing.", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }

        public void UpdateSelection(int newValue)
        {
            if (newValue < 0)
            {
                LecturerId = "";
                Lastname = "";
                Firstname = "";
                ResearchDays = "0";
                ExceptMonday = false;
                ExceptTuesday = false;
                ExceptWednesday = false;
                ExceptThursday = false;
                ExceptFriday = false;
                IsDummy = false;
            }
            else
            {
                LecturerId = LecturersList[newValue].Id;
                Lastname = LecturersList[newValue].LastName;
                Firstname = LecturersList[newValue].FirstName;
                ResearchDays = LecturersList[newValue].NeededNumberOfResearchDays.ToString();
                ExceptMonday = LecturersList[newValue].AvailableResearchDays.Contains(0) ? false : true;
                ExceptTuesday = LecturersList[newValue].AvailableResearchDays.Contains(1) ? false : true;
                ExceptWednesday = LecturersList[newValue].AvailableResearchDays.Contains(2) ? false : true;
                ExceptThursday = LecturersList[newValue].AvailableResearchDays.Contains(3) ? false : true;
                ExceptFriday = LecturersList[newValue].AvailableResearchDays.Contains(4) ? false : true;
                IsDummy = LecturersList[newValue].IsDummy;
            }
        }
    }
}
