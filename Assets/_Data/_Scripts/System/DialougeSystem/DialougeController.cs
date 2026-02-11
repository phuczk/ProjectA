using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using GlobalEnums;

public class DialougeController : Singleton<DialougeController>
{
    [SerializeField] private GameStateChannel _stateChannel;

    [Header("UI")]
    [SerializeField] private GameObject _dialougePanel;
    [SerializeField] private TextMeshProUGUI _nameNPCText;
    [SerializeField] private TextMeshProUGUI _dialougeText;

    private Queue<string> paragraphs = new Queue<string>();

    public bool conversationsEnded = false;

    private bool _isTyping = false;
    private string _currentFullSentence;

    protected override void Awake()
    {
        base.Awake();

        _stateChannel ??= Resources.Load<GameStateChannel>("New Game State Channel");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveUI();
    }

    private void ResolveUI()
    {
        if (_dialougePanel == null)
        {
            var panel = GameObject.Find("DialougePanel");
            if (panel != null)
            {
                _dialougePanel = panel;
            }
        }

        if (_dialougePanel != null)
        {
            if (_nameNPCText == null)
            {
                _nameNPCText = _dialougePanel.transform
                    .Find("NameNPCText")?
                    .GetComponent<TextMeshProUGUI>();
            }

            if (_dialougeText == null)
            {
                _dialougeText = _dialougePanel.transform
                    .Find("DialougeText")?
                    .GetComponent<TextMeshProUGUI>();
            }

            _dialougePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (_dialougePanel == null) return;
        if (!_dialougePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (_isTyping)
        {
            CompleteSentence();
        }
        else
        {
            if (paragraphs.Count > 0)
            {
                ShowNextLine();
            }
            else
            {
                EndConversation();
            }
        }
    }

    private void ShowNextLine()
    {
        if (_dialougePanel == null) return;

        if (paragraphs.Count > 0)
        {
            _currentFullSentence = paragraphs.Dequeue();
            StopAllCoroutines();
            StartCoroutine(TypeSentence(_currentFullSentence));
        }
        else
        {
            EndConversation();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;

        if (_dialougeText == null) yield break;

        _dialougeText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            _dialougeText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        _isTyping = false;
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();

        if (_dialougeText != null)
            _dialougeText.text = _currentFullSentence;

        _isTyping = false;
    }

    public void DisplayNextParagraph(DialougeText dialougeText, string NPCKey)
    {
        if (_dialougePanel == null)
        {
            ResolveUI();
            if (_dialougePanel == null) return;
        }

        if (paragraphs.Count == 0)
        {
            if (!_dialougePanel.activeSelf)
            {
                StartConversation(dialougeText, NPCKey);
                ShowNextLine();
            }
            else
            {
                EndConversation();
            }
        }
        else
        {
            ShowNextLine();
        }
    }

    private void StartConversation(DialougeText dialougeText, string NPCKey)
    {
        conversationsEnded = false;
        paragraphs.Clear();

        if (_dialougePanel != null && !_dialougePanel.activeSelf)
            _dialougePanel.SetActive(true);

        _stateChannel?.RaiseRequest(GameState.Dialogue);

        if (_nameNPCText != null)
            _nameNPCText.text = dialougeText.NameNPC;

        foreach (string key in dialougeText.dialougeKey)
        {
            string content = Localization.GetByScope(NPCKey, key);
            paragraphs.Enqueue(content);
        }
    }

    private void EndConversation()
    {
        conversationsEnded = true;
        paragraphs.Clear();

        if (_dialougePanel != null)
            _dialougePanel.SetActive(false);

        _stateChannel?.RaiseRequest(GameState.Playing);

        Debug.Log("Conversation Ended.");
    }
}
