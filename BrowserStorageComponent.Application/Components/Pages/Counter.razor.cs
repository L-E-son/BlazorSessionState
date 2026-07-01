using BrowserStorageComponent.Attributes;

namespace BrowserStorageComponent.Application.Components.Pages
{
    public partial class Counter
    {
        [UseBrowserStorage] public int CurrentCount { get; set; } = 1;

        private void IncrementCount()
        {
            CurrentCount++;
        }
    }
}