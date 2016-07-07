using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfApplication1.Classes;

namespace WpfApplication1
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
        }

        private ObservableCollection<DataItemViewModel> _data = new ObservableCollection<DataItemViewModel>();
        public ObservableCollection<DataItemViewModel> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                onPropertyChanged();
            }
        }

        private ICommand _addDataCommand;
        public ICommand AddDataCommand
        {
            get
            {
                if (_addDataCommand == null)
                    _addDataCommand = new RelayCommand((o) => addDataHandler());
                return _addDataCommand;
            }
        }
        private void addDataHandler()
        {
            var dataSet = DataItem.CreateData(999).Select(x => new DataItemViewModel(x)).ToArray();
            this.Data = new ObservableCollection<DataItemViewModel>(dataSet);
        }

        private ICommand _selectedCellsChangedCommand;
        public ICommand SelectedCellsChangedCommand
        {
            get
            {
                if (_selectedCellsChangedCommand == null)
                    _selectedCellsChangedCommand = new RelayCommand(selectedCellsChangedHandler);
                return _selectedCellsChangedCommand;
            }
        }

        private void selectedCellsChangedHandler(object param = null)
        {
            IEnumerable<object> selected = param as IEnumerable<object>;
            var selectedDataItems = selected.Cast<DataItemViewModel>().ToList();

            // Clone the SelectedEntries
            List<DataItemViewModel> clonedSelectedEntries = new List<DataItemViewModel>(selectedDataItems);

            // keep the previous entry that is part of the selection
            DataItemViewModel previousSelectedItem = null;

            // Keep the previous entry that is in the list
            DataItemViewModel previousItem = null;

            // build list backwards so that diffs happen in order going up
            int viewCount = Data.Count;
            for (int i = viewCount - 1; i >= 0; i--)
            {
                // Get current item in the list
                var currentItem = Data[i] as DataItemViewModel;

                // If the item is part of the selection we will want some kind of diff
                if (clonedSelectedEntries.Contains(currentItem))
                {
                    // If the previous selected item is null
                    // this means that it is the bottom most item in the selection
                    // and compare it to the previous non selected string
                    if (previousSelectedItem != null)
                    {
                        currentItem.PrevName = previousSelectedItem.Name;
                        currentItem.IsVisualDiffVisible = true;
                    }
                    else if (previousItem != null) // if there is a previous selected item compare the item with that one
                    {
                        currentItem.PrevName = previousItem.Name;
                        currentItem.IsVisualDiffVisible = true;
                    }
                    else //the selected item is the bottom item with nothing else to compare, so just show differences from an empty string
                    {
                        currentItem.PrevName = String.Empty;
                        currentItem.IsVisualDiffVisible = true;
                    }

                    // Set the previously selected item to the current item to keep for comparison with the next selected item on the way up
                    previousSelectedItem = currentItem;

                    // clean up
                    clonedSelectedEntries.Remove(currentItem);
                }
                else // if item is not part of the selection we will want the original text
                {
                    currentItem.PrevName = String.Empty;
                    currentItem.IsVisualDiffVisible = false;
                }

                // regardless of whether item is in selection or not we keep it as the previous item when we move up to the next one
                previousItem = currentItem;
            }

        }
    }
}
