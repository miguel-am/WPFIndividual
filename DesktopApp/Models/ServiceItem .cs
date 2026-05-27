using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class ServiceItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Key { get; }
        public string Name { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceItem(string key, string name, bool isSelected = false)
        {
            Key = key;
            Name = name;
            IsSelected = isSelected;
        }
    }
}
