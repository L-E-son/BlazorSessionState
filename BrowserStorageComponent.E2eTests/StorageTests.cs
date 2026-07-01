using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright.NUnit;

namespace BrowserStorageComponent.E2eTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class StorageTests : PageTest
{
    private static readonly Uri RootUri = new("https://localhost:7194");
    private static readonly string AppUrl = $"{RootUri.AbsoluteUri}counter";

    private readonly WebApplicationFactory<Application.Program> _webApplicationFactory = new();
    private HttpClient? _httpClient;

    [SetUp]
    public async Task Setup()
    {
        _httpClient = _webApplicationFactory.CreateClient();
    }

    [Test]
    public async Task GoToCounter()
    {
        await Page.GotoAsync(AppUrl);
        ;
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }
}
