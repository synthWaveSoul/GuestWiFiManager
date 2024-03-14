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
        // for error handling on the page
        protected bool isErrorAnywhere = false;
        protected string errorMessage;

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        public Modal modalDetails = default!;
        public Modal modalError = default!;
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

        // if there is no data from the server needed to display the page, go to the home page
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (responseDetails.dataResponse == null)
            {
                navManager.NavigateTo("/", true);
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        // show pop up window with login and password for selected user
        protected async Task showDetails(string login, string password, string name)
        {
            var detailsParameters = new Dictionary<string, object>();

            detailsParameters.Add("Login", login);
            detailsParameters.Add("Password", password);

            await modalDetails.ShowAsync<DetailsComponent>(title: name, parameters: detailsParameters);
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
                if (!isErrorAnywhere && deleteResponseDetails.error == "no_error")
                {
                    PreloadService.Hide();
                    modalConfirmRevoke.HideAsync();

                    var detailsParameters = new Dictionary<string, object>();

                    detailsParameters.Add("Name", name);
                    detailsParameters.Add("afterCloseRevoke", EventCallback.Factory.Create<MouseEventArgs>(this, afterCloseRevoke));

                    await modalAfterRevoke.ShowAsync<RevokeSuccessfulComponent>(title: "Access revoked", parameters: detailsParameters);
                }
                else
                {
                    PreloadService.Hide();
                    modalConfirmRevoke.HideAsync();
                    modalError.ShowAsync();
                }
            }
        }

        // ask if sure to revoke access
        protected async Task confirmRevokeAccess(string name, string id)
        {
            var detailsParameters = new Dictionary<string, object>();

            detailsParameters.Add("Name", name);
            detailsParameters.Add("revokeConfirmed", EventCallback.Factory.Create<MouseEventArgs>(this, arg => { revokeAccess(id, name); }));
            detailsParameters.Add("revokeCancelled", EventCallback.Factory.Create<MouseEventArgs>(this, arg => { modalConfirmRevoke.HideAsync(); }));

            await modalConfirmRevoke.ShowAsync<ConfirmRevokeComponent>(title: "Confirmation", parameters: detailsParameters);
        }

        // refresh the data from API by reload page
        protected void afterCloseRevoke()
        {
            OnHideModalClick();
            navManager.NavigateTo(navManager.Uri, true);
        }
    }
}
