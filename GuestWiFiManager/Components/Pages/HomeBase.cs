using BlazorBootstrap;
using GuestWiFiManager.Components.Models;
using GuestWiFiManager.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GuestWiFiManager.Components.Pages
{
    public class HomeBase : ComponentBase
    {
        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        public Modal modal = default!;

        protected async Task OnShowModalClick()
        {
            await modal.ShowAsync();
        }

        protected async Task OnHideModalClick()
        {
            await modal.HideAsync();
        }

        // error state from python api
        public bool isErrorAnywhere = false;

        // error details from python api
        public string errorMessage;

        [Inject]
        protected IResponseDetailsService responseDetailsService { get; set; }

        [Inject]
        protected GetPythonApiResponseDetails responseDetails { get; set; }

        protected async Task GetDataFromPythonApi()
        {
            try
            {
                PreloadService.Show(SpinnerColor.Light, "Loading data, please wait ...");
                responseDetails.dataResponse = await responseDetailsService.PythonAPIGetAccessDetails();
            }
            catch (Exception ex)
            {
                isErrorAnywhere = true;
                errorMessage = ex.Message;
            }
            finally
            {
                PreloadService.Hide();
            }
        }

        [Inject]
        protected StateObject stateObject { get; set; }

        protected void changeState()
        {
            stateObject.changeState();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (stateObject.isNewestDataFromMerakiLoaded == false)
                {
                    await GetDataFromPythonApi();
                    changeState();
                }
                StateHasChanged();
            }
        }

        protected string submitName;

        // hardcoded "1" is the default value
        protected string submitDays = "1";

        //hardcoded username due to the lack of logging implementation yet
        protected string userId = "CK01";

        protected PutPythonApiResponseDetails putPythonApiResponseDetails { get; set; }

        protected async Task setAccess()
        {
            try
            {
                PreloadService.Show(SpinnerColor.Light, "Authorising access, please wait...");
                putPythonApiResponseDetails = await responseDetailsService.PythonAPISetAccess(submitName, submitDays, userId);
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

        [Inject]
        NavigationManager navManager { get; set; }

        protected void afterCloseNewAccess()
        {
            OnHideModalClick();
            navManager.NavigateTo(navManager.Uri, true);
        }
    }
}
