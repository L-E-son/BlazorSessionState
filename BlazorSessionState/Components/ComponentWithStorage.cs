using BlazorSessionState.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;

namespace BlazorSessionState.Components
{
    public abstract class ComponentWithBrowserStorage<TStorageKind> : ComponentBase where TStorageKind : ProtectedBrowserStorage
    {
        private readonly ObservableCollection<KeyValuePair<string, object?>> _values;
        private readonly Dictionary<string, Type> _types = [];
        private readonly Dictionary<string, object?> _defaultValues = [];

        private bool _storageLoaded = false;

        [Inject] private TStorageKind BrowserStorage { get; set; } = default!;

        protected ComponentWithBrowserStorage()
        {
            _values = [];
            _values.CollectionChanged += ValuesCollectionChanged;

            InitializeTypes();
        }

        ~ComponentWithBrowserStorage()
        {
            _values.CollectionChanged -= ValuesCollectionChanged;
        }

        private void InitializeTypes()
        {
            foreach (var property in GetStorageAttributeProperties())
            {
                var propertyName = property.Name!;
                var propertyValue = property.GetValue(this);
                var propertyType = property.PropertyType 
                    ?? throw new Exception($"Could not get type for property {propertyName}.");

                if (!property.CanWrite)
                {
                    throw new Exception($"Property {propertyName} must have a setter.");
                }

                _types.Add(propertyName, propertyType);
                _values.Add(new(propertyName, propertyValue));

                if (propertyValue is IEnumerable enumerable)
                {
                    var immutable = enumerable.Cast<object?>().ToArray();
                    _defaultValues.Add(propertyName, immutable);
                }
                else
                {
                    _defaultValues.Add(propertyName, propertyValue);
                }
            }
        }

        private async void ValuesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Replace)
            {
                return;
            }

            var updates = e.NewItems?.Cast<KeyValuePair<string, object?>>() ?? [];
            foreach (var item in updates)
            {
                var newValue = item.Value;
                var valueType = _types[item.Key];

                var serialized = JsonSerializer.Serialize(newValue, valueType!);
                var storageKey = GetStorageKey(item.Key);
                await BrowserStorage.SetAsync(storageKey, serialized);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_storageLoaded)
            {
                await TryLoadStorageValues();
                _storageLoaded = true;
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
                .Where(p => p.GetCustomAttribute<UseBrowserStorageAttribute>() != null);
        }

        private async Task TryLoadStorageValues()
        {
            foreach (var property in this.GetStorageAttributeProperties())
            {
                var propertyName = property.Name!;
                var propertyType = property.PropertyType;

                var storageKey = GetStorageKey(propertyName);
                var storageValue = await BrowserStorage.GetAsync<string?>(storageKey);

                if (storageValue.Success)
                {
                    var deserialized = JsonSerializer.Deserialize(storageValue.Value!, propertyType!);
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

        private void SetStorageValue(string propertyName, object? newStorageValue)
        {
            bool equals;
            var defaultValue = _defaultValues[propertyName];

            if (newStorageValue is IEnumerable newEnumerable && defaultValue is IEnumerable defaultEnumerable)
            {
                equals = CollectionEquals(newEnumerable, defaultEnumerable);
            }
            else
            {
                equals = Equals(newStorageValue, defaultValue);
            }

            // Do nothing if the value is default
            if (equals)
            {
                return;
            }

            var valueIfFound = _values.FirstOrDefault(v => string.Equals(v.Key, propertyName, StringComparison.Ordinal));

            // Replace
            var index = _values.IndexOf(valueIfFound);
            _values[index] = new KeyValuePair<string, object?>(propertyName, newStorageValue);
        }

        private string GetStorageKey(string propertyName) => $"{this.GetType().Name}.{propertyName}";

        private static bool CollectionEquals(IEnumerable left, IEnumerable right)
        {
            var leftCasted = left.Cast<object?>().ToArray();
            var rightCasted = right.Cast<object?>().ToArray();

            return Enumerable.SequenceEqual(leftCasted, rightCasted);
        }
    }
}
