using UnityEngine;

/*
 * PauseMenu.cs
 * 
 * Purpose: Controls game pause functionality and pause menu display
 * Used by: Game state management, UI system
 * 
 * Key Features:
 * - Toggle pause state with Escape key
 * - Cursor management
 * - Time scale control
 * - Menu visibility handling
 * 
 * Performance Considerations:
 * - Minimal update overhead
 * - Efficient state management
 * - Smart audio handling
 * 
 * Dependencies:
 * - UIAudioManager for sound effects
 * - Requires pause menu UI setup
 * - Time system integration
 */
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private static bool isPaused = false;

    // Check the pause state of the game
    public static bool IsGamePaused()
    {
        return isPaused;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Make the cursor invisible
        
        // Play menu toggle off sound
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayMenuToggle(false);
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pause game time
        isPaused = true;
        Cursor.lockState = CursorLockMode.None; // Free the cursor
        Cursor.visible = true; // Make the cursor visible
        
        // Play menu toggle on sound
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayMenuToggle(true);
        }
    }
}
