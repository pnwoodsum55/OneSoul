using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class JournalEntry : MonoBehaviour
{
    public TextMeshProUGUI textMesh { get; private set; }

    public void SetText(string journalText)
    {
        if (!textMesh) textMesh = GetComponentInChildren<TextMeshProUGUI>();

        textMesh.text = journalText;
    }
}
