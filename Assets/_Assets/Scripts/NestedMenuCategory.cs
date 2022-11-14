using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NestedMenuCategory : MonoBehaviour
{
    [SerializeField] private bool currActiveMenu = false;
    private bool startMenu = false;

    private Vector3 activeLocation;
    private Vector3 inactiveLocation;

    private NestedMenuCategory previousMenu;
    private NestedMenuCategory nextMenu;

    private List<Button> buttons = new List<Button>();

    private Coroutine currCoroutine = null;
    private Vector3 lerpDestination = new Vector3(500, 500, 500);

    public Button LastSelectedButton { get; private set; }

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            buttons.Add(transform.GetChild(i).GetComponent<Button>());
        }

        LastSelectedButton = buttons[0];

        activeLocation = Vector3.zero;
        inactiveLocation = transform.localPosition;
    }

    void Start()
    {
        if (currActiveMenu)
        {
            startMenu = true;
            OnActivate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((InputHandler.Instance.Menu_Back.down) && (currActiveMenu))
        {
            SetLastSelectedButton();
            OnDeactive();
        }
    }

    /// <summary>
    /// Called when this menu becomes the currently active one.
    /// lerps to "active" location and selects first button
    /// Additionally, sets previous menu var to menu which called this function
    /// </summary>
    public void OnActivate(NestedMenuCategory _prevMenu = null)
    {
        currActiveMenu = true;

        if (_prevMenu != null)
            previousMenu = _prevMenu;

        LastSelectedButton.Select();
        if (currCoroutine != null)
        {
            //Finish current coroutine before starting next one
            StopCoroutine(currCoroutine);
            transform.localPosition = lerpDestination;
        }
        currCoroutine = StartCoroutine(LerpToPos(activeLocation));
    }

    /// <summary>
    /// Called when "menu_back" is pressed while this is the current active menu
    /// lerps to "inactive" location and selects the previous menu's first button
    /// </summary>
    public void OnDeactive()
    {
        if (previousMenu == null)
            return;

        currActiveMenu = false;

        previousMenu.LastSelectedButton.Select();

        if (currCoroutine != null)
        {
            //Finish current coroutine before starting next one
            StopCoroutine(currCoroutine);
            transform.localPosition = lerpDestination;
        }
        currCoroutine = StartCoroutine(LerpToPos(inactiveLocation));
    }

    private void SetLastSelectedButton()
    {
        foreach (Button button in buttons)
        {
            if (button.gameObject == EventSystem.current.currentSelectedGameObject)
            {
                LastSelectedButton = button;
                break;
            }
        }
    }

    /// <summary>
    /// Called when a button is pressed with an action to head to a new menu
    /// </summary>
    /// <param name="_buttonPressed"> button that was pressed, contains info of which menu to go to </param>
    public void GoToNextMenu(NestedMenuButton _buttonPressed)
    {
        currActiveMenu = false;

        LastSelectedButton = _buttonPressed.Button;
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
