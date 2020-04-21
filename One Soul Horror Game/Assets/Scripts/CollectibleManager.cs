using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    private const float ENTRY_SPACING = 35.0f;

    public Collectible[] collectibles { get; private set; }
    private int totalCollectibles;
    public int currentlyCollected { get; private set; } = 0;
    public List<GameObject> journalEntries;

    public void OnStart()
    {
        foreach (Collectible c in collectibles)
        {
            c.gameObject.SetActive(true);
            c.SetCollected(false);
        }
        currentlyCollected = 0;

        foreach (GameObject je in journalEntries)
        {
            je.GetComponent<JournalEntry>().SetText("");

            je.SetActive(false);
        }
    }

    private void Start()
    {
        collectibles = GetComponentsInChildren<Collectible>();
        totalCollectibles = collectibles.Length;
        foreach (Collectible c in collectibles)
        {
            c.gameObject.SetActive(true);
        }
    }

    public void PickUp(Collectible collectible)
    {
        if (!collectible.gameObject.activeSelf)
        {
            return;
        } else if (collectible.collected)
        {
            collectible.gameObject.SetActive(false);
            return;
        }

        collectible.PickUp();

        currentlyCollected++;

        GameObject newEntry = journalEntries[currentlyCollected - 1];
        Debug.Log(newEntry.activeSelf);
        newEntry.SetActive(true);
        Debug.Log(newEntry.activeSelf);
        newEntry.GetComponent<JournalEntry>().SetText(collectible.message);

        // Space this journal entry based on the length of the previous journal entry
        if (currentlyCollected > 1)
        {
            GameObject previousEntry = journalEntries[currentlyCollected - 2];

            Bounds previousEntryBounds = previousEntry.GetComponent<JournalEntry>().textMesh.textBounds;

            float previousEntryY = previousEntry.transform.localPosition.y;

            Vector3 newEntryPosition = newEntry.transform.localPosition;

            newEntryPosition.y = previousEntryY  - previousEntryBounds.size.y - ENTRY_SPACING;

            newEntry.transform.localPosition = newEntryPosition;
        }
        Debug.Log(newEntry.activeSelf);
        if (currentlyCollected >= totalCollectibles)
        {
            GameManager.p_instance.DisableBadEnding();
        }
    }

    public static readonly string[] entries = {
        "You found a Bottle of Cologne. This fragrance could only belong to your friend Joe. You always insisted that the scent was too strong. And that it would never in a million years sway any girl he was courting. While you never used to be able to stand the smell, now it brings a smile to your face. It reminds you of your humanity.",
        "You found a Pair of Glasses. The purple rims prove that they belong to your co-worker Monica. She never went anywhere without her glasses, so it’s strange to find them here. One time she tripped and lost them. You helped her find them as she crawled around searching aimlessly. You never saw her outside of work, but she was always incredibly friendly and ready with a new joke. The memory of her restores your humanity.",
        "You found a Scotch Glass. It’s a vintage 19th century design. It belongs to your acquaintance Brian. You were never super close to him, but every Friday at 4pm he would invite you to his office for a glass of Scotch. You wish nothing more, than for him to be here offering you some right now. The memories with him bring back some humanity.",
        "You found a Novel. It’s 1984 by George Orwell. It belongs to your friend Sarah. There was rarely a conversation where she didn’t bring up the book at least once. She was always extremely critical about mass surveillance and revisionism. I always found that funny considering the morality of the corporation we work for. Your memories of her bring you closer to true humanity."
    };
}