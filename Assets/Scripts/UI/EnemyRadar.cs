using UnityEngine;
using UnityEngine.UI;

public class EnemyRadar : MonoBehaviour
{
    [Header("References")]
    public Transform player;                    
    public RectTransform radarCanvas;          
    public RectTransform compassBar;           // This will be our parent for indicators
    public GameObject enemyIndicatorPrefab;     
    
    [Header("Settings")]
    public float maxDistance = 50f;            
    public float compassBarWidth;              
    
    private GameObject[] enemies;               
    private GameObject[] indicators;            
    private int lastEnemyCount = 0;    

    void Start()
    {
        if (compassBarWidth == 0)
            compassBarWidth = compassBar.rect.width;

        RefreshEnemies();
        lastEnemyCount = enemies.Length;
    }

    void LateUpdate()
    {
        // Check if enemy count has changed
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemyCount != lastEnemyCount)
        {
            RefreshEnemies();
            lastEnemyCount = currentEnemyCount;
            return;
        }

        // Update indicators and check for destroyed or dead enemies
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null && indicators[i] != null)
            {
                // Enemy was destroyed, remove its indicator
                Destroy(indicators[i]);
                indicators[i] = null;
                continue;
            }

            if (enemies[i] != null && indicators[i] != null)
            {
                // Check if enemy is dead - if so, destroy indicator
                ZombieAI zombieAI = GetZombieAI(enemies[i]);
                
                // If we found the component and enemy is dead, remove indicator
                if (zombieAI != null && zombieAI.IsDead())
                {
                    Destroy(indicators[i]);
                    indicators[i] = null;
                    continue;
                }

                // Update indicator position for living enemies
                UpdateIndicatorPosition(enemies[i], indicators[i]);
            }
        }
    }

    void RefreshEnemies()
    {
        if (compassBar == null)
        {
            Debug.LogError("Compass Bar is not assigned!");
            return;
        }

        // Clean up existing indicators
        if (indicators != null)
        {
            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] != null)
                {
                    Destroy(indicators[i]);
                }
            }
        }

        // Find enemies
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        System.Collections.Generic.List<GameObject> enemyList = new System.Collections.Generic.List<GameObject>();
    
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == enemyLayer && obj != null)
            {
                // Exclude dead enemies from radar
                ZombieAI zombieAI = GetZombieAI(obj);
                if (zombieAI != null && zombieAI.IsDead())
                {
                    continue;
                }
                enemyList.Add(obj);
            }
        }
    
        enemies = enemyList.ToArray();
        
        // Create new indicators under the compass bar
        indicators = new GameObject[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            indicators[i] = Instantiate(enemyIndicatorPrefab, compassBar);
            indicators[i].SetActive(true);
        }
    }

    void UpdateIndicatorPosition(GameObject enemy, GameObject indicator)
    {
        // Safety check: if enemy is dead, hide indicator immediately
        ZombieAI zombieAI = GetZombieAI(enemy);
        if (zombieAI != null && zombieAI.IsDead())
        {
            indicator.SetActive(false);
            return;
        }

        // Get direction to enemy
        Vector3 directionToEnemy = enemy.transform.position - player.position;
        float distanceToEnemy = directionToEnemy.magnitude;

        // Only show indicator if enemy is within range
        if (distanceToEnemy <= maxDistance)
        {
            indicator.SetActive(true);

            // Calculate angle between forward and enemy direction
            Vector3 forwardVector = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
            Vector3 enemyVector = Vector3.ProjectOnPlane(directionToEnemy, Vector3.up).normalized;
            
            // Get the angle between player's forward and direction to enemy
            float angleToEnemy = Vector3.SignedAngle(forwardVector, enemyVector, Vector3.up);
            float indicatorPosition = (angleToEnemy / 180f) * compassBarWidth;

            // Clamp to edges when beyond ±58 degrees
            if (angleToEnemy > 58f)
            {
                indicatorPosition = compassBarWidth * 0.322f; // Adjusted for 58 degrees (58/180)
            }
            else if (angleToEnemy < -58f)
            {
                indicatorPosition = -compassBarWidth * 0.322f; // Adjusted for -58 degrees (-58/180)
            }

            // Update indicator position - keep the original Y position
            RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
            float currentY = indicatorRect.anchoredPosition.y;
            indicatorRect.anchoredPosition = new Vector2(indicatorPosition, currentY);

            // Optional: Scale indicator based on distance
            float scale = 1 - (distanceToEnemy / maxDistance);
            indicatorRect.localScale = new Vector3(scale, scale, 1);
        }
        else
        {
            indicator.SetActive(false);
        }
    }

    // Helper method to find ZombieAI component on an enemy GameObject
    private ZombieAI GetZombieAI(GameObject enemy)
    {
        if (enemy == null) return null;
        
        ZombieAI zombieAI = enemy.GetComponent<ZombieAI>();
        if (zombieAI != null) return zombieAI;
        
        zombieAI = enemy.GetComponentInChildren<ZombieAI>();
        if (zombieAI != null) return zombieAI;
        
        zombieAI = enemy.GetComponentInParent<ZombieAI>();
        return zombieAI;
    }

    // Call this method when new enemies spawn or die
    public void OnEnemiesChanged()
    {
        RefreshEnemies();
    }
}