using Microsoft.Win32;
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
        private Construction construction;
        private PreprocessorViewModel _preprocessorViewModel;
        
        private IInputElement _currentFocus;
        public IInputElement CurrentFocus
        {
            get { return _currentFocus; }
            set
            {
                _currentFocus = value;
                OnPropertyChanged("CurrentFocus");
            }
        }

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
            _preprocessorViewModel = new PreprocessorViewModel();
            SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
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

        private void SwitchToPanel(UserControl control, object viewModel)
        {
            CurrentModeTemplate = control;
            CurrentModeTemplate.DataContext = viewModel;
            CurrentModeTemplate.Focusable = true;
            CurrentFocus = CurrentModeTemplate;
        }

        private void SaveConstrictionToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, "Hello");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
