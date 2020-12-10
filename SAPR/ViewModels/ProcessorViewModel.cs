using SAPR.ConstructionUtils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SAPR.ViewModels
{
    class ProcessorViewModel : INotifyPropertyChanged
    {
        public bool IsActive { set { OnPropertyChanged("ProcessorData"); } }
        public string ProcessorData
        {
            get
            {
                if(cachedConstruction.IsProcessed)
                {
                    return "Processed data";
                }
                else if(cachedConstruction.IsValid())
                {
                    return "Please, process data";
                }
                else
                {
                    return "Construction is not valid";
                }
            }
        }
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
                      CalculateResults();
                  },
                  (obj) => (!cachedConstruction.IsProcessed && cachedConstruction.IsValid())));
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
                  },
                  (obj) => cachedConstruction.IsProcessed));
            }
        }
        #endregion

        private void CalculateResults()
        {
            cachedConstruction.IsProcessed = true;
            OnPropertyChanged("ProcessorData");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
