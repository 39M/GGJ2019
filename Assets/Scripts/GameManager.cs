﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public float startTime = 0;

    VideoPlayer video;
    public VideoClip videoClipOP;
    public VideoClip videoClipED;
    public GameObject videoMask;

    new AudioSource audio;
    public AudioClip soundEffect;
    public Const gameConst;
    public List<NoteInfo> noteInfoList = new List<NoteInfo>();
    public int noteInfoIndex = 0;
    public NoteInfo currentNoteInfo;
    public List<Note> noteList = new List<Note>();

    public Animator humanAnimator;
    public Animator petAnimator;
    public RuntimeAnimatorController petNormalController;
    public RuntimeAnimatorController petNormalControllerHurt;
    public RuntimeAnimatorController petNormalControllerHurtMore;
    public RuntimeAnimatorController petWhenWakeUpController;
    public RuntimeAnimatorController petWhenWakeUpControllerHurt;

    public GameObject missEffectPrefab;
    public GameObject hitEffectPrefab;

    public float humanHp = 100;
    public List<GameObject> bloodList;

    public int humanStateIndex = 0;

    public float audioTime;

    public Slider slider;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        if (startTime > video.length)
        {
            video.enabled = false;
        }
        else
        {
            video.time = startTime;
            videoMask.SetActive(true);
            slider.gameObject.SetActive(false);
        }

        audio = GetComponent<AudioSource>();
        if (startTime > 0)
        {
            audio.time = startTime;
        }

        var textAsset = Resources.Load<TextAsset>("NoteInfo");
        var lines = textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var fields = line.Split(new[] { ",", ", ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length >= 2)
            {
                NoteInfo noteInfo = new NoteInfo
                {
                    beatTime = float.Parse(fields[0]),
                    isLeft = int.Parse(fields[1]) == 1,
                    prefab = gameConst.notePrefabs[UnityEngine.Random.Range(0, gameConst.notePrefabs.Count)],
                };
                noteInfoList.Add(noteInfo);
            }
        }

        while (noteInfoIndex < noteInfoList.Count && noteInfoList[noteInfoIndex].beatTime + gameConst.hitRange - gameConst.lifetime < startTime)
        {
            noteInfoIndex++;
        }
        if (noteInfoIndex < noteInfoList.Count)
        {
            currentNoteInfo = noteInfoList[noteInfoIndex];
        }
        while (humanStateIndex < gameConst.humanStateChanges.Count && gameConst.humanStateChanges[humanStateIndex].time < startTime)
        {
            humanStateIndex++;
        }

        DOTween.defaultEaseType = Ease.Linear;

        foreach (GameObject go in bloodList)
            go.SetActive(false);
    }

    bool passSecond = false;
    bool passThird = false;
    bool openEye = false;
    void Update()
    {
        audioTime = audio.time;

        if (humanStateIndex < gameConst.humanStateChanges.Count && gameConst.humanStateChanges[humanStateIndex].time < audio.time)
        {
            string stateName = gameConst.humanStateChanges[humanStateIndex].state;
            humanAnimator.Play(stateName);
            if (stateName.Equals("OpenEye"))
            {
                openEye = true;
                if (audio.time < gameConst.secondStageTime)
                    petAnimator.runtimeAnimatorController = petWhenWakeUpController;
                else
                    petAnimator.runtimeAnimatorController = petWhenWakeUpControllerHurt;
            }
            else
            {
                openEye = false;
                if (audio.time < gameConst.secondStageTime)
                    petAnimator.runtimeAnimatorController = petNormalController;
                else
                    petAnimator.runtimeAnimatorController = petNormalControllerHurt;
            }
            humanStateIndex++;
        }

        if (audio.time > gameConst.secondStageTime && !passSecond)
        {
            petAnimator.runtimeAnimatorController = petNormalControllerHurt;
            passSecond = true;
        }

        if (audio.time > gameConst.thirdStageTime && !passThird)
        {
            petAnimator.runtimeAnimatorController = petNormalControllerHurtMore;
            passThird = true;
        }

        CreateNotes();
        CheckHit();

        if (audio.time < gameConst.edTime && audio.time >= video.length && video.enabled)
        {
            video.enabled = false;
            videoMask.SetActive(false);
            slider.gameObject.SetActive(true);
        }
        if (audio.time >= gameConst.edTime)
        {
            slider.gameObject.SetActive(false);
            video.clip = videoClipED;
            video.enabled = true;
            video.Play();
        }

        if (slider.gameObject.activeSelf)
        {
            slider.value = (float)((audio.time - videoClipOP.length) / (gameConst.edTime - videoClipOP.length));
        }
    }

    void CreateNotes()
    {
        while (noteInfoIndex < noteInfoList.Count && currentNoteInfo.beatTime + gameConst.hitRange - gameConst.lifetime < audio.time)
        {
            GameObject gameObject = Instantiate(currentNoteInfo.prefab);
            gameObject.transform.position = currentNoteInfo.isLeft ?
                Vector3.Lerp(gameConst.leftSpawnPositionMin, gameConst.leftSpawnPositionMax, UnityEngine.Random.value)
                : Vector3.Lerp(gameConst.rightSpawnPositionMin, gameConst.rightSpawnPositionMax, UnityEngine.Random.value);
            gameObject.transform.localScale = new Vector3(gameConst.spawnScale, gameConst.spawnScale, gameConst.spawnScale);
            gameObject.transform.DOScale(gameConst.targetScale, gameConst.lifetime).SetEase(Ease.Linear);

            Note note = new Note
            {
                gameObject = gameObject,
                info = currentNoteInfo
            };

            noteList.Add(note);

            gameObject.transform.DOMove(currentNoteInfo.isLeft ? gameConst.leftTargetPosition : gameConst.rightTargetPosition,
                gameConst.lifetime).SetEase(Ease.Linear).OnComplete(() =>
                {
                    OnMiss(note);
                });

            noteInfoIndex++;
            currentNoteInfo = noteInfoIndex < noteInfoList.Count ? noteInfoList[noteInfoIndex] : null;
        }
    }

    void CheckHit()
    {
        bool inputLeft = false;
        bool inputRight = false;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 viewportPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            inputLeft = viewportPoint.x <= 0.5;
            inputRight = viewportPoint.x >= 0.5;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            inputLeft = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            inputRight = true;
        }
        if (inputLeft)
        {
            petAnimator.Play("AttackLeft" + UnityEngine.Random.Range(1, 8).ToString());
        }
        if (inputRight)
        {
            petAnimator.Play("AttackRight" + UnityEngine.Random.Range(1, 8).ToString());
        }
        if (inputRight || inputLeft)
        {
            if (openEye)
            {
                var go = Instantiate(gameConst.whatHappenedPrefabs[UnityEngine.Random.Range(0, gameConst.whatHappenedPrefabs.Count)]);
                Destroy(go, gameConst.whatHappenedPrefabLifetime);
            }
        }

        foreach (var note in noteList)
        {
            if (inputLeft && note.info.isLeft)
                if (Mathf.Abs(note.info.beatTime - audio.time) < gameConst.hitRange)
                {
                    OnHit(note);
                    break;
                }
        }

        foreach (var note in noteList)
        {
            if (inputRight && !note.info.isLeft)
                if (Mathf.Abs(note.info.beatTime - audio.time) < gameConst.hitRange)
                {
                    OnHit(note);
                    break;
                }
        }
    }

    void OnHit(Note note)
    {
        Debug.LogFormat("命中，时间：{0}，节奏点时间：{1}，准确度：{2:P}", audio.time, note.info.beatTime, 1 - Mathf.Abs(note.info.beatTime - audio.time) / gameConst.hitRange);

        GameObject effect = Instantiate(hitEffectPrefab);
        effect.transform.position = note.gameObject.transform.position;
        if (note.info.isLeft)
        {
            effect.transform.localScale = new Vector3(-1, 1, 1);
        }
        Destroy(effect, effect.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);

        Destroy(note.gameObject);
        noteList.Remove(note);
        audio.PlayOneShot(soundEffect);

        // audio
        // animation
        // effect
    }

    void OnMiss(Note note)
    {
        GameObject effect = Instantiate(missEffectPrefab);
        effect.transform.position = note.gameObject.transform.position;
        if (note.info.isLeft)
        {
            effect.transform.localScale = new Vector3(-1, 1, 1);
        }
        Destroy(effect, effect.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);

        Destroy(note.gameObject);
        noteList.Remove(note);

        humanHp -= gameConst.missHpDrop;
        for (int i = 0; i < bloodList.Count; i++)
        {
            bloodList[i].SetActive(humanHp < 100 - (100 / bloodList.Count * (i + 1)));
        }

        // audio
        // animation
        // effect
    }

    void OnVainClick()
    {

    }
}
