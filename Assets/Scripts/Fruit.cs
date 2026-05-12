using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    public FruitData Data { get; private set; }
    public int Level => Data != null ? Data.level : 0;

    [HideInInspector]
    public bool isMerging;

    // DangerZone이 이 과일을 무시해야 하는 동안 true (스폰 직후 유예 기간)
    public bool IsIgnoringDanger { get; private set; }

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
    }

    // 호출 즉시 IsIgnoringDanger = true, seconds 후 false
    public void SetDangerGrace(float seconds)
    {
        StopAllCoroutines();
        IsIgnoringDanger = true;
        StartCoroutine(ClearGrace(seconds));
    }

    IEnumerator ClearGrace(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsIgnoringDanger = false;
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

        // 수박(level 11)은 최종 단계 — 병합하지 않음
        if (Level >= 11)
            return;

        FruitMerger.Instance?.TryMerge(this, other);
    }
}
