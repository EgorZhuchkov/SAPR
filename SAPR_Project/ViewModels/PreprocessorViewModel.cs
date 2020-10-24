using SAPR.ConstructionUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SAPR.ViewModels
{
    class PreprocessorViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
        public bool HasErrors => _errorsByPropertyName.Any();

        private bool hasRightSupport;
        private bool hasLeftSupport;
        public bool HasRightSupport
        {
            get { return hasRightSupport; }
            set
            {
                hasRightSupport = value;
                UpdateConstruction();
                OnPropertyChanged("HasRightSupport");
            }
        }
        public bool HasLeftSupport
        {
            get { return hasLeftSupport; }
            set
            {
                UpdateConstruction();
                hasLeftSupport = value;
                OnPropertyChanged("HasLeftSupport");
            }
        }
        public ObservableCollection<Rod> Rods { get; set; }
        public ObservableCollection<Strain> Strains { get; set; }
        private Construction construction;

        #region Commands
        private RelayCommand _addRodCommand;
        public RelayCommand AddRodCommand
        {
            get
            {
                return _addRodCommand ??
                  (_addRodCommand = new RelayCommand(obj =>
                  {
                      var rod = new Rod { Index = Rods.Count + 1 };
                      Rods.Add(rod);
                      rod.PropertyChanged += Rod_PropertyChangedCallback;
                      UpdateConstruction();
                  }));
            }
        }

        private RelayCommand _removeRodCommand;
        public RelayCommand RemoveRodCommand
        {
            get
            {
                return _removeRodCommand ??
                  (_removeRodCommand = new RelayCommand(obj =>
                  {
                      var rodToRemove = obj as Rod;
                      if (rodToRemove != null)
                      {
                          int rodIndex = Rods.IndexOf(rodToRemove);
                          Rods.Remove(rodToRemove);
                          for (int i = rodIndex; i < Rods.Count; i++)
                          {
                              Rods[i].Index = i + 1;
                          }
                          UpdateConstruction();
                      }
                  },
                  (obj) => Rods.Count > 0));
            }
        }

        private RelayCommand _addStrainCommand;
        public RelayCommand AddStrainCommand
        {
            get
            {
                return _addStrainCommand ??
                  (_addStrainCommand = new RelayCommand(obj =>
                  {
                      var strain = new Strain { Index = Strains.Count + 1 };
                      Strains.Add(strain);
                      strain.PropertyChanged += Rod_PropertyChangedCallback;
                      UpdateConstruction();
                  }));
            }
        }

        private RelayCommand _removeStrainCommand;
        public RelayCommand RemoveStrainCommand
        {
            get
            {
                return _removeStrainCommand ??
                  (_removeStrainCommand = new RelayCommand(obj =>
                  {
                      var strainToRemove = obj as Strain;
                      if (strainToRemove != null)
                      {
                          int strainIndex = Strains.IndexOf(strainToRemove);
                          Strains.Remove(strainToRemove);
                          for (int i = strainIndex; i < Strains.Count; i++)
                          {
                              Strains[i].Index = i + 1;
                          }
                          UpdateConstruction();
                      }
                  },
                  (obj) => Strains.Count > 0));
            }
        }

        private RelayCommand _testCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return _testCommand ??
                  (_testCommand = new RelayCommand(obj =>
                  {
                      MessageBox.Show($"Construction: {construction.Rods[1].Length}");
                  }));
            }
        }
        #endregion

        public PreprocessorViewModel()
        {
            construction = new Construction();

            Rods = new ObservableCollection<Rod>
            {
                new Rod { Length = 10.0f, AllowedStress = 5.0f, Area = 20.0f, Elasticity = 2.0f, Index = 1}
            };
            Strains = new ObservableCollection<Strain>
            {
                new Strain {NodeIndex = 1, Magnitude  = 10.0f, StrainType = StrainType.Lengthwise, Index = 1}
            };
            Rods[0].PropertyChanged += Rod_PropertyChangedCallback;
            Strains[0].PropertyChanged += Rod_PropertyChangedCallback;
            HasRightSupport = true;
            UpdateConstruction();
        }

        public void Rod_PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            UpdateConstruction();
        }

        private void UpdateConstruction()
        {
            construction.Update(Rods.ToList(), Strains.ToList(), HasRightSupport, HasLeftSupport);
            //MessageBox.Show("Construction updated");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public IEnumerable GetErrors(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
