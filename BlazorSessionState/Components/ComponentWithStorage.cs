using BlazorSessionState.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlazorSessionState.Components
{
    public abstract class ComponentWithStorage<TStorageKind> : ComponentBase where TStorageKind : ProtectedBrowserStorage
    {
        private readonly ObservableCollection<KeyValuePair<string, object?>> _values;
        private readonly Dictionary<string, Type?> _types = [];

        private static readonly NotifyCollectionChangedAction[] _notifyCollectionChangedActions =
        {
            NotifyCollectionChangedAction.Add,
            NotifyCollectionChangedAction.Replace
        };

        [Inject] private TStorageKind BrowserStorage { get; set; } = default!;

        protected ComponentWithStorage()
        {
            _values = [];

            _values.CollectionChanged += ValuesCollectionChanged;
        }

        ~ComponentWithStorage()
        {
            _values.CollectionChanged -= ValuesCollectionChanged;
        }

        private async void ValuesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_notifyCollectionChangedActions.Contains(e.Action))
            {
                return;
            }

            var newItems = e.NewItems?.Cast<KeyValuePair<string, object>>() ?? [];
            foreach (var item in newItems)
            {
                var newValue = item.Value;
                var valueType = _types[item.Key];

                var serialized = JsonSerializer.Serialize(newValue, valueType!);
                await BrowserStorage.SetAsync(item.Key, serialized);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await TryLoadStorageValues();
            }
            else
            {
                SetStorageValues();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private IEnumerable<PropertyInfo> GetStorageAttributeProperties()
        {
            return this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<UseBrowserStorage>() != null);
        }

        private async Task TryLoadStorageValues()
        {
            foreach (var property in this.GetStorageAttributeProperties())
            {
                var propertyName = property.Name!;
                var propertyType = property.PropertyType;
                var storageValue = await BrowserStorage.GetAsync<string>(propertyName);

                _types.TryAdd(propertyName, propertyType);

                if (storageValue.Success)
                {
                    var deserialized = JsonSerializer.Deserialize(storageValue.Value!, propertyType);
                    property.SetValue(this, deserialized);

                    StateHasChanged();
                }
            }
        }

        private void SetStorageValues()
        {
            foreach (var property in this.GetStorageAttributeProperties())
            {
                var propertyName = property.Name!;
                var propertyValue = property.GetValue(this);

                SetStorageValue(propertyName, propertyValue);
            }
        }

        private void SetStorageValue(string propertyName, object? storageValue)
        {
            var valueIfFound = _values.FirstOrDefault(v => string.Equals(v.Key, propertyName, StringComparison.Ordinal));

            // Add
            if (Equals(valueIfFound, default(KeyValuePair<string, object?>)))
            {
                var type = storageValue?.GetType();
                _types[propertyName] = type;

                var newValue = new KeyValuePair<string, object?>(propertyName, storageValue);
                _values.Add(newValue);

                return;
            }

            // Do nothing if the value didn't change
            if (Equals(valueIfFound.Value, storageValue))
            {
                return;
            }

            // Replace
            var index = _values.IndexOf(valueIfFound);
            _values[index] = new KeyValuePair<string, object?>(propertyName, storageValue);
        }
    }
}
