using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SAPR.ConstructionUtils
{
    class Rod : INotifyPropertyChanged, IDataErrorInfo
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

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Length":
                        if (Length < 0)
                        {
                            error = "Длина не должна быть меньше 0";
                        }
                        break;
                    case "Elasticity":
                        if (Elasticity < 0)
                        {
                            error = "Модуль упругости не должен быть меньше 0";
                        }
                        break;
                    case "AllowedStress":
                        if (AllowedStress < 0)
                        {
                            error = "Допускаемое напряжение не должно быть меньше 0";
                        }
                        break;
                }
                return error;
            }
        }

        public string Error => throw new System.NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
