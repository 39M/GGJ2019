using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    public GameObject black;

    bool start = false;
    void Update()
    {
        if (start)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            start = true;
            black.SetActive(true);
            black.transform.DOLocalMoveZ(1, 0.25f).OnComplete(() =>
            {
                SceneManager.LoadScene("Game");
            });
        }
    }
}
