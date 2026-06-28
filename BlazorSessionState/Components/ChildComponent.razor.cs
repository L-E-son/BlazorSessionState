using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorSessionState.Attributes;

namespace BlazorSessionState.Components
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
        [UseBrowserStorage]
        public int ParentCount { get; set; }

        //[UseBrowserStorage]
        public int DerivedCount => ParentCount - 1;

        [UseBrowserStorage]
        public int ThirdCount { get; init; }

        [Parameter] public EventCallback<int> ParentCountChanged { get; set; }

        public int RegularProperty { get; set; }

        public void IncrementSecretCount()
        {
            SecretCount++;
            //StateHasChanged();
        }
    }
}