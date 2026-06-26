using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorSessionState.Attributes;

namespace BlazorSessionState.Components
{
    public partial class ChildComponent
    {
        [UseBrowserStorage]
        private int SecretCount { get; set; } = 6;

        [Parameter]
        [UseBrowserStorage]
        public int ParentCount { get; set; }

        [Parameter] public EventCallback<int> ParentCountChanged { get; set; }

        public int RegularProperty { get; set; }

        public void IncrementSecretCount()
        {
            SecretCount++;
            //StateHasChanged();
        }
    }
}