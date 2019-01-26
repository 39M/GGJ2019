using UnityEngine;

public class NoteInfo
{
    [Tooltip("Prefab")]
    public GameObject prefab;
    [Tooltip("对应音乐中的时间")]
    public float beatTime;
    [Tooltip("是从左边来的吗")]
    public bool isLeft;
}
