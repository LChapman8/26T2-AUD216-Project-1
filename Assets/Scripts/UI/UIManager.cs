using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this line
using TMPro;
using DG.Tweening; // Ensure DOTween is referenced
using System.Collections; // Add this line

/*
 * UIManager.cs
 * 
 * Purpose: Central manager for all UI interactions and displays
 * Used by: Game systems requiring UI interaction
 * 
 * Key Features:
 * - Interaction prompt system
 * - Dynamic UI positioning
 * - Smooth fade transitions
 * - Scene-persistent UI management
 * 
 * Performance Considerations:
 * - Efficient canvas management
 * - Optimized UI updates
 * - Smart component caching
 * - Scene transition handling
 * 
 * Dependencies:
 * - DOTween for animations
 * - TextMeshPro
 * - Unity UI system
 * - Scene management system
 */

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction Prompt UI")]
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private GameObject interactionPromptPanel;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private CanvasGroup interactionCanvasGroup;

    [Header("Fading Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("UI Positioning")]
    [SerializeField] private Vector2 promptPosition = new Vector2(-20, -20);
    [SerializeField] private Vector2 promptSize = new Vector2(400, 60);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Ensure this GameObject is at the root of the hierarchy
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (interactionCanvas == null)
        {
            CreateInteractionCanvas();
        }

        if (interactionPromptPanel == null)
        {
            CreateInteractionPromptPanel();
        }
        else
        {
            // Ensure the panel has a RectTransform
            if (interactionPromptPanel.GetComponent<RectTransform>() == null)
            {
                interactionPromptPanel.AddComponent<RectTransform>();
            }
        }

        EnsureComponentsAssigned();
        SetupPanelTransform(interactionPromptPanel.GetComponent<RectTransform>());
        InitializeUIState();
    }

    private void CreateInteractionCanvas()
    {
        GameObject canvasObject = new GameObject("InteractionUICanvas");
        interactionCanvas = canvasObject.AddComponent<Canvas>();
        interactionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        interactionCanvas.sortingOrder = 100;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObject);
    }

    private void CreateInteractionPromptPanel()
    {
        interactionPromptPanel = new GameObject("InteractionPromptPanel", typeof(RectTransform));
        interactionPromptPanel.transform.SetParent(interactionCanvas.transform, false);
        interactionCanvasGroup = interactionPromptPanel.AddComponent<CanvasGroup>();

        GameObject textObject = new GameObject("PromptText", typeof(RectTransform));
        textObject.transform.SetParent(interactionPromptPanel.transform, false);
        interactionPromptText = textObject.AddComponent<TextMeshProUGUI>();
        SetupTextProperties(interactionPromptText);

        RectTransform textRectTransform = interactionPromptText.GetComponent<RectTransform>();
        SetupTextTransform(textRectTransform);
    }

    private void EnsureComponentsAssigned()
    {
        if (interactionCanvasGroup == null)
        {
            interactionCanvasGroup = interactionPromptPanel.GetComponent<CanvasGroup>();
            if (interactionCanvasGroup == null)
            {
                interactionCanvasGroup = interactionPromptPanel.AddComponent<CanvasGroup>();
            }
        }

        if (interactionPromptText == null)
        {
            interactionPromptText = interactionPromptPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void InitializeUIState()
    {
        interactionCanvasGroup.alpha = 0f;
        interactionPromptPanel.SetActive(false);
    }

    private void SetupPanelTransform(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
           // Debug.LogError("RectTransform is null in SetupPanelTransform. Adding a new RectTransform component.");
            rectTransform = interactionPromptPanel.AddComponent<RectTransform>();
        }

        rectTransform.anchorMin = Vector2.one;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = Vector2.one;
        rectTransform.anchoredPosition = new Vector2(-20, -20);
        rectTransform.sizeDelta = promptSize;
    }

    private void SetupTextTransform(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private void SetupTextProperties(TextMeshProUGUI text)
    {
        text.alignment = TextAlignmentOptions.Right;
        text.fontSize = 20;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
    }

    public void DisplayPrompt(string promptText)
    {
        if (interactionPromptPanel != null && interactionPromptText != null && interactionCanvasGroup != null)
        {
            interactionPromptText.text = promptText;
            interactionPromptPanel.SetActive(true);
            
            RectTransform rectTransform = interactionPromptPanel.GetComponent<RectTransform>();
            SetupPanelTransform(rectTransform);

            interactionCanvasGroup.alpha = 0f;
            interactionCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
            
            StartCoroutine(AdjustTextSize());
        }
    }

    private IEnumerator AdjustTextSize()
    {
        yield return new WaitForEndOfFrame();
        
        if (interactionPromptText != null)
        {
            while (interactionPromptText.isTextOverflowing && interactionPromptText.fontSize > 12)
            {
                interactionPromptText.fontSize--;
                yield return null;
            }
        }
    }

    public void HidePrompt()
    {
        if (interactionPromptPanel != null && interactionCanvasGroup != null)
        {
            interactionCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    interactionPromptPanel.SetActive(false);
                });
        }
    }

    public void DisplayFishingPrompt(string promptText)
    {
        if (interactionPromptPanel != null && interactionPromptText != null && interactionCanvasGroup != null)
        {
            interactionPromptText.text = promptText;
            interactionPromptText.fontSize = 90f; // Set fixed size for fishing prompts
            interactionPromptPanel.SetActive(true);
            
            RectTransform rectTransform = interactionPromptPanel.GetComponent<RectTransform>();
            SetupPanelTransform(rectTransform);

            interactionCanvasGroup.alpha = 0f;
            interactionCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
            
            // Don't call AdjustTextSize for fishing prompts
        }
    }

    public void DisplayInteractionPrompt(string promptText)
    {
        if (interactionPromptPanel != null && interactionPromptText != null && interactionCanvasGroup != null)
        {
            interactionPromptText.text = promptText;
            interactionPromptText.fontSize = 90f; // Same size as fishing prompts
            interactionPromptPanel.SetActive(true);
            
            RectTransform rectTransform = interactionPromptPanel.GetComponent<RectTransform>();
            SetupPanelTransform(rectTransform);

            interactionCanvasGroup.alpha = 0f;
            interactionCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
            
            // Don't call AdjustTextSize for interaction prompts
        }
    }

    private void Update()
    {
        ForceUpdateUIPosition();
    }

    private void ForceUpdateUIPosition()
    {
        if (interactionPromptPanel != null)
        {
            RectTransform rectTransform = interactionPromptPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                SetupPanelTransform(rectTransform);
            }
        }
    }
}