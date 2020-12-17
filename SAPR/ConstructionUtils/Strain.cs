using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SAPR.ConstructionUtils
{
    public enum StrainType 
    {
        [Description("Сосредоточенная")]
        Concentrated,
        [Description("Распределенная")]
        Lengthwise 
    }

    public class Strain : INotifyPropertyChanged
    {
        private int index;
        private int nodeIndex;
        private StrainType strainType;
        private float magnitude;

        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged("Index");
            }
        }

        public int NodeIndex
        {
            get { return nodeIndex; }
            set
            {
                nodeIndex = value;
                OnPropertyChanged("NodeIndex");
            }
        }

        public StrainType StrainType
        {
            get { return strainType; }
            set
            {
                strainType = value;
                OnPropertyChanged("StrainType");
            }
        }
        public float Magnitude
        {
            get { return magnitude; }
            set
            {
                magnitude = value;
                OnPropertyChanged("Magnitude");
            }
        }

        //public Strain(StrainType strainType, float magnitude, int index, int nodeIndex)
        //{
        //    StrainType = strainType;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
