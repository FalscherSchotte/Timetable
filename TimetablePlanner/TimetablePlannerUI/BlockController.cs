using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using TimetablePlanner;
using System.Text;
using Microsoft.Windows.Controls;

namespace TimetablePlannerUI
{
    public class BlockController : BaseController
    {
        public ObservableCollection<Block> BlockList { get; private set; }

        private string _StartTimeHours = "8";
        public string StartTimeHours
        {
            get { return _StartTimeHours; }
            set
            {
                _StartTimeHours = value;
                RaisePropertyChanged(() => StartTimeHours);
            }
        }

        private string _EndTimeHours = "9";
        public string EndTimeHours
        {
            get { return _EndTimeHours; }
            set
            {
                _EndTimeHours = value;
                RaisePropertyChanged(() => EndTimeHours);
            }
        }

        private string _StartTimeMinutes = "0";
        public string StartTimeMinutes
        {
            get { return _StartTimeMinutes; }
            set
            {
                _StartTimeMinutes = value;
                RaisePropertyChanged(() => StartTimeMinutes);
            }
        }

        private string _EndTimeMinutes = "30";
        public string EndTimeMinutes
        {
            get { return _EndTimeMinutes; }
            set
            {
                _EndTimeMinutes = value;
                RaisePropertyChanged(() => EndTimeMinutes);
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

        private int _SelectedIndex;
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

        public BlockController()
            : base()
        {
            BlockList = new ObservableCollection<Block>();
        }

        public override void Save()
        {
            if (SelectedIndex < 0)
                return;
            if (!DataIsValid())
                return;

            BlockList[SelectedIndex].Start = DateTime.Parse(StartTimeHours + ":" + StartTimeMinutes);
            BlockList[SelectedIndex].End = DateTime.Parse(EndTimeHours + ":" + EndTimeMinutes);

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
            BlockList[SelectedIndex].Exceptions = exceptions.ToArray();

            Block tmpBlock = BlockList[SelectedIndex];
            int tmpIndex = SelectedIndex;
            BlockList.RemoveAt(tmpIndex);
            BlockList.Insert(tmpIndex, tmpBlock);
            SelectedIndex = tmpIndex;
        }

        public override void Delete()
        {
            if (SelectedIndex < 0)
                return;
            BlockList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            if (!DataIsValid())
                return;

            DateTime start = DateTime.Parse(StartTimeHours + ":" + StartTimeMinutes);
            DateTime end = DateTime.Parse(EndTimeHours + ":" + EndTimeMinutes);
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
            BlockList.Add(new Block(start, end, exceptions.ToArray()));

            RaisePropertyChanged(() => this.BlockList);
        }

        public void UpdateSelection(int newValue)
        {
            if (newValue < 0)
            {
                StartTimeHours = "08";
                StartTimeMinutes = "00";
                EndTimeHours = "09";
                EndTimeMinutes = "30";

                ExceptMonday = false;
                ExceptTuesday = false;
                ExceptWednesday = false;
                ExceptThursday = false;
                ExceptFriday = false;
            }
            else
            {
                if (!DataIsValid())
                    return;

                StartTimeHours = BlockList[newValue].Start.TimeOfDay.Hours.ToString();
                StartTimeMinutes = BlockList[newValue].Start.TimeOfDay.Minutes.ToString();
                EndTimeHours = BlockList[newValue].End.TimeOfDay.Hours.ToString();
                EndTimeMinutes = BlockList[newValue].End.TimeOfDay.Minutes.ToString();

                if (BlockList[newValue].Exceptions != null)
                {
                    ExceptMonday = BlockList[newValue].Exceptions.Contains(DayOfWeek.Monday) ? true : false;
                    ExceptTuesday = BlockList[newValue].Exceptions.Contains(DayOfWeek.Tuesday) ? true : false;
                    ExceptWednesday = BlockList[newValue].Exceptions.Contains(DayOfWeek.Wednesday) ? true : false;
                    ExceptThursday = BlockList[newValue].Exceptions.Contains(DayOfWeek.Thursday) ? true : false;
                    ExceptFriday = BlockList[newValue].Exceptions.Contains(DayOfWeek.Friday) ? true : false;
                }
                else
                {
                    ExceptMonday = false;
                    ExceptTuesday = false;
                    ExceptWednesday = false;
                    ExceptThursday = false;
                    ExceptFriday = false;
                }
            }
        }

        public bool DataIsValid()
        {
            DateTime start = DateTime.Parse(StartTimeHours + ":" + StartTimeMinutes);
            DateTime end = DateTime.Parse(EndTimeHours + ":" + EndTimeMinutes);
            if ((end - start).TotalMinutes < 1)
            {
                MessageBox.Show("Starttime must be before endtime.", "Invalid data", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}
