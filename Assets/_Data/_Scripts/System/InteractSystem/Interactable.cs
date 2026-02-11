using UnityEngine;
using TMPro;

public abstract class Interactable : MonoBehaviour
{
    public TextMeshPro interactText;
    [SerializeField] private string _interactKey;
    protected virtual string InteractKey => _interactKey;

    private PlayerInputHandler _input;
    private Transform _player;
    private bool _inRange;

    private void Start()
    {
        if (interactText != null)
        {
            var key = InteractKey;
            if (!string.IsNullOrEmpty(key))
                interactText.text = Localization.Get(key);
            interactText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _input = other.GetComponent<PlayerInputHandler>();
        _player = other.transform;
        _inRange = true;
        interactText?.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = false;
        _input = null;
        _player = null;
        interactText?.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_inRange) return;
        if (_input != null && _input.IsInteract())
        {
            OnInteract(_player);
        }
    }

    protected abstract void OnInteract(Transform player);
}
