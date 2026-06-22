using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * EndGame.cs
 * 
 * Purpose: Handles game ending and scene reloading functionality
 * Used by: Menu system, game exit options
 * 
 * Key Features:
 * - Scene reloading functionality
 * - Main menu return option
 * 
 * Performance Considerations:
 * - Minimal overhead
 * - Simple scene management
 * 
 * Dependencies:
 * - Unity Scene Management system
 * - Requires proper scene setup
 */

public class EndGame : MonoBehaviour
{
    public void MainMenu()
    {
        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
