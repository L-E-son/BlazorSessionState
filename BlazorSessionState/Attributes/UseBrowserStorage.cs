namespace BlazorSessionState.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class UseBrowserStorage : Attribute { }
}
