using GuestWiFiManager.Components.Models;
using Microsoft.AspNetCore.Components;

namespace GuestWiFiManager.Components.Pages
{
    public class AccessDetailsBase : ComponentBase
    {
        [Inject]
        NavigationManager navManager { get; set; }

        [Inject]
        protected GetPythonApiResponseDetails responseDetails { get; set; }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (responseDetails.dataResponse == null)
            {
                navManager.NavigateTo("/", true);
            }

            return base.OnAfterRenderAsync(firstRender);
        }
    }
}
