using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteracting : MonoBehaviour
{
    [SerializeField] private float interactRange;
    [SerializeField] private int layerIndex;

    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, 1 << layerIndex);

        if (colliders.Length > 0)
        {
            UIManager.Instance.ShowWithinInteractRange();
        }
        else
            UIManager.Instance.HideWithinInteractRange();
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
