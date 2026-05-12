using UnityEngine;

public class FruitMerger : MonoBehaviour
{
    public static FruitMerger Instance { get; private set; }

    [SerializeField]
    Transform fruitParent;

    [SerializeField]
    int watermelonBonus = 500;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TryMerge(Fruit a, Fruit b)
    {
        if (a == null || b == null)
            return;
        if (a.isMerging || b.isMerging)
            return;

        a.isMerging = true;
        b.isMerging = true;

        Vector3 mid = (a.transform.position + b.transform.position) * 0.5f;
        FruitData data = a.Data;

        // 수박+수박: 둘 다 제거 + 보너스 (GDD §2.2, §4.2)
        if (data.level >= 11)
        {
            // ScoreManager.Instance?.AddScore(watermelonBonus); // Phase 4
            Destroy(a.gameObject);
            Destroy(b.gameObject);
            return;
        }

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        FruitData next = data.nextFruit;
        if (next == null || next.prefab == null)
            return;

        var go = Instantiate(next.prefab, mid, Quaternion.identity, fruitParent);
        var fruit = go.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.Init(next);
            fruit.SetDangerGrace(1.5f); // 병합 직후 DangerZone 오탐 방지
        }
        // ScoreManager.Instance?.AddScore(next.mergeScore); // Phase 4
    }
}
