using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    /// <summary>
    /// Text shown in a popup window above the player saying what this interactable does
    /// </summary>
    [field: SerializeField] public string PopupText { get; set; }

    [field: SerializeField] public int Priority { get; set; }
    [field: SerializeField] public bool OnlyInteractOnce { get; set; }

    [SerializeField] private UnityEvent eventToDo;

    public void OnInteract()
    {
        eventToDo.Invoke();

        if (OnlyInteractOnce)
            Destroy(gameObject);
    }
}
