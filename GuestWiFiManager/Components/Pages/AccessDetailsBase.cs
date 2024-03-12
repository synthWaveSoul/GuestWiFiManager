using GuestWiFiManager.Components.Models;
using GuestWiFiManager.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GuestWiFiManager.Components.Pages
{
    public class AccessDetailsBase : ComponentBase
    {
        protected bool isErrorAnywhere = false;

        protected string errorMessage;

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

        [Inject]
        protected IResponseDetailsService responseDetailsService { get; set; }

        protected DeletePythonApiResponseDetails deleteResponseDetails { get; set; }

        protected async Task revokeAccess(string merakiEmailIdToRevoke)
        {
            try
            {
                deleteResponseDetails = await responseDetailsService.PythonAPIRevokeAccess(merakiEmailIdToRevoke);
            }
            catch (Exception ex)
            {
                isErrorAnywhere = true;
                errorMessage = ex.Message;
            }
        }
    }
}
