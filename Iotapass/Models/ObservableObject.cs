using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Iotapass
{
    // if a UI element is binded to a variable in the code behind, and changes to the variable in the code should update the UI, that var must implement ObservableObject.
    internal class ObservableObject : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
