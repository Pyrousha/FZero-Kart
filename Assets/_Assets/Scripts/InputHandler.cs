using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance;

    private bool dialogueInteractPressed;

    public struct ButtonState
    {
        private bool firstFrame;
        public bool held{get; private set;}
        public bool down
        {
            get
            {
                return held && firstFrame;
            }
        }
        public bool released
        {
            get
            {
                return !held && firstFrame;
            }
        }

        public void Set(InputAction.CallbackContext ctx)
        {
            held = !ctx.canceled;
            firstFrame = true;
        }
        public void Reset()
        {
            firstFrame = false;
        }
    }

    //Movement Buttons
    private ButtonState up;
    public ButtonState Up => up;

    private ButtonState down;
    public ButtonState Down => down;

    private ButtonState left;
    public ButtonState Left => left;

    private ButtonState right;
    public ButtonState Right => right;

    //Combat Buttons
    private ButtonState attack;
    public ButtonState Attack => attack;

    private ButtonState block;
    public ButtonState Block => block;

    //Interaction buttons
    private ButtonState interact;
    public ButtonState Interact => interact;

    private ButtonState menu;
    public ButtonState Menu => menu;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    void Update()
    {
        dialogueInteractPressed = interact.down;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        direction = GetDirection();

        //Rest direction buttons
        up.Reset();
        down.Reset();
        left.Reset();
        right.Reset();

        //reset attack inputs
        attack.Reset();
        block.Reset();

        //reset menu buttons
        interact.Reset();
        menu.Reset();
    }

    private Vector2 GetDirection()
    {
        float x = 0;
        float y = 0;

        if(up.held)
            y++;
        if (down.held)
            y--;

        if (left.held)
            x--;
        if (right.held)
            x++;

        Vector2 dir = new Vector2(x, y);
        dir = dir.normalized;

        //Debug.Log(dir);

        return dir;
    }

    private Vector2 direction;
    public Vector2 Direction => direction;

    //Set movement
    public void Button_Up(InputAction.CallbackContext ctx)
    {
        up.Set(ctx);
    }
    public void Button_Down(InputAction.CallbackContext ctx)
    {
        down.Set(ctx);
    }
    public void Button_Left(InputAction.CallbackContext ctx)
    {
        left.Set(ctx);
    }
    public void Button_Right(InputAction.CallbackContext ctx)
    {
        right.Set(ctx);
    }

    //Set Combat Buttons
    public void Button_Attack(InputAction.CallbackContext ctx)
    {
        attack.Set(ctx);
    }
    public void Button_Block(InputAction.CallbackContext ctx)
    {
        block.Set(ctx);
    }

    //Set Misc Buttons
    public void Button_Menu(InputAction.CallbackContext ctx)
    {
        menu.Set(ctx);
    }

    public void Button_Interact(InputAction.CallbackContext ctx)
    {
        interact.Set(ctx);
    }
}
