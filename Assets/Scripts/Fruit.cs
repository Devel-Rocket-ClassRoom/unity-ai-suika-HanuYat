using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    public FruitData Data { get; private set; }
    public int Level => Data != null ? Data.level : 0;

    [HideInInspector]
    public bool isMerging;

    // 스폰 직후 물리 충돌이 즉시 발생하는 오탐 방지 (GDD §10)
    const float collisionIgnoreSeconds = 0.05f;
    float spawnTime;

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    public void Init(FruitData data)
    {
        Data = data;

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = data.sprite;
        sr.color = data.color;

        // Knob 스프라이트는 64×64 at PPU 100 → 1 world unit.
        // localScale = diameter = radius * 2 로 설정하면
        // CircleCollider2D(radius=0.5) 의 world 반지름 = 0.5 * scale = data.radius 가 됨.
        transform.localScale = Vector3.one * (data.radius * 2f);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isMerging)
            return;
        if (Time.time - spawnTime < collisionIgnoreSeconds)
            return;

        var other = col.gameObject.GetComponent<Fruit>();
        if (other == null || other.isMerging)
            return;
        if (other.Level != Level)
            return;

        // InstanceID가 더 큰 쪽만 TryMerge 를 호출해 이중 트리거 방지
        if (GetInstanceID() < other.GetInstanceID())
            return;

        FruitMerger.Instance?.TryMerge(this, other);
    }
}
