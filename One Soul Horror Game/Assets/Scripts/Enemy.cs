using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    const float CHASE_DURATION = 10.0f;
    const float GIVE_UP_TIME = 3.0f;
    const float PATH_SPEED = 2.5f;
    const float CHASE_SPEED = 2.5f;
    const float DETECT_RADIUS = 5.0f;
    const float MIN_DISTANCE = 0.2f;

    public List<Node> nodes;
    public Node previousNode { get; private set; }
    private int nodeIndex = 0;
    public Node targetNode { get; private set; }
    private bool chasing = false;
    private Vector3 previousPosition;
    private bool moving = false;

    public void Init()
    {
        moving = true;
        transform.position = nodes[0].transform.position;
        previousNode = nodes[0];
        nodeIndex = 0;
        targetNode = nodes[(nodeIndex + 1) % nodes.Count];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            other.GetComponent<Player>().EnemyCollision(transform);
            moving = false;
        }
    }

    private void Move()
    {
        Vector3 direction = Vector3.Normalize(targetNode.transform.position - previousNode.transform.position);

        if (chasing)
        {
            transform.position += direction * CHASE_SPEED * Time.deltaTime;
        } else
        {
            transform.position += direction * PATH_SPEED * Time.deltaTime;
        }
    }

    public void OnUpdate()
    {
        if (!moving)
        {
            return;
        }
        //Debug.Log("Distance to Player: " + Vector3.Distance(transform.position, GameManager.p_instance.player.transform.position));
        float distance = Vector3.Distance(transform.position, targetNode.gameObject.transform.position);

        if (distance < MIN_DISTANCE)
        {
            transform.position = targetNode.transform.position;
            previousNode = targetNode;
            nodeIndex = (nodeIndex + 1) % nodes.Count;
            targetNode = nodes[(nodeIndex + 1) % nodes.Count];
        }

        if (Vector3.Distance(transform.position, GameManager.p_instance.player.transform.position) < DETECT_RADIUS)
        {
            //Debug.Log("Player Detected");
        }

        Move();
    }
}