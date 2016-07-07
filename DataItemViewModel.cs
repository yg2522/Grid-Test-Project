using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApplication1.Classes;

namespace WpfApplication1
{
    public class DataItemViewModel : ViewModelBase
    {
        public DataItemViewModel(DataItem di)
        {
            this.Name = di.Name;
            _matches.Add(new TextSelection(0, this.Name.Substring(0, 2)));
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                onPropertyChanged();
            }
        }

        private string _prevName;
        public string PrevName
        {
            get { return _prevName; }
            set
            {
                _prevName = value;
                onPropertyChanged();
            }
        }

        private bool _isVisualDiffVisible;
        public bool IsVisualDiffVisible
        {
            get { return _isVisualDiffVisible; }
            set
            {
                _isVisualDiffVisible = value;
                onPropertyChanged();
            }
        }

        private IList<TextSelection> _matches = new List<TextSelection>();
        public IList<TextSelection> Matches
        {
            get { return _matches; }
            set
            {
                _matches = value;
                onPropertyChanged();
            }
        }
    }
}
