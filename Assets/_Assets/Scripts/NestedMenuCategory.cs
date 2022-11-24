using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class NestedMenuCategory : MonoBehaviour
{
    [SerializeField] private bool currActiveMenu = false;
    [SerializeField] private bool deactiveAfterPress = false;

    private Vector3 activeLocation;
    private Vector3 inactiveLocation;

    [SerializeField] private NestedMenuCategory previousMenu;
    private NestedMenuCategory nextMenu;

    [SerializeField] private List<Selectable> selectables = new List<Selectable>();
    public List<Selectable> Selectables => selectables;

    private Coroutine currCoroutine = null;
    private Vector3 lerpDestination = new Vector3(500, 500, 500);

    [field: SerializeField] public Selectable LastSelected { get; private set; }

    [SerializeField] private UnityEvent onCancelledEvent;

    void Awake()
    {
        if (selectables.Count == 0)
            selectables = new List<Selectable>(GetComponentsInChildren<Selectable>());

        LastSelected = selectables[0];

        activeLocation = Vector3.zero;
        inactiveLocation = transform.localPosition;
    }

    void Start()
    {
        if (currActiveMenu)
        {
            OnActivate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((InputHandler.Instance.Menu_Back.down) && (currActiveMenu))
        {
            if (SetLastSelectedButton())
            {
                if (onCancelledEvent != null)
                    onCancelledEvent.Invoke();

                OnDeactivate();
            }
            else
            {
                GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
                if (selectedObj.tag == "DropdownItem")
                    selectedObj.transform.parent.parent.parent.parent.GetComponent<TMP_Dropdown>().OnCancel(new BaseEventData(EventSystem.current));
            }
        }
    }

    /// <summary>
    /// Called when this menu becomes the currently active one.
    /// lerps to "active" location and selects first button
    /// Additionally, sets previous menu var to menu which called this function
    /// </summary>
    public void OnActivate(NestedMenuCategory _prevMenu)
    {
        StartCoroutine(Activate(_prevMenu));
    }

    /// <summary>
    /// Called when this menu becomes the currently active one.
    /// lerps to "active" location and selects first button
    /// </summary>
    public void OnActivate()
    {
        StartCoroutine(Activate());
    }

    private IEnumerator Activate(NestedMenuCategory _prevMenu = null)
    {
        yield return null;
        yield return null;

        currActiveMenu = true;

        if (_prevMenu != null)
            previousMenu = _prevMenu;

        LastSelected.Select();
        if (currCoroutine != null)
        {
            //Finish current coroutine before starting next one
            StopCoroutine(currCoroutine);
            transform.localPosition = lerpDestination;
        }
        currCoroutine = StartCoroutine(LerpToPos(activeLocation));
    }

    /// <summary>
    /// Called when "menu_back" is pressed while this is the current active menu, or if the menu is set to deactivate after pressed.
    /// lerps to "inactive" location and selects the previous menu's first button
    /// </summary>
    public void OnDeactivate(bool activatePreviousMenu = true)
    {
        StartCoroutine(Deactivate(activatePreviousMenu));
    }

    private IEnumerator Deactivate(bool activatePreviousMenu = true)
    {
        yield return null;
        yield return null;

        if (previousMenu == null && activatePreviousMenu)
            yield break;

        currActiveMenu = false;

        if (activatePreviousMenu)
            previousMenu.OnActivate();

        if (currCoroutine != null)
        {
            //Finish current coroutine before starting next one
            StopCoroutine(currCoroutine);
            transform.localPosition = lerpDestination;
        }
        currCoroutine = StartCoroutine(LerpToPos(inactiveLocation));
    }

    /// <summary>
    /// Sets the last selected variable if the current selected object is one of this category's selectables.
    /// </summary>
    /// <returns> true if current selected object is one of this category's selectables </returns>
    private bool SetLastSelectedButton()
    {
        foreach (Selectable select in selectables)
        {
            if (select.gameObject == EventSystem.current.currentSelectedGameObject)
            {
                LastSelected = select;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Called when a button is pressed with an action to head to a new menu
    /// </summary>
    /// <param name="_buttonPressed"> button that was pressed, contains info of which menu to go to </param>
    public void GoToNextMenu(NestedMenuButton _buttonPressed)
    {
        currActiveMenu = false;

        LastSelected = _buttonPressed.Button;

        if (deactiveAfterPress)
            OnDeactivate(false);

        _buttonPressed.NextMenu.OnActivate(this);
    }

    /// <summary>
    /// Lerp this transform to a new position
    /// </summary>
    /// <param name="_newPos"> position to lerp to </param>
    /// <returns></returns>
    private IEnumerator LerpToPos(Vector3 _newPos)
    {
        lerpDestination = _newPos;

        while (Vector3.Distance(transform.localPosition, _newPos) > 2)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _newPos, 0.05f);
            //Debug.Log(transform.localPosition);
            yield return null;
        }

        transform.localPosition = _newPos;
        currCoroutine = null;
    }
}
