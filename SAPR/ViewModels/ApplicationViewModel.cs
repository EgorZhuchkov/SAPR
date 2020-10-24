using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SAPR.ViewModels
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private PreprocessorViewModel _preprocessorViewModel;
        
        private IInputElement _currentFocus;
        public IInputElement CurrentFocus
        {
            get { return _currentFocus; }
            set
            {
                _currentFocus = value;
                OnPropertyChanged("CurrentModeTemplate");
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

        public ApplicationViewModel()
        {
            _preprocessorViewModel = new PreprocessorViewModel();
            SwitchToPanel(new Preprocessor(), _preprocessorViewModel);
        }

        private void SwitchToPanel(UserControl control, object viewModel)
        {
            CurrentModeTemplate = control;
            CurrentModeTemplate.DataContext = viewModel;
            CurrentModeTemplate.Focusable = true;
            CurrentFocus = CurrentModeTemplate;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
