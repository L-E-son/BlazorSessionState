namespace BrowserStorageComponent.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class UseBrowserStorageAttribute : Attribute { }
}
