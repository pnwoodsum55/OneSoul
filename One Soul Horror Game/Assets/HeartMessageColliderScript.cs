using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartMessageColliderScript : MonoBehaviour
{
    public MessageController message;

    public void ResetMessage()
    {
        message.FadeOut(0.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            message.FadeInOut(1.0f);
        }

        gameObject.SetActive(false);
    }
}
