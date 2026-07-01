using Microsoft.AspNetCore.Components;
using BrowserStorageComponent.Attributes;

namespace BrowserStorageComponent.Application.Components
{
    public partial class ChildComponent
    {
        [UseBrowserStorage]
        private int SecretCount { get; set; } = 6;

        [UseBrowserStorage]
        private string? SecretKey { get; set; } = null;

        [UseBrowserStorage]
        private string SecretValue { get; set; } = "DLL";

        [Parameter]
        public int ParentCount { get; set; }

        //[UseBrowserStorage]
        public int DerivedCount => ParentCount - 1;

        [UseBrowserStorage]
        public int FourthCount { get; init; }

        [UseBrowserStorage]
        public SimpleCount SimpleCount { get; set; } = new(1, 2);

        [UseBrowserStorage]
        public IList<int> Counts { get; set; } = [];

        [UseBrowserStorage]
        public IList<object> BoxedCounts { get; set; } = [];

        [Parameter] public EventCallback<int> ParentCountChanged { get; set; }

        public int RegularProperty { get; set; }

        public void IncrementSecretCount()
        {
            SecretCount++;
        }

        public void SetCounts()
        {
            Counts.Clear();
            Counts.Add(SecretCount);
            Counts.Add(ParentCount);
            Counts.Add(DerivedCount);
            Counts.Add(FourthCount);
        }
    }

    public record SimpleCount(int Count1, int Count2) { }
}