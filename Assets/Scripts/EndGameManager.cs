using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using TMPro;

public class EndGameManager : MonoBehaviour
{
    Color[] playerColors = {Color.red, Color.blue, Color.yellow, Color.green};  
    public TMP_Text text;
    public SpriteShapeRenderer arrow;
    GameObject transition;

    private void Start() {
        text.text = "PLAYER " + (PlayerPrefs.GetInt("PlayerIndex") + 1);
        text.color = playerColors[PlayerPrefs.GetInt("PlayerIndex")];

        arrow.color = playerColors[PlayerPrefs.GetInt("PlayerIndex")];

        Time.timeScale = 1f;
        transition = Camera.main.transform.GetChild(0).GetChild(0).gameObject;;
        transition.SetActive(true);
        StartCoroutine(ChangeSceneIn());
    }

    public void goToCharacterSelect()
    {
        ChangeScene("CharacterSelect");
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneOut(sceneName));
    }

    IEnumerator ChangeSceneOut(string sceneName)
    {
        transition.SetActive(true);
        transition.GetComponent<Animator>().Play("SwipeOut");
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ChangeSceneIn()
    {
        transition.GetComponent<Animator>().Play("SwipeIn");
        yield return new WaitForSecondsRealtime(0.5f);
        transition.SetActive(false);
    }
}
