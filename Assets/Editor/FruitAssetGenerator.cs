using UnityEditor;
using UnityEngine;

/// <summary>
/// Suika > Generate Fruit Assets 메뉴로 실행.
/// FruitData 11종 + Prefab 11종을 일괄 생성하고 nextFruit/prefab 체인을 연결한다.
/// </summary>
public static class FruitAssetGenerator
{
    static readonly (string name, float radius, int score, Color color)[] FruitTable =
    {
        ("Cherry", 0.30f, 1, new Color(0.85f, 0.07f, 0.07f)),
        ("Strawberry", 0.40f, 3, new Color(0.98f, 0.40f, 0.55f)),
        ("Grape", 0.50f, 6, new Color(0.50f, 0.15f, 0.70f)),
        ("Dekopon", 0.62f, 10, new Color(1.00f, 0.55f, 0.00f)),
        ("Persimmon", 0.75f, 15, new Color(0.85f, 0.40f, 0.00f)),
        ("Apple", 0.90f, 21, new Color(1.00f, 0.10f, 0.10f)),
        ("Pear", 1.10f, 28, new Color(0.75f, 0.90f, 0.20f)),
        ("Peach", 1.25f, 36, new Color(1.00f, 0.75f, 0.60f)),
        ("Pineapple", 1.45f, 45, new Color(1.00f, 0.85f, 0.00f)),
        ("Melon", 1.65f, 55, new Color(0.45f, 0.85f, 0.35f)),
        ("Watermelon", 1.90f, 66, new Color(0.15f, 0.65f, 0.15f)),
    };

    const string DataFolder = "Assets/Data/FruitData";
    const string PrefabFolder = "Assets/Prefabs";
    const string MaterialPath = "Assets/Materials/FruitPhysics.physicsMaterial2D";

    [MenuItem("Suika/Generate Fruit Assets")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(DataFolder))
            AssetDatabase.CreateFolder("Assets/Data", "FruitData");
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
        if (mat == null)
            Debug.LogWarning(
                $"[FruitAssetGenerator] FruitPhysics material not found at {MaterialPath}. Colliders will have no physics material."
            );

        // 내장 Knob 스프라이트 (원형 플레이스홀더)
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        int count = FruitTable.Length;
        var dataAssets = new FruitData[count];
        var prefabs = new GameObject[count];

        // 1단계: FruitData 에셋 생성
        for (int i = 0; i < count; i++)
        {
            var (fruitName, radius, score, color) = FruitTable[i];
            string assetPath = $"{DataFolder}/{fruitName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<FruitData>(assetPath);
            FruitData fd =
                existing != null ? existing : ScriptableObject.CreateInstance<FruitData>();

            fd.level = i + 1;
            fd.fruitName = fruitName;
            fd.sprite = knob;
            fd.color = color;
            fd.radius = radius;
            fd.mergeScore = score;

            if (existing == null)
                AssetDatabase.CreateAsset(fd, assetPath);
            else
                EditorUtility.SetDirty(fd);

            dataAssets[i] = fd;
        }

        // 2단계: Prefab 생성
        for (int i = 0; i < count; i++)
        {
            string prefabPath = $"{PrefabFolder}/{FruitTable[i].name}.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject go = new GameObject(FruitTable[i].name);
            go.tag = "Fruit";
            go.layer = 8; // Fruit layer (TagManager.asset line 17)

            // Unity GetComponent 결과에 ?? 를 쓰면 fake-null을 잡지 못하므로 명시적 체크
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = dataAssets[i].sprite;
            sr.color = dataAssets[i].color;

            // Rigidbody2D (GDD §7.2)
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.linearDamping = 0.5f;

            // CircleCollider2D
            var col = go.GetComponent<CircleCollider2D>();
            if (col == null)
                col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f; // localScale이 radius*2 라 world 반지름 = data.radius
            if (mat != null)
                col.sharedMaterial = mat;

            // Fruit 스크립트 (RequireComponent가 있어 SR/RB/CC는 이미 위에서 추가됨)
            if (go.GetComponent<Fruit>() == null)
                go.AddComponent<Fruit>();

            // localScale: 디자인타임 가시성 (Init()에서도 재적용됨)
            go.transform.localScale = Vector3.one * (FruitTable[i].radius * 2f);

            if (existingPrefab != null)
            {
                // 기존 프리팹은 덮어쓰기
                prefabs[i] = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            else
            {
                prefabs[i] = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            Object.DestroyImmediate(go);
        }

        // 3단계: FruitData 에 prefab + nextFruit 연결
        for (int i = 0; i < count; i++)
        {
            dataAssets[i].prefab = prefabs[i];
            dataAssets[i].nextFruit = i + 1 < count ? dataAssets[i + 1] : null;
            EditorUtility.SetDirty(dataAssets[i]);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FruitAssetGenerator] {count}종 FruitData + Prefab 생성 완료.");
    }

    /// <summary>
    /// 씬의 FruitMerger / FruitSpawner 컴포넌트에 레퍼런스를 자동 연결한다.
    /// FruitData 에셋과 Preview SpriteRenderer 포함.
    /// </summary>
    [MenuItem("Suika/Wire Scene References")]
    public static void WireScene()
    {
        var fruitParent = GameObject.Find("FruitParent");
        var mergerGO = GameObject.Find("FruitMerger");
        var spawnerGO = GameObject.Find("FruitSpawner");
        var previewGO = GameObject.Find("Preview");

        if (fruitParent == null || mergerGO == null || spawnerGO == null || previewGO == null)
        {
            Debug.LogError(
                "[WireScene] 씬에서 필요한 오브젝트를 찾지 못했습니다. 씬 구조를 확인하세요."
            );
            return;
        }

        // FruitMerger 레퍼런스
        var merger = mergerGO.GetComponent<FruitMerger>();
        if (merger != null)
        {
            var so = new UnityEditor.SerializedObject(merger);
            so.FindProperty("fruitParent").objectReferenceValue = fruitParent.transform;
            so.ApplyModifiedProperties();
        }

        // FruitSpawner 레퍼런스
        var spawner = spawnerGO.GetComponent<FruitSpawner>();
        if (spawner != null)
        {
            var so = new UnityEditor.SerializedObject(spawner);

            so.FindProperty("fruitParent").objectReferenceValue = fruitParent.transform;
            so.FindProperty("previewRenderer").objectReferenceValue =
                previewGO.GetComponent<SpriteRenderer>();

            // droppableData: 레벨 1~5 (Cherry/Strawberry/Grape/Dekopon/Persimmon)
            string[] droppableNames = { "Cherry", "Strawberry", "Grape", "Dekopon", "Persimmon" };
            var arrProp = so.FindProperty("droppableData");
            arrProp.arraySize = droppableNames.Length;
            for (int i = 0; i < droppableNames.Length; i++)
            {
                var fd = AssetDatabase.LoadAssetAtPath<FruitData>(
                    $"{DataFolder}/{droppableNames[i]}.asset"
                );
                arrProp.GetArrayElementAtIndex(i).objectReferenceValue = fd;
            }

            so.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );
        Debug.Log("[WireScene] 씬 레퍼런스 연결 완료.");
    }
}
