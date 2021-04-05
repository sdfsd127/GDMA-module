using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteracting : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey;
    [SerializeField] private float interactRange;
    private int interactLayer;

    private bool isInRangeOfInteractable;
    private Collider currentInteractableCollider;

    private void Start()
    {
        interactLayer = 9;
    }

    private void Update()
    {
        CheckWithinRange();

        if (Input.GetKeyUp(interactKey) && isInRangeOfInteractable)
        {
            currentInteractableCollider.GetComponent<InteractableBuilding>().PrintHello();
        }
    }

    private void CheckWithinRange()
    {
        if (IsWithinRangeOfInteractable())
            TogglePlayerWithinRange(true);
        else
            TogglePlayerWithinRange(false);
    }

    private void TogglePlayerWithinRange(bool withinRange)
    {
        if (withinRange)
        {
            isInRangeOfInteractable = true;
            UIManager.Instance.ShowWithinInteractRange();
        }
        else
        {
            isInRangeOfInteractable = false;
            UIManager.Instance.HideWithinInteractRange();
        }
    }

    //
    // TOOLS
    //
    private Collider[] GetInteractablesInRadius(float radius)
    {
        return Physics.OverlapSphere(transform.position, radius, 1 << interactLayer);
    }

    private bool IsWithinRangeOfInteractable()
    {
        Collider[] colliders = GetInteractablesInRadius(interactRange);

        if (colliders != null && colliders.Length > 0)
        {
            // Check if 1 or more interactables
            if (colliders.Length > 1)
            {
                // If more than 1, find the one the player is looking at
                RaycastHit hit;
                Ray ray = new Ray(transform.position, transform.forward);

                // Shoot ray from player POV forward to see what they are looking at
                if (Physics.Raycast(ray, out hit, interactRange, 1 << interactLayer)) // If it is an interactable building then set it to the current iteraction
                {
                    currentInteractableCollider = hit.collider;
                }
                else // Otherwise find the closest collider to the player and choose that one
                {
                    currentInteractableCollider = GetClosestCollider(transform.position, colliders);
                }
            }
            else
                currentInteractableCollider = colliders[0];

            return true;
        }
        else
            return false;
    }

    private Collider GetClosestCollider(Vector3 target, Collider[] colliders)
    {
        int closestColliderIndex = -1;
        float lowestDist = float.MaxValue;

        for (int i = 0; i < colliders.Length; i++)
        {
            float curDist = Vector3DistanceExcludeY(target, colliders[i].transform.position);

            if (curDist < lowestDist)
            {
                lowestDist = curDist;
                closestColliderIndex = i;
            }
        }

        return colliders[closestColliderIndex];
    }

    private float Vector3DistanceExcludeY(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    //
    // GIZMOS
    //
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
