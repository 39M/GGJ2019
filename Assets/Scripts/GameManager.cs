using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    new AudioSource audio;
    public Level level;
    public Const gameConst;
    public int noteInfoIndex = 0;
    public NoteInfo currentNoteInfo;
    public List<Note> noteList = new List<Note>();

    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.clip = level.bgm;
        audio.Play();

        currentNoteInfo = level.noteInfoList[0];

        DOTween.defaultEaseType = Ease.Linear;
    }

    void Update()
    {
        CreateNotes();
        CheckHit();
    }

    void CreateNotes()
    {
        while (noteInfoIndex < level.noteInfoList.Count && currentNoteInfo.spawnTime < audio.time)
        {
            GameObject gameObject = Instantiate(currentNoteInfo.prefab);
            float lifetime = currentNoteInfo.beatTime + gameConst.hitRange - audio.time;
            gameObject.transform.position = currentNoteInfo.isLeft ? gameConst.leftSpawnPosition : gameConst.rightSpawnPosition;
            gameObject.transform.localScale = new Vector3(gameConst.spawnScale, gameConst.spawnScale, gameConst.spawnScale);
            gameObject.transform.DOScale(gameConst.targetScale, lifetime);

            Note note = new Note
            {
                gameObject = gameObject,
                info = currentNoteInfo
            };

            noteList.Add(note);

            gameObject.transform.DOMove(currentNoteInfo.isLeft ? gameConst.leftTargetPosition : gameConst.rightTargetPosition,
                lifetime).OnComplete(() =>
                {
                    OnMiss(note);
                });

            noteInfoIndex++;
            currentNoteInfo = noteInfoIndex < level.noteInfoList.Count ? level.noteInfoList[noteInfoIndex] : null;
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

        Destroy(note.gameObject);
        noteList.Remove(note);

        // audio
        // animation
        // effect
    }

    void OnMiss(Note note)
    {
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
