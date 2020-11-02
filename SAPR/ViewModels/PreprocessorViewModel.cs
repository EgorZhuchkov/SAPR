using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Ribbon;
using SAPR.ConstructionUtils;

namespace SAPR.ViewModels
{
    class PreprocessorViewModel : INotifyPropertyChanged
    {
        private SupportMode supportMode;
        private List<string> errors;
        private bool hasErrors = false;
        public bool HasErrors { get { return hasErrors; } }
        public string Errors
        {
            get { return String.Join("\r\n", errors.ToArray()); }
        }
        
        public ObservableCollection<Rod> Rods { get; set; }
        public ObservableCollection<Strain> Strains { get; set; }
        public SupportMode SupportMode
        {
            get { return supportMode; }
            set
            {
                supportMode = value;
                OnPropertyChanged("SupportMode");
            }
        }

        public PreprocessorViewModel()
        {

            Rods = new ObservableCollection<Rod>
            {
                new Rod { Length = 10.0f, AllowedStress = 5.0f, Area = 20.0f, Elasticity = 2.0f, Index = 1}
            };
            Strains = new ObservableCollection<Strain>
            {
                new Strain {NodeIndex = 1, Magnitude  = 10.0f, StrainType = StrainType.Lengthwise, Index = 1}
            };
            errors = new List<string>();
            Rods[0].PropertyChanged += Rod_PropertyChangedCallback;
            Strains[0].PropertyChanged += Rod_PropertyChangedCallback;
            UpdateConstruction();
        }

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
                      MessageBox.Show($"{errors.Any()}");
                  }));
            }
        }
        #endregion


        public void Rod_PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            UpdateConstruction();
        }

        private void UpdateConstruction()
        {
            //MessageBox.Show("Construction updated");
            ValidateConstruction();
        }

        private void ValidateConstruction()
        {
            errors.Clear();

            for (int i = 0; i < Rods.Count; i++)
            {
                if (Rods[i].Length <= 0)
                {
                    errors.Add($"Длина стержня должна быть больше 0. [Стержень {i + 1}]");
                }

                if (Rods[i].Area <= 0)
                {
                    errors.Add($"Площадь стержня должна быть больше 0. [Стержень {i + 1}]");
                }

                if (Rods[i].Elasticity <= 0)
                {
                    errors.Add($"Модуль упругости стержня должен быть больше 0. [Стержень {i + 1}]");
                }

                if (Rods[i].AllowedStress <= 0)
                {
                    errors.Add($"Допускаемое напряжение стержня должно быть больше 0. [Стержень {i + 1}]");
                }
            }

            for (int i = 0; i < Strains.Count; i++)
            {
                if (Strains[i].Magnitude <= 0)
                {
                    errors.Add($"Нагрузка должна иметь значение больше 0. [Нагрузка {i + 1}]");
                }
            }

            var concentratedStrains = Strains.Where(o => o.StrainType == StrainType.Concentrated);
            var lengthwiseStrains = Strains.Where(o => o.StrainType == StrainType.Lengthwise);

            foreach (var strain in concentratedStrains)
            {
                if ((strain.NodeIndex < 1) || (strain.NodeIndex > Rods.Count + 1))
                {
                    errors.Add($"Сосредоточенные нагрузки должны прикладываться к существующим узлам. [Нагрузка {Strains.IndexOf(strain) + 1}]");
                }

                var concentratedStrainsInSameRod = concentratedStrains.Where(o => o.NodeIndex == strain.NodeIndex);

                if (concentratedStrainsInSameRod.Count() > 2)
                {
                    var sb = new StringBuilder();
                    sb.Append("В одном узле не должно существовать двух и более сонаправленных нагрузок. [Нагрузки ");
                    foreach (var lenghtwiseStrain in concentratedStrainsInSameRod)
                    {
                        sb.Append($"{Strains.IndexOf(lenghtwiseStrain) + 1} ");
                    }
                    sb.Append("]");

                    errors.Add(sb.ToString());
                }
                else if(concentratedStrainsInSameRod.Count() == 2)
                {
                    int xorRes = 0;
                    var sb = new StringBuilder();
                    sb.Append("В одном узле не должно существовать двух и более сонаправленных нагрузок. [Нагрузки ");
                    foreach (var item in concentratedStrainsInSameRod)
                    {
                        xorRes ^= item.NodeIndex;
                        sb.Append($"{Strains.IndexOf(item) + 1} ");
                    }
                    sb.Append("]");

                    if (xorRes >= 0)
                    {
                        errors.Add(sb.ToString());
                    }
                }
            }

            foreach (var strain in lengthwiseStrains)
            {
                if ((strain.NodeIndex < 1) || (strain.NodeIndex > Rods.Count))
                {
                    errors.Add($"Продольные нагрузки должны прикладываться к существующим стержням. [Нагрузка {Strains.IndexOf(strain) + 1}]");
                }

                var lengthwiseStrainsInSameRod = lengthwiseStrains.Where(o => o.NodeIndex == strain.NodeIndex);

                if (lengthwiseStrainsInSameRod.Count() > 1)
                {
                    var sb = new StringBuilder();
                    sb.Append("Две продольные нагрузки не могут быть приложены к одному и тому же стержню. [Нагрузки ");
                    foreach (var lenghtwiseStrain in lengthwiseStrainsInSameRod)
                    {
                        sb.Append($"{Strains.IndexOf(lenghtwiseStrain) + 1} ");
                    }
                    sb.Append("]");

                    errors.Add(sb.ToString());
                }
            }

            hasErrors = errors.Any();
            OnPropertyChanged("Errors");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
