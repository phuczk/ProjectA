using UnityEngine;
using TMPro;
using System.Collections;

public abstract class Interactable : MonoBehaviour
{
    public TextMeshPro interactText;
    [SerializeField] GameObject interactObject;
    [SerializeField] private string _interactKey;
    protected virtual string InteractKey => _interactKey;

    private PlayerInputHandler _input;
    private Transform _player;
    private bool _inRange;
    private Coroutine _delayCoroutine;

    public float delayShowMessage = 0f;

    private void Start()
    {
        if (interactText != null)
        {
            var key = InteractKey;
            if (!string.IsNullOrEmpty(key))
                interactText.text = Localization.Get(key);
            interactObject?.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _input = other.GetComponent<PlayerInputHandler>();
            _player = other.transform;
            _inRange = true;
            
            if (delayShowMessage > 0)
            {
                _delayCoroutine = StartCoroutine(ShowInteractObjectWithDelay());
            }
            else
            {
                interactObject?.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
        }
        
        _inRange = false;
        _input = null;
        _player = null;
        interactObject?.SetActive(false);
    }

    private void Update()
    {
        if (!_inRange) return;
        if (_input != null && _input.IsInteract())
        {
            OnInteract(_player);
        }
    }

    private IEnumerator ShowInteractObjectWithDelay()
    {
        yield return new WaitForSeconds(delayShowMessage);
        
        if (_inRange && interactObject != null)
        {
            interactObject.SetActive(true);
        }
        
        _delayCoroutine = null;
    }

    protected abstract void OnInteract(Transform player);
}
