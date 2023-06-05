using Microsoft.JSInterop;

namespace BlazorDemo.Shared;

public static class JsRuntimeExtensions
{
    /// <summary>
    /// Calls "console.log" on the client passing the args along with it.
    /// </summary>
    /// <example>
    /// LogAsync("data") //same as console.log('data')
    /// </example>
    /// <example>
    /// LogAsync("data", myData) //same as console.log('data', myData)
    /// </example>
    /// <param name="js"></param>
    /// <param name="args"></param>
    public static async Task LogAsync(this IJSRuntime js, params object?[]? args)
    {
        await js.InvokeVoidAsync("console.log", args);
    }
    
    /// <summary>
    /// Calls "console.table" on the client passing the args along with it.
    /// </summary>
    /// <example>
    /// TableAsync(myData) //same as console.table(data)
    /// </example>
    /// <example>
    /// TableAsync(myData, new []{"firstName", "lastName"}) //same as console.table(myData, ["firstName", "lastName"])
    /// </example>
    /// <param name="js"></param>
    /// <param name="o"></param>
    /// <param name="fields"></param>
    public static async Task TableAsync(this IJSRuntime js, object? o, string[]? fields = null)
    {
        await js.InvokeVoidAsync("console.table", o, fields);
    }
}
