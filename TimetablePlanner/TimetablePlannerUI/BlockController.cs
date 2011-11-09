using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using TimetablePlanner;
using System.Windows.Input;

namespace TimetablePlannerUI
{
    public class BlockController : BaseController
    {
        public ObservableCollection<Block> BlockList
        {
            get { return (ObservableCollection<Block>)GetValue(BlockListProperty); }
            set { SetValue(BlockListProperty, value); }
        }

        public static readonly DependencyProperty BlockListProperty =
            DependencyProperty.Register("BlockList", typeof(ObservableCollection<Block>), typeof(BlockController), new UIPropertyMetadata(new ObservableCollection<Block>()));

        public string StartTimeHours
        {
            get { return (string)GetValue(StartTimeHoursProperty); }
            set { SetValue(StartTimeHoursProperty, value); }
        }

        public static readonly DependencyProperty StartTimeHoursProperty =
            DependencyProperty.Register("StartTimeHours", typeof(string), typeof(BlockController), new UIPropertyMetadata("08"));

        public string StartTimeMinutes
        {
            get { return (string)GetValue(StartTimeMinutesProperty); }
            set { SetValue(StartTimeMinutesProperty, value); }
        }

        public static readonly DependencyProperty StartTimeMinutesProperty =
            DependencyProperty.Register("StartTimeMinutes", typeof(string), typeof(BlockController), new UIPropertyMetadata("00"));

        public string EndTimeHours
        {
            get { return (string)GetValue(EndTimeHoursProperty); }
            set { SetValue(EndTimeHoursProperty, value); }
        }

        public static readonly DependencyProperty EndTimeHoursProperty =
            DependencyProperty.Register("EndTimeHours", typeof(string), typeof(BlockController), new UIPropertyMetadata("09"));

        public string EndTimeMinutes
        {
            get { return (string)GetValue(EndTimeMinutesProperty); }
            set { SetValue(EndTimeMinutesProperty, value); }
        }

        public static readonly DependencyProperty EndTimeMinutesProperty =
            DependencyProperty.Register("EndTimeMinutes", typeof(string), typeof(BlockController), new UIPropertyMetadata("30"));

        public bool ExceptMonday
        {
            get { return (bool)GetValue(ExceptMondayProperty); }
            set { SetValue(ExceptMondayProperty, value); }
        }

        public static readonly DependencyProperty ExceptMondayProperty =
            DependencyProperty.Register("ExceptMonday", typeof(bool), typeof(BlockController), new UIPropertyMetadata(false));

        public bool ExceptTuesday
        {
            get { return (bool)GetValue(ExceptTuesdayProperty); }
            set { SetValue(ExceptTuesdayProperty, value); }
        }

        public static readonly DependencyProperty ExceptTuesdayProperty =
            DependencyProperty.Register("ExceptTuesday", typeof(bool), typeof(BlockController), new UIPropertyMetadata(false));

        public bool ExceptWednesday
        {
            get { return (bool)GetValue(ExceptWednesdayProperty); }
            set { SetValue(ExceptWednesdayProperty, value); }
        }

        public static readonly DependencyProperty ExceptWednesdayProperty =
            DependencyProperty.Register("ExceptWednesday", typeof(bool), typeof(BlockController), new UIPropertyMetadata(false));

        public bool ExceptThursday
        {
            get { return (bool)GetValue(ExceptThursdayProperty); }
            set { SetValue(ExceptThursdayProperty, value); }
        }

        public static readonly DependencyProperty ExceptThursdayProperty =
            DependencyProperty.Register("ExceptThursday", typeof(bool), typeof(BlockController), new UIPropertyMetadata(false));

        public bool ExceptFriday
        {
            get { return (bool)GetValue(ExceptFridayProperty); }
            set { SetValue(ExceptFridayProperty, value); }
        }

        public static readonly DependencyProperty ExceptFridayProperty =
            DependencyProperty.Register("ExceptFriday", typeof(bool), typeof(BlockController), new UIPropertyMetadata(false));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(BlockController), new UIPropertyMetadata(-1, new PropertyChangedCallback(SelectedIndexChanged)));


        public BlockController()
            : base()
        {
            BlockList.Add(new Block(DateTime.Now, DateTime.Now, null));
        }

        public override void Save()
        {
            BlockList[SelectedIndex].Start = DateTime.Parse(StartTimeHours + ":" + StartTimeMinutes);
            BlockList[SelectedIndex].End = DateTime.Parse(EndTimeHours + ":" + EndTimeMinutes);
            
        }

        public override void Delete()
        {
            if (SelectedIndex < 0)
                return;
            BlockList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            BlockList.Add(new Block());
        }

        public static void SelectedIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            (o as BlockController).UpdateSelection(args.NewValue);
        }

        public void UpdateSelection(object newValue)
        {
            if ((int)newValue < 0)
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
                StartTimeHours = BlockList[(int)newValue].Start.TimeOfDay.Hours.ToString();
                StartTimeMinutes = BlockList[(int)newValue].Start.TimeOfDay.Minutes.ToString();
                EndTimeHours = BlockList[(int)newValue].End.TimeOfDay.Hours.ToString();
                EndTimeMinutes = BlockList[(int)newValue].End.TimeOfDay.Minutes.ToString();

                if (BlockList[(int)newValue].Exceptions != null)
                {
                    ExceptMonday = BlockList[(int)newValue].Exceptions.Contains(DayOfWeek.Monday) ? true : false;
                    ExceptTuesday = BlockList[(int)newValue].Exceptions.Contains(DayOfWeek.Tuesday) ? true : false;
                    ExceptWednesday = BlockList[(int)newValue].Exceptions.Contains(DayOfWeek.Wednesday) ? true : false;
                    ExceptThursday = BlockList[(int)newValue].Exceptions.Contains(DayOfWeek.Thursday) ? true : false;
                    ExceptFriday = BlockList[(int)newValue].Exceptions.Contains(DayOfWeek.Friday) ? true : false;
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
    }
}
