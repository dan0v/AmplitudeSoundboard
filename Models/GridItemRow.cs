using Amplitude.ViewModels;
using System.Collections.ObjectModel;

namespace Amplitude.Models
{
    public class GridItemRow
    {
        private ObservableCollection<SoundBoardGridItemViewModel> _list = new();
        public ObservableCollection<SoundBoardGridItemViewModel> List => _list;

        public GridItemRow() { }
    }
}
