using System.Collections.Generic;
using ObservableCollections;

namespace NetNotepad.Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public ObservableList<string> List { get; } = [.. Generate(1000)];

        private static List<string> Generate(int count)
        {
            List<string> result = new(count);
            for (int i = 0; i < count; i++) { result.Add(i.ToString()); }
            return result;
        }
    }
}