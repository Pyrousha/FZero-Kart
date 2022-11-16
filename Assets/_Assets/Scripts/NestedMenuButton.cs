using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NestedMenuButton : MonoBehaviour
{
    [field: SerializeField] public NestedMenuCategory NextMenu { get; private set; }

    public Button Button { get; private set; }

    void Awake()
    {
        Button = GetComponent<Button>();
    }
}
