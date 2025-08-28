using Microsoft.JSInterop;

namespace Streamify.Services;

public class BrowserService
{
    public static async Task<int> GetInnerWidth(IJSRuntime _JS)
    {
        return await _JS.InvokeAsync<int>("browser.getInnerWidth");
    }
}