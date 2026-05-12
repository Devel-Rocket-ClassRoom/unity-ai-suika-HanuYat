using System.Collections.Generic;
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    [SerializeField]
    float gameOverDelay = 3f;

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

    // Enter 대신 Stay를 사용: 유예 기간이 끝난 과일도 다음 프레임에 자동으로 포착
    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Fruit"))
            return;
        var fruit = other.GetComponent<Fruit>();
        if (fruit != null && fruit.IsIgnoringDanger)
            return;
        fruitsAboveLine.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        fruitsAboveLine.Remove(other);
    }
}
