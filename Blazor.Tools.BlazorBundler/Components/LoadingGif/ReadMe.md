# Example on how to use LoadingGif component

## In razor component or page:

``` c#
@page "/loading-example"
@using YourNamespace // Import the namespace where LoadingGif resides
@inject LoadingService LS

<h3>Loading Example</h3>

<button @onclick="ToggleLoading">Toggle Loading</button>

<LoadingGif IsLoading="@isLoading" Message="Fetching data, please wait..." />

@code {
    private bool isLoading = false;

    private async Task ToggleLoading()
    {
        isLoading = !isLoading;

        if (isLoading)
        {
            await Task.Delay(3000); // Simulate a data fetch operation
        }

        isLoading = false;
    }
}
```