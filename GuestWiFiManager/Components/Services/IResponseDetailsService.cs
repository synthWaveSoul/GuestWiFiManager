using GuestWiFiManager.Components.Models;

namespace GuestWiFiManager.Components.Services
{
    public interface IResponseDetailsService
    {
        Task<DataResponse> PythonAPIGetAccessDetails();

        Task<PutPythonApiResponseDetails> PythonAPISetAccess(string name, string duration, string userID);

        Task<DeletePythonApiResponseDetails> PythonAPIRevokeAccess(string deleteMerakiEmailId);
    }
}
