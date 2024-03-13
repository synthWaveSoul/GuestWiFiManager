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

        protected async Task OnShowModalClick()
        {
            await modalAfterRevoke.ShowAsync();
        }

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

        protected async Task revokeAccess(string merakiEmailIdToRevoke)
        {
            try
            {
                PreloadService.Show(SpinnerColor.Light, "Loading data, please wait ...");
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
                OnShowModalClick();
            }
        }

        protected async void testRevoke(MouseEventArgs e)
        {
            try
            {
                PreloadService.Show(SpinnerColor.Light, "Loading data, please wait ...");
                //deleteResponseDetails = await responseDetailsService.PythonAPIRevokeAccess(id);
            }
            catch (Exception ex)
            {
                isErrorAnywhere = true;
                errorMessage = ex.Message;
            }
            finally
            {
                PreloadService.Hide();
                OnShowModalClick();
            }
        }

        //public void testRevoke22(MouseEventArgs e, string id) => testRevoke(e);

        protected async Task confirmRevokeAccess(string name, string id)
        {
            var detailsParameters = new Dictionary<string, object>();

            detailsParameters.Add("Name", name);
            //detailsParameters.Add("Id", id);
            detailsParameters.Add("revokeAccessConfirmed", EventCallback.Factory.Create<MouseEventArgs>(this, testRevoke));
            //detailsParameters.Add("revokeAccessConfirmed", Task.Factory.StartNew<MouseEventArgs>(this, revokeAccess(id)));
            //detailsParameters.Add("revokeAccessConfirmed", Task.Factory.StartNew(() => { revokeAccess(id) ; }));
            //detailsParameters.Add("revokeAccessConfirmed", Task.Factory.StartNew(revokeAccess(id)));

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
