using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CanBusDemoWpf
{
    public class Model : INotifyPropertyChanged
    {
        private string name;
        private string testValue;
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public Model()
        {
        }

        public Model(string value)
        {
            this.name = value;
        }

        public string PersonName
        {
            get { return name; }
            set
            {
                name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        
        public string TestValue
        {
            get { return testValue; }
            set
            {
                testValue = value;
                // Call OnPropertyChanged whenever the property is updated
                
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
