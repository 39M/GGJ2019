using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Const : ScriptableObject
{
    [Tooltip("从左边出生的位置")]
    public Vector3 leftSpawnPosition;
    [Tooltip("从左边最终的位置")]
    public Vector3 leftTargetPosition;
    [Tooltip("从右边出生的位置")]
    public Vector3 rightSpawnPosition;
    [Tooltip("从右边最终的位置")]
    public Vector3 rightTargetPosition;
    [Tooltip("出生的尺寸")]
    public float spawnScale;
    [Tooltip("最终的尺寸")]
    public float targetScale;
    [Tooltip("命中判定区间，以完美判定点为基准")]
    public float hitRange;

    [Tooltip("在空中运行的时间")]
    public float lifetime;

    [Tooltip("物体的 Prefab，序号和表中第三列对应")]
    public List<GameObject> notePrefabs;
}
