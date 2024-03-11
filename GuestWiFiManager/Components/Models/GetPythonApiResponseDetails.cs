namespace GuestWiFiManager.Components.Models
{
    public class GetPythonApiResponseDetails
    {
        public DataResponse dataResponse { get; set; }
    }

    public class DataResponse
    {
        public string error { get; set; }
        public int quantity { get; set; }
        public string fullName { get; set; }
        public AccountsHistory[] accountsHistoryDetails { get; set; }
    }

    public class AccountsHistory
    {
        public string name { get; set; }
        public string merakiEmailId { get; set; }
        public string merakiEmailLogin { get; set; }
        public string isActive { get; set; }
        public string expires { get; set; }
        public string createdAt { get; set; }
        public string password { get; set; }
    }
}
