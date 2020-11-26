using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SAPR.ConstructionUtils
{
    public class Rod : INotifyPropertyChanged
    {
        private int index;
        private float length;
        private float area;
        private float elasticity;
        private float allowedStress;

        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged("Index");
            }
        }

        public float Length
        {
            get { return length; }
            set
            {
                length = value;
                OnPropertyChanged("Length");
            }
        }

        public float Area
        {
            get { return area; }
            set
            {
                area = value;
                OnPropertyChanged("Area");
            }
        }

        public float Elasticity
        {
            get { return elasticity; }
            set
            {
                elasticity = value;
                OnPropertyChanged("Elasticity");
            }
        }

        public float AllowedStress
        {
            get { return allowedStress; }
            set
            {
                allowedStress = value;
                OnPropertyChanged("AllowedStress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
