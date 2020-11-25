using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseButtonEvents : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [Tooltip("The index should match the top-down order of the buttons starting from 0.")]
    public int buttonIndex;
    public event EventHandler<OnButtonEventArgs> OnButtonEvent;
    public class OnButtonEventArgs : EventArgs
    {
        public int buttonIndex;
        public string eventType;
    }
    public Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            OnButtonEvent?.Invoke(this, new OnButtonEventArgs
            {
                buttonIndex = this.buttonIndex,
                eventType = "Highlighted"
            });
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.interactable)
        {
            OnButtonEvent?.Invoke(this, new OnButtonEventArgs
            {
                buttonIndex = this.buttonIndex,
                eventType = "Selected"
            });
        }
    }
}
