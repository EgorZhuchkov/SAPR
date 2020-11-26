using Microsoft.Win32;
using Newtonsoft.Json;
using SAPR.ConstructionUtils;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SAPR.ViewModels
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private Construction _construction = null;
        private string _currentFilePath = null;
        private PreprocessorViewModel _preprocessorViewModel = null;

        public UserControl _currentModeTemplate;
        public UserControl CurrentModeTemplate
        {
            get { return _currentModeTemplate; }
            set
            {
                _currentModeTemplate = value;
                OnPropertyChanged("CurrentModeTemplate");
            }
        }

        public ApplicationViewModel()
        {
            _construction = new Construction();
            _preprocessorViewModel = new PreprocessorViewModel(_construction);
            SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
        }

        #region Commands

        private RelayCommand _testCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return _testCommand ??
                  (_testCommand = new RelayCommand(o =>
                  {
                      MessageBox.Show("Test");
                  }));
            }
        }

        private RelayCommand _preprocessorCommand;
        public RelayCommand PreprocessorCommand
        {
            get
            {
                return _preprocessorCommand ??
                  (_preprocessorCommand = new RelayCommand(o =>
                  {
                      SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
                  }));
            }
        }

        private RelayCommand _processorCommand;
        public RelayCommand ProcessorCommand
        {
            get
            {
                return _processorCommand ??
                  (_processorCommand = new RelayCommand(o =>
                  {
                      MessageBox.Show("Not yet implemented");
                      //CurrentModeTemplate = new Processor();
                  }));
            }
        }

        private RelayCommand _postprocessorCommand;
        public RelayCommand PostprocessorCommand
        {
            get
            {
                return _postprocessorCommand ??
                  (_postprocessorCommand = new RelayCommand(o =>
                  {
                      // CurrentModeTemplate = new Postprocessor();
                      MessageBox.Show("Not yet implemented");
                  }));
            }
        }

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                  (_saveCommand = new RelayCommand(obj =>
                  {
                      SaveConstrictionToFile();
                  },
                  (obj) => !_preprocessorViewModel.HasErrors));
            }
        }

        private RelayCommand _saveAsCommand;
        public RelayCommand SaveAsCommand
        {
            get
            {
                return _saveAsCommand ??
                  (_saveAsCommand = new RelayCommand(obj =>
                  {
                      SaveConstrictionToNewFile();
                  },
                  (obj) => !_preprocessorViewModel.HasErrors));
            }
        }

        private RelayCommand _openCommand;
        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand ??
                  (_openCommand = new RelayCommand(obj =>
                  {
                      LoadConstructionFromFile();
                  }));
            }
        }

        private RelayCommand _newProjectCommand;
        public RelayCommand NewProjectCommand
        {
            get
            {
                return _newProjectCommand ??
                  (_newProjectCommand = new RelayCommand(obj =>
                  {
                      CreateNewProject();
                  }));
            }
        }

        private RelayCommand _exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand ??
                  (_exitCommand = new RelayCommand(o =>
                  {
                      Application.Current.Shutdown();
                  }));
            }
        }

        #endregion

        private void SwitchToPanel(UserControl control, object viewModel)
        {
            CurrentModeTemplate = control;
            CurrentModeTemplate.DataContext = viewModel;
        }

        private void SaveConstrictionToFile()
        {
            if (_currentFilePath == null)
            {
                SaveConstrictionToNewFile();
            }
            else
            {
                string serializedConstruction = JsonConvert.SerializeObject(_construction);
                File.WriteAllText(_currentFilePath, serializedConstruction);
            }   
        }

        private void SaveConstrictionToNewFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                string serializedConstruction = JsonConvert.SerializeObject(_construction);
                File.WriteAllText(saveFileDialog.FileName, serializedConstruction);

                _currentFilePath = saveFileDialog.FileName;
            }
        }

        private void LoadConstructionFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string serializedConstruction = File.ReadAllText(openFileDialog.FileName);

                _construction = JsonConvert.DeserializeObject<Construction>(serializedConstruction);
                _currentFilePath = openFileDialog.FileName;

                _preprocessorViewModel.UpdatePreprocessor(_construction);
                SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
            }
        }

        private void CreateNewProject()
        {
            _currentFilePath = null;
            _construction = new Construction();

            _preprocessorViewModel.UpdatePreprocessor(_construction);
            SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
