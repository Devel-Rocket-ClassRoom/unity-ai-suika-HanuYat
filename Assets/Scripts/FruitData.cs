using UnityEngine;

[CreateAssetMenu(fileName = "FruitData", menuName = "Suika/FruitData")]
public class FruitData : ScriptableObject
{
    public int level;
    public string fruitName;
    public Sprite sprite;
    public Color color;
    public float radius; // world units — CircleCollider2D 반지름
    public int mergeScore;
    public GameObject prefab; // 병합 시 Instantiate 대상 (본인 프리팹 자기참조)
    public FruitData nextFruit; // level == 11이면 null
}
