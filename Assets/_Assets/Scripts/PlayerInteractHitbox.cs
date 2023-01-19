using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractHitbox : MonoBehaviour
{
    private List<Interactable> interactables = new List<Interactable>();

    [SerializeField] private TextMeshProUGUI interactButtonText;
    [SerializeField] private TextMeshProUGUI popupText;

    void Update()
    {
        if (InputHandler.Instance.Menu_Confirm.down)
        {
            if (interactables.Count > 0)
            {
                Interactable highestPrioInteract = interactables[0];
                if (highestPrioInteract.OnlyInteractOnce)
                    interactables.RemoveAt(0);

                highestPrioInteract.OnInteract();
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Interactable newInteract = col.GetComponent<Interactable>();

        if (!interactables.Contains(newInteract))
        {
            interactables.Add(newInteract);
        }

        SortInteractables();
    }

    void OnTriggerExit(Collider col)
    {
        Interactable newInteract = col.GetComponent<Interactable>();

        int index = interactables.IndexOf(newInteract);
        if (index > -1)
        {
            interactables.RemoveAt(index);
        }

        SortInteractables();
    }

    /// <summary>
    /// Sorts interactables list based on priority (highest priority gets index 0)
    /// </summary>
    private void SortInteractables()
    {
        interactables.Sort(CompareInteractables);

        int CompareInteractables(Interactable interact1, Interactable interact2)
        {
            if (interact1.Priority < interact2.Priority)
                return 1;
            if (interact1.Priority > interact2.Priority)
                return -1;
            return 0;
        }

        if (interactables.Count > 0)
        {
            //show highest prio interactable popup text
            interactButtonText.text = "[" + InputHandler.Instance.Menu_Confirm.buttonName + "]";
            popupText.text = interactables[0].PopupText;
        }
        else
        {
            //clear popup text
            interactButtonText.text = "";
            popupText.text = "";
        }
    }
}
