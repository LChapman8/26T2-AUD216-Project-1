using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [System.Serializable]
    public struct TeleportShortcut
    {
        public GameObject destination;
        public KeyCode key;
        public KeyCode modifier;
    }

    [Tooltip("Configure teleport destinations and their keyboard shortcuts")]
    public TeleportShortcut[] shortcuts = new TeleportShortcut[5];

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        foreach (var shortcut in shortcuts)
        {
            if (shortcut.destination != null && 
                Input.GetKey(shortcut.modifier) && 
                Input.GetKeyDown(shortcut.key))
            {
                TeleportTo(shortcut.destination);
                break;
            }
        }
    }

    void TeleportTo(GameObject sphere)
    {
        if (sphere != null)
        {
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.position = sphere.transform.position;

            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }
    }
}
