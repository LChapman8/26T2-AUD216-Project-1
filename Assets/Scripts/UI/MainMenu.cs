using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * MainMenu.cs
 * 
 * Purpose: Handles main menu functionality and scene transitions
 * Used by: Game startup, menu system
 * 
 * Key Features:
 * - Game start functionality
 * - Application quit handling
 * - Scene management
 * - Platform-specific handling
 * 
 * Performance Considerations:
 * - Minimal overhead
 * - Clean scene transitions
 * 
 * Dependencies:
 * - Unity Scene Management system
 * - Requires proper scene setup
 * - Scene build index configuration
 */

public class MainMenu : MonoBehaviour
{
    public void PlayGame ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame ()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
