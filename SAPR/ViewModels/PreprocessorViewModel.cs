using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using System.Windows.Shapes;
using SAPR.ConstructionUtils;
using SAPR.UIElements;

namespace SAPR.ViewModels
{
    class PreprocessorViewModel : INotifyPropertyChanged
    {
        public ConstructionCanvas UC_ConstructionCanvas { get; private set; }

        private Construction _cachedConstruction;
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
        public SupportMode SupportsMode
        {
            get { return supportMode; }
            set
            {
                supportMode = value;
                UpdateConstruction();
                OnPropertyChanged("SupportsMode");
            }
        }

        public PreprocessorViewModel(Construction construction)
        {
            _cachedConstruction = construction;
            Rods = new ObservableCollection<Rod>(_cachedConstruction.Rods);
            Strains = new ObservableCollection<Strain>(_cachedConstruction.Strains);
            errors = new List<string>();
            UC_ConstructionCanvas = new ConstructionCanvas(Rods, Strains, SupportsMode);
            UC_ConstructionCanvas.SizeChanged += UC_ConstructionCanvas_SizeChanged;

            if (_cachedConstruction.HasRightSupport && _cachedConstruction.HasLeftSupport)
            {
                SupportsMode = SupportMode.Both;
            }
            else if(_cachedConstruction.HasRightSupport)
            {
                SupportsMode = SupportMode.Right;
            }
            else if (_cachedConstruction.HasLeftSupport)
            {
                SupportsMode = SupportMode.Left;
            }

            foreach (var rod in Rods)
            {
                rod.PropertyChanged += Construction_PropertyChangedCallback;
            }
            foreach (var strain in Strains)
            {
                strain.PropertyChanged += Construction_PropertyChangedCallback;
            }
        }

        public void UpdatePreprocessor(Construction construction)
        {
            _cachedConstruction = construction;
            Rods = new ObservableCollection<Rod>(_cachedConstruction.Rods);
            Strains = new ObservableCollection<Strain>(_cachedConstruction.Strains);
            errors = new List<string>();

            UC_ConstructionCanvas.ClearConstruction();
            UC_ConstructionCanvas.UpdateConstructionCanvas(Rods, Strains, SupportsMode);
            if (_cachedConstruction.HasRightSupport && _cachedConstruction.HasLeftSupport)
            {
                SupportsMode = SupportMode.Both;
            }
            else if (_cachedConstruction.HasRightSupport)
            {
                SupportsMode = SupportMode.Right;
            }
            else if (_cachedConstruction.HasLeftSupport)
            {
                SupportsMode = SupportMode.Left;
            }

            foreach (var rod in Rods)
            {
                rod.PropertyChanged += Construction_PropertyChangedCallback;
            }
            foreach (var strain in Strains)
            {
                strain.PropertyChanged += Construction_PropertyChangedCallback;
            }
        }

        private void UC_ConstructionCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(errors.Count == 0)
            {
                UC_ConstructionCanvas.DrawConstruction(supportMode);
            }
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
                      rod.PropertyChanged += Construction_PropertyChangedCallback;
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
                      strain.PropertyChanged += Construction_PropertyChangedCallback;
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
                      UC_ConstructionCanvas.ShowDebugInfo();
                  }));
            }
        }
        #endregion


        public void Construction_PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            UpdateConstruction();
        }

        private void UpdateConstruction()
        {
            ValidateConstruction();
            //UpdateConstructionCanvas();
            if(errors.Count == 0)
            {
                UC_ConstructionCanvas.DrawConstruction(supportMode);
                UpdateMainConstruction();
            }
            else
            {
                UC_ConstructionCanvas.ClearConstruction();
            }
            
        }

        private void ValidateConstruction()
        {
            errors.Clear();

            if(Rods.Count == 0)
            {
                errors.Add("В конструкции отсутствуют стержни");
            }

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
                if (Math.Abs(Strains[i].Magnitude) < Double.Epsilon)
                {
                    errors.Add($"Нагрузка должна иметь значение больше 0. [Нагрузка {i + 1}]");
                }
            }

            var concentratedStrains = Strains.Where(o => o.StrainType == StrainType.Concentrated);
            var lengthwiseStrains = Strains.Where(o => o.StrainType == StrainType.Lengthwise);

            foreach (var strain in concentratedStrains)
            {
                if ((strain.NodeIndex <= 0) || (strain.NodeIndex > Rods.Count + 1))
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
                        var sign = Math.Sign(item.Magnitude);
                        xorRes ^= sign;
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
                if ((strain.NodeIndex <= 0) || (strain.NodeIndex > Rods.Count))
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

        //private void UpdateConstructionCanvas()
        //{
        //    constructionCanvas = new Canvas();

        //    if (errors.Count == 0)
        //    {
        //        DrawConstruction();
        //    }

        //    OnPropertyChanged("ConstructionCanvas");
        //}

        private void UpdateMainConstruction()
        {
            _cachedConstruction.Rods = Rods.ToList<Rod>();
            _cachedConstruction.Strains = Strains.ToList<Strain>();
            _cachedConstruction.HasLeftSupport = (SupportsMode == SupportMode.Both || SupportsMode == SupportMode.Left);
            _cachedConstruction.HasRightSupport = (SupportsMode == SupportMode.Both || SupportsMode == SupportMode.Right);
        }

        //private void DrawConstruction()
        //{
        //    var height = constructionCanvas.Height;
        //    var width = constructionCanvas.Width;

        //    const int heightOffset = 50;
        //    const int widthOffset = 50;

        //    var rodsTotalLength = Rods.ToList().Select(rod => rod.Length).Sum();
        //    var rodsMaxArea = Rods.ToList().Select(rod => rod.Area).Max();

        //    var rectangleWidth = rodsTotalLength / (width - 2 * widthOffset);
        //    var rectangleHeight = rodsMaxArea / (height - 3 * heightOffset);

        //    double currentOffset = widthOffset;

        //    #region Supports

        //    var leftSupport = new Path()
        //    {
        //        Data = Geometry.Parse("M 0 10 L 10 0 M 0 20 L 10 10 M 0 30 L 10 20 M 0 40 L 10 30 M 0 50 L 10 40 M 0 60 L 10 50 M 0 70 L 10 60 M 0 80 L 10 70 M 10 0 L 10 70"),
        //        Stroke = Brushes.Black,
        //        StrokeThickness = 1,
        //        RenderTransform = new ScaleTransform(1, 1)
        //    };

        //    Canvas.SetLeft(leftSupport, currentOffset - 10);
        //    Canvas.SetTop(leftSupport, height / 2 - 45);

        //    var rightSupport = new Path()
        //    {
        //        Data = Geometry.Parse("M 0 10 L 10 0 M 0 20 L 10 10 M 0 30 L 10 20 M 0 40 L 10 30 M 0 50 L 10 40 M 0 60 L 10 50 M 0 70 L 10 60 M 0 80 L 10 70 M 10 0 L 10 70"),
        //        Stroke = Brushes.Black,
        //        StrokeThickness = 1,
        //        RenderTransform = new TransformGroup()
        //        {
        //            Children = {
        //                new ScaleTransform(1, 1),
        //                new RotateTransform(180)
        //            }
        //        }
        //    };

        //    Canvas.SetRight(rightSupport, currentOffset - 25);
        //    Canvas.SetTop(rightSupport, height / 2 - 45);

        //    if (SupportsMode == SupportMode.Both)
        //    {
        //        constructionCanvas.Children.Add(leftSupport);
        //        constructionCanvas.Children.Add(rightSupport);
        //    }
        //    else if(SupportsMode == SupportMode.Left)
        //    {
        //        constructionCanvas.Children.Add(leftSupport);
        //    }
        //    else
        //    {
        //        constructionCanvas.Children.Add(rightSupport);
        //    }

        //    #endregion



        //}

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
