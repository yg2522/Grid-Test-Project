using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace WpfApplication1
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged([CallerMemberName] string caller = null)
        {
            if (caller != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
