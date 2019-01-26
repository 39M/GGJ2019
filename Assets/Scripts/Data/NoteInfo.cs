using UnityEngine;

[CreateAssetMenu]
public class NoteInfo : ScriptableObject
{
    [Tooltip("Prefab")]
    public GameObject prefab;
    [Tooltip("出现的时间")]
    public float spawnTime;
    [Tooltip("对应音乐中的时间")]
    public float beatTime;
    [Tooltip("是从左边来的吗")]
    public bool isLeft;
}
