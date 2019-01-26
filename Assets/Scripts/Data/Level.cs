using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    [Tooltip("音乐")]
    public AudioClip bgm;
    [Tooltip("节奏点列表")]
    public List<NoteInfo> noteInfoList;
}
