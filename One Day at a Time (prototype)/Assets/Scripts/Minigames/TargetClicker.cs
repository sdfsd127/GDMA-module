using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetClicker : Minigame
{
    private const int MAX_TIME = 30;
    private const int MAX_TARGETS = 30;
    private int currentTargetsHit;

    [SerializeField] private Text targetsHitText;
    [SerializeField] private Text timeText;

    [SerializeField] private GameObject targetPrefab;
    private GameObject currentTargetObject;

    [SerializeField] private Vector2 upperLeftBound;
    [SerializeField] private Vector2 lowerRightBound;

    private void Start()
    {
        // Base Class
        m_MinigameName = "Target Clicker";
        m_DisplayedInformation = "This is Target Clicker.";
        m_MinigameEndCondition = MINIGAME_END_CONDITION.BEFORE_TIMER;

        InitMinigame(MAX_TIME);

        // This Class
        InitTargetClicker();
    }

    private void InitTargetClicker()
    {
        currentTargetsHit = 0;
        UpdateTargetUIText();
        SpawnNewTarget();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Get mouse world position
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            // Setup raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform raycast on clicked position
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.name == "Target(Clone)")
                {
                    ClickedTarget();
                }
            }
        }

        UpdateMinigame();
        UpdateTimeText();
    }

    private void ClickedTarget()
    {
        currentTargetsHit++;
        UpdateTargetUIText();

        if (currentTargetsHit < MAX_TARGETS)
        {
            RemoveCurrentTarget();
            SpawnNewTarget();
        }
        else
            MinigameWon();
    }

    private void RemoveCurrentTarget()
    {
        Destroy(currentTargetObject);
        currentTargetObject = null;
    }

    private void SpawnNewTarget()
    {
        GameObject newTargetObject = Instantiate(targetPrefab, GetRandomSpawnPoint(), Quaternion.identity);
        newTargetObject.GetComponent<SpriteRenderer>().color = new Color(GetRandomUnitFloat(), GetRandomUnitFloat(), GetRandomUnitFloat(), 1.0f);

        currentTargetObject = newTargetObject;
    }

    private void UpdateTargetUIText()
    {
        targetsHitText.text = currentTargetsHit + " / " + MAX_TARGETS;
    }

    private void UpdateTimeText()
    {
        timeText.text = ((float)MAX_TIME - GetTimerElapsedTime()).ToString("0.0");
    }

    //
    // TOOLS
    //
    private Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(upperLeftBound.x, lowerRightBound.x), Random.Range(lowerRightBound.y, upperLeftBound.y), 0);
    }

    private float GetRandomUnitFloat()
    {
        return ((float)Random.Range(0, 255) / 255);
    }
}
