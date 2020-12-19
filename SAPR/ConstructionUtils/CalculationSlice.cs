using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SAPR.ConstructionUtils
{
    public class CalculationSlice : INotifyPropertyChanged
    {
        public double X
        {
            get { return x; }
            set 
            { 
                x = value;
                OnPropertyChanged("X");
            }
        }

        public double Ux
        {
            get { return ux; }
            set 
            { 
                ux = value;
                OnPropertyChanged("Ux");
            }
        }

        public double Nx
        {
            get { return nx; }
            set
            {
                nx = value;
                OnPropertyChanged("Nx");
            }
        }

        public double SigmaX
        {
            get { return sigmax; }
            set
            {
                sigmax = value;
                OnPropertyChanged("SigmaX");
            }
        }

        private double x;
        private double ux;
        private double nx;
        private double sigmax;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
