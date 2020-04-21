using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public Animator doorOpenAnimator;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.name == "Player") {
            doorOpenAnimator.Play("door_open",-1,0);
        }

        if (GameManager.p_instance.collectibleManager.currentlyCollected >= 4)
        {
            GameManager.p_instance.DisableBadEnding();
        }
        gameObject.SetActive(false);
    }
  
}
