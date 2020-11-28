using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
// using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GameManager : MonoBehaviour
{
    GameObject transition;
    // PlayerInputManager playerInputManager;
    public CinemachineTargetGroup targetGroup;
    public GameObject percentage;
    public GameObject playerPrefab;
    public GameObject stockImage;

    public Animator GAMEanimator;
    AudioManager audioManager;

    float targetGroupWeight = 5f;
    Color[] playerColors = { Color.red, Color.blue, Color.yellow, Color.green };

    public GameObject ingameCanvas;

    int[] playerLives;
    int playersAlive = 0;

    private void Awake()
    {
        Time.timeScale = 1f;
        transition = Camera.main.transform.GetChild(0).GetChild(0).gameObject;
        transition.SetActive(true);
        StartCoroutine(ChangeSceneIn());

        // playerInputManager = GetComponent<PlayerInputManager>();
        audioManager = FindObjectOfType<AudioManager>();

        playerLives = new int[InputUser.all.Count];

        print("================== game ===================");
        foreach (InputUser user in InputUser.all)
        {
            print((user.index, user.pairedDevices[0].name));

            playerLives[user.index] = 3;
            playersAlive += 1;

            AddPlayer(user);
        }
    }

    #region Event Listeners

    void onPlayerJoined(PlayerInput playerInput)
    {
        // Creating an empty GameObject that will follow the player (for camera framing purposes)
        GameObject followGO = new GameObject("FollowTargetGroupElement");
        followGO.AddComponent<FollowTargetGroupElements>();
        followGO.GetComponent<FollowTargetGroupElements>().objectToFollow = playerInput.gameObject;
        followGO.GetComponent<FollowTargetGroupElements>().boundSize = new Vector2(40, 20);
        followGO.GetComponent<FollowTargetGroupElements>().weight = targetGroupWeight;

        // Add it to the camera's target group
        targetGroup.AddMember(followGO.transform, 1, targetGroupWeight);

        // Change the players overhead arrow color
        playerInput.GetComponentInChildren<SpriteShapeRenderer>().color = playerColors[playerInput.playerIndex];

        // Add the Players Percentage UI if it doesn't exist
        if (playerInput.playerIndex + 1 > ingameCanvas.transform.GetChild(0).childCount)
            Instantiate(percentage, ingameCanvas.transform.GetChild(0)).GetComponent<Image>().color = playerColors[playerInput.playerIndex];

        // Default percentage to zero
        UpdatePercentage(playerInput.playerIndex, 0);

    }

    void onPlayerLeft(PlayerInput playerInput)
    {
        // targetGroup.RemoveMember(playerInput.transform);
    }

    #endregion

    // private void Update()
    // {
    //     var keyboard = Keyboard.current;

    //     if (keyboard.rKey.wasPressedThisFrame)
    //     {
    //         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //     }
    // }

    public void UpdatePercentage(int playerIndex, int value)
    {
        ingameCanvas.transform.GetChild(0).GetChild(playerIndex).GetComponentInChildren<TMP_Text>().text = value.ToString() + "%";
    }

    public void UpdateLives(int playerIndex, int value)
    {
        Transform lives = ingameCanvas.transform.GetChild(0).GetChild(playerIndex).GetChild(1);

        foreach (Transform life in lives)
        {
            Destroy(life.gameObject);
        }

        for (int i = 0; i < value; i++)
        {
            Instantiate(stockImage, lives);
        }
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

    void AddPlayer(InputUser user)
    {

        Transform spawnPoint = GameObject.Find("SpawnPoint" + user.index).transform;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        playerInstance.GetComponent<Player>().user = user;

        // Create a new instance of input actions to prevent InputUser from triggering actions on another InputUser.
        Controls controlsForThisUser = new Controls();
        // you must enable the controls to use them
        controlsForThisUser.Enable();
        // the real work is done for us in InputUser
        user.AssociateActionsWithUser(controlsForThisUser);
        playerInstance.GetComponent<PlayerMovement>().BindControls(controlsForThisUser.Player);

        // Creating an empty GameObject that will follow the player (for camera framing purposes)
        GameObject followGO = new GameObject("FollowTargetGroupElement");
        followGO.AddComponent<FollowTargetGroupElements>();
        followGO.GetComponent<FollowTargetGroupElements>().objectToFollow = playerInstance;
        followGO.GetComponent<FollowTargetGroupElements>().boundSize = new Vector2(40, 20);
        followGO.GetComponent<FollowTargetGroupElements>().weight = targetGroupWeight;

        // Add it to the camera's target group
        targetGroup.AddMember(followGO.transform, 1, targetGroupWeight);

        // Change the players overhead arrow color
        playerInstance.GetComponentInChildren<SpriteShapeRenderer>().color = playerColors[user.index];

        // Add the Players Percentage UI if it doesn't exist
        if (user.index + 1 > ingameCanvas.transform.GetChild(0).childCount)
            Instantiate(percentage, ingameCanvas.transform.GetChild(0)).GetComponent<Image>().color = playerColors[user.index];

        // Default percentage to zero
        UpdatePercentage(user.index, 0);
        UpdateLives(user.index, playerLives[user.index]);
    }

    public void OnPlayersDeath(int index)
    {
        playerLives[index] -= 1;
        if (playerLives[index] > 0)
            AddPlayer(InputUser.all[index]);
        else
            playersAlive -= 1;

        UpdateLives(index, playerLives[index]);

        if (playersAlive < 2)
        {
            for (int i = 0; i < playerLives.Length; i++)
            {
                if (playerLives[i] != 0)
                    PlayerPrefs.SetInt("PlayerIndex", i);
            }
            StartCoroutine(EndGameSequence());
        }

    }

    IEnumerator EndGameSequence()
    {
        float timeScale = 1f;
        float controlRate = 0.01f / 1; // waitforseconds / seconds

        targetGroup.RemoveMember(targetGroup.m_Targets[0].target);
        GAMEanimator.Play("GAME");
        audioManager.Play("GAME");
        while (timeScale > 0f)
        {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(0.01f);
            timeScale -= controlRate;
        }

        timeScale = 0f;
        Time.timeScale = timeScale;
        ChangeScene("EndGame");
    }
}
