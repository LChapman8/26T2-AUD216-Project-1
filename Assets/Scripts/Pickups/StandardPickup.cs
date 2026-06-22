public class StandardPickup : PickupBase
{
    public override void PlaySound()
    {
        PickupsAudioManager.Instance?.PlayPickupSound();
    }
}