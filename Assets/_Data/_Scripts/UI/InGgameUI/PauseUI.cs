using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections;
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

    public UISelectionManager _mainSelectionManager;
    public UISelectionManager _keyboardSelectionManager;
    public UISelectionManager _audioSelectionManager;

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
        
        if (_mainSelectionManager != null)
        {
            StartCoroutine(SelectAfterFrame(_mainSelectionManager));
        }
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
        KeyboardPanel.SetActive(!KeyboardPanel.activeSelf);
        
        if (isOpeningKeyboard)
        {
            if (_keyboardSelectionManager != null)
            {
                StartCoroutine(SelectAfterFrame(_keyboardSelectionManager));
            }
        }
        else
        {
            if (_mainSelectionManager != null)
            {
                StartCoroutine(SelectAfterFrame(_mainSelectionManager));
            }
        }
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
        
        if (isOpeningAudio)
        {
            if (_audioSelectionManager != null)
            {
                StartCoroutine(SelectAfterFrame(_audioSelectionManager));
            }
        }
        else
        {
            if (_mainSelectionManager != null)
            {
                StartCoroutine(SelectAfterFrame(_mainSelectionManager));
            }
        }
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
    
    private IEnumerator SelectAfterFrame(UISelectionManager selectionManager)
    {
        yield return null;
        
        yield return null;
        
        selectionManager.InitializeFromChildren();
    }
}
