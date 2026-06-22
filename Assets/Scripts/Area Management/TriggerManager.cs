using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    public AmbienceManager.AmbienceState targetState;
    private AmbienceManager ambienceManager;
    private MusicManager musicManager;

    // Helper property to determine if the ambience state should use indoor music
    private bool ShouldUseIndoorMusic => targetState == AmbienceManager.AmbienceState.Cave;

    void Start()
    {
        ambienceManager = FindFirstObjectByType<AmbienceManager>();
        musicManager = FindFirstObjectByType<MusicManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle ambience state change
            if (ambienceManager != null)
            {
                ambienceManager.ChangeState(targetState);
            }

            // Handle music state change
            if (musicManager != null)
            {
                bool isIndoor = ShouldUseIndoorMusic;
                musicManager.SetIndoorState(isIndoor);
                
                // If we're going indoors, explicitly change to Inside music state
                if (isIndoor)
                {
                    musicManager.ChangeState(MusicManager.MusicState.Inside);
                }
                else
                {
                    musicManager.ChangeState(MusicManager.MusicState.Exploring);
                }
            }
        }
    }
}
