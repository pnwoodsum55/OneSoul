using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public List<MessageController> messageControllers;

    // Update is called once per frame
    public void Play()
    {
        StartCoroutine(MessageCoroutine());
    }

    private IEnumerator MessageCoroutine()
    {
        for (int i = 0; i < messageControllers.Count; i++)
        {
            if (i > 0)
            {
                messageControllers[i - 1].FadeOut(1.0f);
                yield return new WaitForSeconds(1.0f);
            }

            messageControllers[i].FadeIn(1.0f);

            yield return new WaitForSeconds(messageControllers[i].holdTime + 2.0f);
        }

        GameManager.p_instance.SetCurrentState(GameManager.GameState.playing);
    }

    public void Stop()
    {
        StopAllCoroutines();
        for (int i = 0; i < messageControllers.Count; i++)
        {
            messageControllers[i].Stop();
            messageControllers[i].gameObject.SetActive(false);
        }
    }
}
