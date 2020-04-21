using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public bool collected { get; private set; } = false;

    public string message;

    public void SetCollected(bool value)
    {
        collected = value;
    }

    public void PickUp()
    {
        collected = true;
        gameObject.SetActive(false);
    }
}
