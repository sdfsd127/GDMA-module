using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBuilding : MonoBehaviour
{
    public void Interact()
    {
        GameManager.Instance.PlayRandomMinigame();
    }
}
