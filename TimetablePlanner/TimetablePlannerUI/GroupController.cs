using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimetablePlanner;
using System.Collections.ObjectModel;
using Microsoft.Windows.Controls;

namespace TimetablePlannerUI
{
    public class GroupController : BaseController
    {
        public ObservableCollection<Group> GroupList { get; set; }

        private string _GroupName = "";
        public string GroupName
        {
            get { return _GroupName; }
            set
            {
                _GroupName = value;
                RaisePropertyChanged(() => GroupName);
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

        public GroupController()
            : base()
        {
            GroupList = new ObservableCollection<Group>();
        }


        public override void Save()
        {
            if (SelectedIndex < 0)
                return;
            if (GroupName == null || GroupName.Length <= 0)
            {
                MessageBox.Show("Groupname required.", "Input missing", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            GroupList[SelectedIndex].Name = GroupName;
            GroupList[SelectedIndex].Id = GroupName;

            Group tmpGroup = GroupList[SelectedIndex];
            int tmpIndex = SelectedIndex;
            GroupList.RemoveAt(tmpIndex);
            GroupList.Insert(tmpIndex, tmpGroup);
            SelectedIndex = tmpIndex;
        }

        public override void Delete()
        {
            GroupList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            if (GroupName == null || GroupName.Length <= 0)
            {
                MessageBox.Show("Groupname required.", "Input missing", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            GroupList.Add(new Group(GroupName, GroupName));
        }

        public void UpdateSelection(int newValue)
        {
            if (newValue < 0)
                GroupName = "";
            else
                GroupName = GroupList[newValue].Name;
        }
    }
}
