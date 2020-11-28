using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject canvas;
    public Button button;
    Controls Controls;
    AudioManager audioManager;
    GameObject transition;

    Color[] playerColors = { Color.red, Color.blue, Color.yellow, Color.green };

    private void Awake()
    {
        for (int i = InputUser.all.Count - 1; i >= 0; i--)
        {
            InputUser.all[i].UnpairDevicesAndRemoveUser();
        }

        Time.timeScale = 1f;
        transition = Camera.main.transform.GetChild(0).GetChild(0).gameObject;;
        transition.SetActive(true);
        StartCoroutine(ChangeSceneIn());

        // canvas = FindObjectOfType<Canvas>().transform.GetChild(0).gameObject;
        audioManager = FindObjectOfType<AudioManager>();

        // my IInputActionCollection
        Controls = new Controls();
        // you must enable
        Controls.Enable();
        // assign the callback for listening
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;

        // Listening must be enabled explicitly.
        // ++InputUser.listenForUnpairedDeviceActivity;
        InputUser.listenForUnpairedDeviceActivity = 4;

        button.interactable = false;
    }

    void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        // Debug.Log("Unpaired device detected " + control.device.displayName +  " ||| " + control.path);
        // Ignore anything but button presses.
        bool isGamepad = false;
        if (control.device.displayName.Length > 18)
            isGamepad = control.device.displayName.Substring(0, 19) == "Wireless Controller";
        if (!(control is ButtonControl && (control.device.displayName == "Keyboard" || isGamepad)))
            return;
        Debug.Log("Unpaired device added " + control.device.displayName + " " + control.device.deviceId);

        // get a new InputUser, now paired with the device
        InputUser user = InputUser.PerformPairingWithDevice(control.device);

        canvas.transform.GetChild(user.index).GetComponent<Image>().color = playerColors[user.index];
        canvas.transform.GetChild(user.index).GetChild(0).gameObject.SetActive(true);

        audioManager.Play("Menu Tick");

        InputUser.listenForUnpairedDeviceActivity--;

        if (InputUser.listenForUnpairedDeviceActivity < 3) {
            button.interactable = true;
        } else {
            button.interactable = false;
        }
    }

    public void printHey()
    {
        InputUser.listenForUnpairedDeviceActivity = 0;
        audioManager.Play("Menu Submit");
        ChangeScene("SampleScene");
    }

    private void OnDestroy()
    {
        InputUser.listenForUnpairedDeviceActivity = 0;
        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
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
