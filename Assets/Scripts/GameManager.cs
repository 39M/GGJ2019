using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public float startTime = 0;

    new AudioSource audio;
    public AudioClip soundEffect;
    public Const gameConst;
    public List<NoteInfo> noteInfoList = new List<NoteInfo>();
    public int noteInfoIndex = 0;
    public NoteInfo currentNoteInfo;
    public List<Note> noteList = new List<Note>();

    public GameObject missEffectPrefab;
    public GameObject hitEffectPrefab;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.time = startTime;
        audio.Play();

        var textAsset = Resources.Load<TextAsset>("NoteInfo");
        var lines = textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var fields = line.Split(new[] { ",", ", ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length >= 3)
            {
                NoteInfo noteInfo = new NoteInfo
                {
                    beatTime = float.Parse(fields[0]),
                    isLeft = int.Parse(fields[1]) == 1,
                    prefab = gameConst.notePrefabs[int.Parse(fields[2])],
                };
                noteInfoList.Add(noteInfo);
            }
        }

        while (noteInfoList[noteInfoIndex].beatTime + gameConst.hitRange - gameConst.lifetime < startTime)
        {
            noteInfoIndex++;
        }
        currentNoteInfo = noteInfoList[noteInfoIndex];

        DOTween.defaultEaseType = Ease.Linear;
    }

    void Update()
    {
        CreateNotes();
        CheckHit();
    }

    void CreateNotes()
    {
        while (noteInfoIndex < noteInfoList.Count && currentNoteInfo.beatTime + gameConst.hitRange - gameConst.lifetime < audio.time)
        {
            GameObject gameObject = Instantiate(currentNoteInfo.prefab);
            gameObject.transform.position = currentNoteInfo.isLeft ? gameConst.leftSpawnPosition : gameConst.rightSpawnPosition;
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
        Debug.LogFormat("命中，时间：{0}，节奏点时间：{1}，准确度：{2:P}", audio.time, note.info.beatTime, Mathf.Abs(note.info.beatTime - audio.time) / gameConst.hitRange);

        GameObject effect = Instantiate(hitEffectPrefab);
        effect.transform.position = note.gameObject.transform.position;
        Destroy(effect, 1);

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
        Destroy(effect, 1);

        Destroy(note.gameObject);
        noteList.Remove(note);

        // audio
        // animation
        // effect
    }

    void OnVainClick()
    {

    }
}
