// Place in: WeldAdminPro.Core\Utilities\ObservableObjectShim.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeldAdminPro.Core.Utilities
{
    /// <summary>
    /// Tiny ObservableObject shim implementing INotifyPropertyChanged.
    /// Not a full replacement for CommunityToolkit, but enough for simple VMs.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
