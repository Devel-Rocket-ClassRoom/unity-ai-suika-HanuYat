using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [SerializeField]
    FruitData[] droppableData; // 레벨 1~5 (체리~감)

    [SerializeField]
    Transform fruitParent;

    [SerializeField]
    SpriteRenderer previewRenderer; // 자식 Preview 오브젝트의 SpriteRenderer

    [SerializeField]
    float spawnY = 4.5f;

    [SerializeField]
    float wallInnerX = 3.4f;

    [SerializeField]
    float dropCooldown = 0.5f;

    FruitData currentData;
    FruitData nextData;
    float lastDropTime = -999f;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        currentData = PickRandom();
        nextData = PickRandom();
        ApplyPreview();
        // UIManager.Instance?.UpdateNextFruit(nextData); // Phase 7
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;
        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;
        if (currentData == null)
            return;

        // 마우스 월드 X 좌표로 스폰 위치 이동, 벽 안쪽으로 클램프 (GDD §2.1, §2.3)
        Vector3 mw = cam.ScreenToWorldPoint(Input.mousePosition);
        float half = wallInnerX - currentData.radius;
        float x = Mathf.Clamp(mw.x, -half, half);
        transform.position = new Vector3(x, spawnY, 0f);

        if (Input.GetMouseButtonDown(0) && Time.time - lastDropTime >= dropCooldown)
            Drop();
    }

    void Drop()
    {
        var go = Instantiate(
            currentData.prefab,
            transform.position,
            Quaternion.identity,
            fruitParent
        );
        var fruit = go.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.Init(currentData);
            fruit.SetDangerGrace(1.0f); // y=4.5에서 낙하 중 DangerZone 오탐 방지
        }

        lastDropTime = Time.time;
        currentData = nextData;
        nextData = PickRandom();
        ApplyPreview();
        // UIManager.Instance?.UpdateNextFruit(nextData); // Phase 7
    }

    void ApplyPreview()
    {
        if (previewRenderer == null || currentData == null)
            return;
        previewRenderer.sprite = currentData.sprite;
        previewRenderer.color = new Color(
            currentData.color.r,
            currentData.color.g,
            currentData.color.b,
            0.5f
        );
        // Fruit.Init()과 동일한 sprite bounds 기반 스케일로 미리보기 크기 일치
        float spriteSize = (currentData.sprite != null) ? currentData.sprite.bounds.size.x : 1f;
        float scale = (currentData.radius * 2f) / spriteSize;
        previewRenderer.transform.localScale = Vector3.one * scale;
    }

    FruitData PickRandom() => droppableData[Random.Range(0, droppableData.Length)];
}
