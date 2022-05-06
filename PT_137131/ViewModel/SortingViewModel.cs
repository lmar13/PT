using PT_137131.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_137131.ViewModel
{
    public class SortingViewModel : ViewModelBase
    {
        private SortBy sortBy;
        private Direction direction;
        private TaskCreationOptions taskCreationOptions;

        public SortBy SortBy
        {
            get { return sortBy; }
            set
            {
                sortBy = value;
                NotifyPropertyChanged();
            }
        }

        public Direction Direction
        {
            get { return direction; }
            set { direction = value; NotifyPropertyChanged(); }
        }

        public TaskCreationOptions TaskCreationOptions
        {
            get { return taskCreationOptions; }
            set { taskCreationOptions = value; NotifyPropertyChanged(); }
        }
    }
}
