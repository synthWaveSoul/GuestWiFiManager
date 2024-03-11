namespace GuestWiFiManager.Components.Models
{
    public class StateObject
    {
        public bool isNewestDataFromMerakiLoaded { get; set; } = false;

        public void changeState() => isNewestDataFromMerakiLoaded = true;
    }
}
