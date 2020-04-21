using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimaxLoopScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            GameManager.p_instance.climaxLoop.Play(0.0f, true);
            GameManager.p_instance.climaxLoop.StartFade(0, 1, 0.05f);
        }
    }
}
