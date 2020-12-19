using Microsoft.Win32;
using OxyPlot.Series;
using SAPR.ConstructionUtils;
using SAPR.UIElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace SAPR.ViewModels
{
    class PostprocessorViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private ProcessorViewModel _processor;
        private Construction _construction;

        private float _samplingFrequency = 0.01f;
        public float SamplingFrequency
        {
            get { return _samplingFrequency; }
            set
            {
                _samplingFrequency = value;
                OnPropertyChanged("SamplingFrequency");
                if (value > 0)
                    RecalculateResults();
            }
        }

        public ObservableCollection<CalculationSlice> CalculationResults { get; set; }

        public ObservableCollection<int> AvaliableRodIndexes { get; set; }
        private int _currentRodIndex = 1;
        public int CurrentRodIndex
        {
            get { return _currentRodIndex; }
            set
            {
                _currentRodIndex = value;
                RecalculateResults();
                OnPropertyChanged("CurrentRodIndex");
            }
        }

        private int _sliceRodIndex = 1;
        public int SliceRodIndex
        {
            get { return _sliceRodIndex; }
            set
            {
                _sliceRodIndex = value;
                OnPropertyChanged("SliceRodIndex");
                if (_sectionPosition >= 0.0f && _sectionPosition <= _construction.Rods[_currentRodIndex - 1].Length)
                {
                    OnPropertyChanged("SectionUx");
                    OnPropertyChanged("SectionNx");
                    OnPropertyChanged("SectionSigmaX");
                }
            }
        }

        private float _sectionPosition = 0.1f;
        public float SectionPosition
        {
            get { return _sectionPosition; }
            set
            {
                _sectionPosition = value;
                OnPropertyChanged("SectionPosition");
                if (value >= 0.0f && value <= _construction.Rods[_currentRodIndex - 1].Length)
                {
                    OnPropertyChanged("SectionUx");
                    OnPropertyChanged("SectionNx");
                    OnPropertyChanged("SectionSigmaX");
                }
            }
        }

        public double SectionUx
        {
            get { return _processor.GetU(_sectionPosition, _sliceRodIndex - 1); }
        }

        public double SectionNx
        {
            get { return _processor.GetN(_sectionPosition, _sliceRodIndex - 1); }
        }

        public double SectionSigmaX
        {
            get { return _processor.GetSigma(_sectionPosition, _sliceRodIndex - 1); }
        }

        public List<string> PlotModes { get; set; }

        private string _currentPlotMode;
        public string CurrentPlotMode
        {
            get { return _currentPlotMode; }
            set
            {
                _currentPlotMode = value;
            }
        }

        private int _diagramRodIndex = 1;
        public int DiagramRodIndex
        {
            get { return _diagramRodIndex; }
            set
            {
                _diagramRodIndex = value;
                OnPropertyChanged("DiagramRodIndex");
            }
        }

        #region Commands

        private RelayCommand _testCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return _testCommand ??
                  (_testCommand = new RelayCommand(obj =>
                  {
                      //MessageBox.Show(DiagramModel.Series.ToString());
                  }));
            }
        }

        private RelayCommand _saveTableCommand;
        public RelayCommand SaveTableCommand
        {
            get
            {
                return _saveTableCommand ??
                  (_saveTableCommand = new RelayCommand(obj =>
                  {
                      SaveTableToFile();
                  }));
            }
        }

        private RelayCommand _showPlotCommand;
        public RelayCommand ShowPlotCommand
        {
            get
            {
                return _showPlotCommand ??
                  (_showPlotCommand = new RelayCommand(obj =>
                  {
                      var plotWindow = new PlotWindow(_currentPlotMode, _processor, _diagramRodIndex - 1, _construction);
                      plotWindow.Show();
                  }));
            }
        }

        #endregion

        private void SaveTableToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(saveFileDialog.FileName);
                var separator = String.Empty;

                switch (extension.ToLower())
                {
                    case ".csv":
                        separator = ",";
                        break;
                    default:
                        separator = "\t";
                        break;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"X{separator}U(x){separator}N(x){separator}Sigma(x)");
                foreach (var calculation in CalculationResults)
                {
                    sb.AppendLine($"{calculation.X}{separator}{calculation.Ux}{separator}{calculation.Nx}{separator}{calculation.SigmaX}");
                }
                File.WriteAllText(saveFileDialog.FileName, sb.ToString());
            }
        }

        public PostprocessorViewModel(Construction construction, ProcessorViewModel processor)
        {
            _processor = processor;
            _construction = construction;
            CalculationResults = new ObservableCollection<CalculationSlice>();
            AvaliableRodIndexes = new ObservableCollection<int>();
            PlotModes = new List<string>
            {
                "N(x)",
                "U(x)",
                "Sigma(x)"
            };
            CurrentPlotMode = "N(x)";
        }

        public void UpdatePostProcessor(Construction construction)
        {
            _construction = construction;

            AvaliableRodIndexes.Clear();

            for (int i = 0; i < _construction.Rods.Count; i++)
            {
                AvaliableRodIndexes.Add(i + 1);
            }
            CurrentRodIndex = 1;
            DiagramRodIndex = 1;
            SliceRodIndex = 1;

            RecalculateResults();
        }

        public void RecalculateResults()
        {
            if(!_construction.IsProcessed)
            {
                return;
            }

            CalculationResults.Clear();
            var dataSlices = _construction.Rods[_currentRodIndex - 1].Length / _samplingFrequency;
            var currentX = 0.0f;

            for (int i = 0; i <= dataSlices; i++)
            {
                CalculationResults.Add(new CalculationSlice
                {
                    X = Math.Round(currentX, 4),
                    Nx = Math.Round(_processor.GetN(currentX, _currentRodIndex - 1), 4),
                    Ux = Math.Round(_processor.GetU(currentX, _currentRodIndex - 1), 4),
                    SigmaX = Math.Round(_processor.GetSigma(currentX, _currentRodIndex - 1), 4)
                });
                currentX += _samplingFrequency;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "SamplingFrequency":
                        if (SamplingFrequency <= 0)
                        {
                            error = "Частота дискретизации должна быть больше 0";
                        }
                        break;
                    case "SectionPosition":
                        if(SamplingFrequency <= 0)
                        {
                            error = "Позиция сечения должна быть больше 0";
                        }
                        else if(SamplingFrequency > _construction.Rods[_currentRodIndex - 1].Length)
                        {
                            error = "Позиция сечения должна быть меньше длины стержня";
                        }
                        break;
                }
                return error;
            }
        }
    }
}
