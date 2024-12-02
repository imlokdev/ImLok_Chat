using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AtalhoManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonsEvent : UnityEvent{ }

    public InputField inicialSelect;
    public ButtonsEvent onEnter, onEsc, onTab;

    private void Start() => inicialSelect?.Select();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) onEnter?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape)) onEsc?.Invoke();
        if (Input.GetKeyDown(KeyCode.Tab)) onTab?.Invoke();
    }
}