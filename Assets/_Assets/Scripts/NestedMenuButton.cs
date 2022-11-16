using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NestedMenuButton : MonoBehaviour
{
    [field: SerializeField] public NestedMenuCategory NextMenu { get; private set; }
    [SerializeField] private UnityEvent onCancelledEvent;

    public Button Button { get; private set; }

    public void OnCancelled()
    {
        if (onCancelledEvent != null)
            onCancelledEvent.Invoke();
    }

    void Awake()
    {
        Button = GetComponent<Button>();
    }
}
