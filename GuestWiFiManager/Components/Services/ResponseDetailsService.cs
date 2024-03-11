using GuestWiFiManager.Components.Models;

namespace GuestWiFiManager.Components.Services
{
    public class ResponseDetailsService : IResponseDetailsService
    {
        private readonly HttpClient httpClient;

        public ResponseDetailsService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<GetPythonApiResponseDetails> PythonAPIGetAccessDetails()
        {
            // hardcoded username due to the lack of logging implementation yet
            return await httpClient.GetFromJsonAsync<GetPythonApiResponseDetails>("pythonapi/get/CK01");
        }

        public async Task<PutPythonApiResponseDetails> PythonAPISetAccess(string name, string duration, string userID)
        {
            return await httpClient.GetFromJsonAsync<PutPythonApiResponseDetails>("/pythonapi/put/name=" + name + "&duration=" + duration + "&userid=" + userID);
        }

        public async Task<DeletePythonApiResponseDetails> PythonAPIRevokeAccess(string deleteMerakiEmailId)
        {
            return await httpClient.GetFromJsonAsync<DeletePythonApiResponseDetails>("/pythonapi/delete/" + deleteMerakiEmailId);
        }
    }
}
