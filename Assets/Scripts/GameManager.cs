using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public float startTime = 0;

    VideoPlayer video;
    public VideoClip videoClipED;

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
    public RuntimeAnimatorController petWhenWakeUpController;

    public GameObject missEffectPrefab;
    public GameObject hitEffectPrefab;

    public float humanHp = 100;
    public GameObject humanBloodHp66;
    public GameObject humanBloodHp33;

    public int humanStateIndex = 0;

    public float audioTime;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        if (startTime > video.length)
            video.enabled = false;

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

        humanBloodHp33.SetActive(false);
        humanBloodHp66.SetActive(false);
    }

    void Update()
    {
        audioTime = audio.time;

        if (humanStateIndex < gameConst.humanStateChanges.Count && gameConst.humanStateChanges[humanStateIndex].time < audio.time)
        {
            string stateName = gameConst.humanStateChanges[humanStateIndex].state;
            humanAnimator.Play(stateName);
            if (stateName.Equals("OpenEye"))
            {
                petAnimator.runtimeAnimatorController = petWhenWakeUpController;
            }
            else
            {
                petAnimator.runtimeAnimatorController = petNormalController;
            }
            humanStateIndex++;
        }

        CreateNotes();
        CheckHit();


        if (audio.time < gameConst.edTime && audio.time >= video.length && video.enabled)
        {
            video.enabled = false;
        }
        if (audio.time >= gameConst.edTime)
        {
            video.clip = videoClipED;
            video.enabled = true;
            video.Play();
        }
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
        if (inputLeft)
        {
            petAnimator.Play("AttackLeft" + UnityEngine.Random.Range(1, 8).ToString());
        }
        if (inputRight)
        {
            petAnimator.Play("AttackRight" + UnityEngine.Random.Range(1, 8).ToString());
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
        if (33 - gameConst.missHpDrop / 2 <= humanHp && humanHp <= 33 + gameConst.missHpDrop / 2)
        {
            humanBloodHp33.SetActive(true);
        }
        else if (66 - gameConst.missHpDrop / 2 <= humanHp && humanHp <= 66 + gameConst.missHpDrop / 2)
        {
            humanBloodHp66.SetActive(true);
        }

        // audio
        // animation
        // effect
    }

    void OnVainClick()
    {

    }
}
