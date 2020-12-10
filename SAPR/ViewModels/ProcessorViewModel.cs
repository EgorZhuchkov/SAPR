using SAPR.ConstructionUtils;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SAPR.ViewModels
{
    class ProcessorViewModel : INotifyPropertyChanged
    {
        private Construction cachedConstruction;

        public ProcessorViewModel(Construction construction)
        {
            cachedConstruction = construction;
        }

        #region Commands
        private RelayCommand _processConstructionCommand;
        public RelayCommand ProcessConstructionCommand
        {
            get
            {
                return _processConstructionCommand ??
                  (_processConstructionCommand = new RelayCommand(obj =>
                  {
                      MessageBox.Show("Not yet implemented");
                  },
                  (obj) => cachedConstruction.IsValid()));
            }
        }

        private RelayCommand _saveResultsCommand;
        public RelayCommand SaveResultsCommand
        {
            get
            {
                return _saveResultsCommand ??
                  (_saveResultsCommand = new RelayCommand(obj =>
                  {
                      MessageBox.Show("Not yet implemented");
                  }));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
