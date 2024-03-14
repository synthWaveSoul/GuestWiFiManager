using BlazorBootstrap;
using GuestWiFiManager.Components.Models;
using GuestWiFiManager.Components.Pages.ModalComponents;
using GuestWiFiManager.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GuestWiFiManager.Components.Pages
{
    public class AccessDetailsBase : ComponentBase
    {
        protected bool isErrorAnywhere = false;

        protected string errorMessage;

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        public Modal modalDetails = default!;

        public Modal modalConfirmRevoke = default!;

        public Modal modalAfterRevoke = default!;

        protected async Task OnHideModalClick()
        {
            await modalAfterRevoke.HideAsync();
        }

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

        protected async Task revokeAccess(string merakiEmailIdToRevoke, string name)
        {
            try
            {
                PreloadService.Show(SpinnerColor.Light, "Work in progress, please wait ...");
                deleteResponseDetails = await responseDetailsService.PythonAPIRevokeAccess(merakiEmailIdToRevoke);
            }
            catch (Exception ex)
            {
                isErrorAnywhere = true;
                errorMessage = ex.Message;
            }
            finally
            {
                PreloadService.Hide();
                modalConfirmRevoke.HideAsync();

                var detailsParameters = new Dictionary<string, object>();

                detailsParameters.Add("Name", name);
                detailsParameters.Add("afterCloseRevoke", EventCallback.Factory.Create<MouseEventArgs>(this, afterCloseRevoke));

                await modalAfterRevoke.ShowAsync<RevokeSuccessfulComponent>(title: "Access revoked", parameters: detailsParameters);
            }
        }

        protected async Task confirmRevokeAccess(string name, string id)
        {
            var detailsParameters = new Dictionary<string, object>();

            detailsParameters.Add("Name", name);
            detailsParameters.Add("revokeConfirmed", EventCallback.Factory.Create<MouseEventArgs>(this, arg => { revokeAccess(id, name); }));
            detailsParameters.Add("revokeCancelled", EventCallback.Factory.Create<MouseEventArgs>(this, arg => { modalConfirmRevoke.HideAsync(); }));

            await modalConfirmRevoke.ShowAsync<ConfirmRevokeComponent>(title: "Confirmation", parameters: detailsParameters);
        }

        protected void afterCloseRevoke()
        {
            OnHideModalClick();
            navManager.NavigateTo(navManager.Uri, true);
        }

        protected async Task showDetails(string login, string password, string name)
        {
            var detailsParameters = new Dictionary<string, object>();

            detailsParameters.Add("Login", login);
            detailsParameters.Add("Password", password);

            await modalDetails.ShowAsync<DetailsComponent>(title: name, parameters: detailsParameters);
        }
    }
}
