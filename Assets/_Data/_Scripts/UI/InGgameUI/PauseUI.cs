using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour, IBackHandler
{
    [SerializeField] private GameStateChannel _stateChannel;

    public Button ResumeButton;
    public Button KeyBoardButton;
    public Button AudioButton;
    public Button ExitButton;

    public GameObject KeyboardPanel;
    public GameObject AudioPanel;

    public Button KeyboardCloseButton;

    [SerializeField] private string _slotSceneName = "SlotScene";

    private void Awake()
    {
        ResumeButton.onClick.AddListener(OnResumeClick);
        KeyBoardButton.onClick.AddListener(ToggleKeyboard);
        AudioButton.onClick.AddListener(ToggleAudio);
        KeyboardCloseButton.onClick.AddListener(ToggleKeyboard);
        ExitButton.onClick.AddListener(OnExitClick);
    }

    private void OnResumeClick()
    {
        _stateChannel.RaiseRequest(GameState.Playing);
    }

    private void OnDisable() {
        ResetUI();
    }

    public void ResetUI()
    {
        KeyboardPanel?.SetActive(false);
        AudioPanel?.SetActive(false);
        
        ResumeButton.gameObject.SetActive(true);
        KeyBoardButton.gameObject.SetActive(true);
        AudioButton.gameObject.SetActive(true);

        ExitButton.gameObject.SetActive(true);
    }

    private void ToggleKeyboard()
    {
        bool isOpeningKeyboard = !KeyboardPanel.activeSelf;

        if (!isOpeningKeyboard)
        {
            //InputRebindManager.Instance?.SaveRebinds();
        }
        ResumeButton.gameObject.SetActive(KeyboardPanel.activeSelf);
        KeyBoardButton.gameObject.SetActive(KeyboardPanel.activeSelf);
        AudioButton.gameObject.SetActive(KeyboardPanel.activeSelf);
        ExitButton.gameObject.SetActive(KeyboardPanel.activeSelf);
        KeyboardPanel.SetActive(!KeyboardPanel.activeSelf);
    }

    private void ToggleAudio()
    {
        bool isOpeningAudio = !AudioPanel.activeSelf;

        if (!isOpeningAudio)
        {
            //InputRebindManager.Instance?.SaveRebinds();
        }
        ResumeButton.gameObject.SetActive(AudioPanel.activeSelf);
        AudioButton.gameObject.SetActive(AudioPanel.activeSelf);
        KeyBoardButton.gameObject.SetActive(AudioPanel.activeSelf);
        ExitButton.gameObject.SetActive(AudioPanel.activeSelf);
        AudioPanel.SetActive(!AudioPanel.activeSelf);
    }

    private void OnExitClick()
    {
        SceneTransitionManager.Instance.TransitionToScene(_slotSceneName, TransitionType.Death, FadeDirection.Right);
        _stateChannel.RaiseRequest(GameState.MainMenu);
    }

    public bool OnBack()
    {
        if (KeyboardPanel != null && KeyboardPanel.activeSelf)
        {
            ResetUI();
            return true;
        }
        if (AudioPanel != null && AudioPanel.activeSelf)
        {
            ResetUI();
            return true;
        }
        return false;
    }
}
