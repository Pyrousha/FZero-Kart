using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventInvoker : MonoBehaviour
{
    [SerializeField] private List<UnityEvent> events = new List<UnityEvent>();

    public void DoEvent(int index)
    {
        events[index].Invoke();
    }
}
