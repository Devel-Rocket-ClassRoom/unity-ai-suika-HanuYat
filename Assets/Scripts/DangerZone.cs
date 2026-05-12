using System.Collections.Generic;
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    [SerializeField] float gameOverDelay = 3f;

    readonly HashSet<Collider2D> fruitsAboveLine = new();
    float dangerTimer;

    void Update()
    {
        fruitsAboveLine.RemoveWhere(c => c == null);

        if (fruitsAboveLine.Count > 0)
        {
            dangerTimer += Time.deltaTime;
            if (dangerTimer >= gameOverDelay)
                GameManager.Instance?.GameOver();
        }
        else
        {
            dangerTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fruit"))
            fruitsAboveLine.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        fruitsAboveLine.Remove(other);
    }
}
