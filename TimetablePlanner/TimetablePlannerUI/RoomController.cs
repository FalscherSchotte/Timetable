using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TimetablePlanner;
using Microsoft.Windows.Controls;

namespace TimetablePlannerUI
{
    public class RoomController : BaseController
    {
        public ObservableCollection<Room> RoomList { get; private set; }

        private bool _IsLab;
        public bool IsLab
        {
            get { return _IsLab; }
            set
            {
                _IsLab = value;
                RaisePropertyChanged(() => IsLab);
            }
        }

        private string _RoomName = "";
        public string RoomName
        {
            get { return _RoomName; }
            set
            {
                _RoomName = value;
                RaisePropertyChanged(() => RoomName);
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

        public RoomController()
            : base()
        {
            RoomList = new ObservableCollection<Room>();
        }

        public override void Save()
        {
            if (SelectedIndex > 0 && NameValid())
            {
                RoomList[SelectedIndex].IsLab = IsLab;
                RoomList[SelectedIndex].Name = RoomName;

                int tmpSelection = SelectedIndex;
                Room tmpRoom = RoomList[SelectedIndex];
                RoomList.RemoveAt(tmpSelection);
                RoomList.Insert(tmpSelection, tmpRoom);
                SelectedIndex = tmpSelection;
            }
        }

        public override void Delete()
        {
            if (SelectedIndex >= 0)
                RoomList.RemoveAt(SelectedIndex);
        }

        public override void New()
        {
            if (NameValid())
                RoomList.Add(new Room(RoomName, IsLab));
        }

        public void UpdateSelection(int newValue)
        {
            if (newValue < 0)
            {
                RoomName = "";
                IsLab = false;
            }
            else
            {
                RoomName = RoomList[newValue].Name;
                IsLab = RoomList[newValue].IsLab;
            }
        }

        private bool NameValid()
        {
            if (RoomName.Length <= 0)
            {
                MessageBox.Show("You need to specify a room name.", "No roomname entered",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}
