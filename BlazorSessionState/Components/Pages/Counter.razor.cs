
using Microsoft.AspNetCore.Components;

namespace BlazorSessionState.Components.Pages
{
    public partial class Counter
    {
        [Parameter] public int CurrentCount { get; set; } = 1;

        private void IncrementCount()
        {
            CurrentCount++;
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            var gotValue = parameters.TryGetValue<int>(nameof(CurrentCount), out var currentCount);

            return base.SetParametersAsync(parameters);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }
    }
}