﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Const : ScriptableObject
{
    [Tooltip("从左边出生的位置")]
    public Vector3 leftSpawnPositionMin;
    public Vector3 leftSpawnPositionMax;
    [Tooltip("从左边最终的位置")]
    public Vector3 leftTargetPosition;
    [Tooltip("从右边出生的位置")]
    public Vector3 rightSpawnPositionMin;
    public Vector3 rightSpawnPositionMax;
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

    [Tooltip("每次 Miss 扣的 HP")]
    public float missHpDrop;

    [Tooltip("第二阶段的时间")]
    public float secondStageTime;
    [Tooltip("第三阶段的时间")]
    public float thirdStageTime;

    [Tooltip("ED 播放时间")]
    public float edTime;

    [Tooltip("物体的 Prefab，序号和表中第三列对应")]
    public List<GameObject> notePrefabs;

    [Tooltip("叹号 Prefab 存在时间")]
    public float whatHappenedPrefabLifetime = 0.5f;
    [Tooltip("叹号 Prefab")]
    public List<GameObject> whatHappenedPrefabs;

    public List<HumanStateChange> humanStateChanges;
}


[System.Serializable]
public class HumanStateChange
{
    [Tooltip("对应音乐中的时间")]
    public float time;
    [Tooltip("此时要切的状态名")]
    public string state;
}