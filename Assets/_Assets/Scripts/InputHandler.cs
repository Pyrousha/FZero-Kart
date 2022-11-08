using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputHandler : Singleton<InputHandler>
{ 
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
    private ButtonState accelerateBoost;
    public ButtonState AccelerateBoost => accelerateBoost;

    private ButtonState brake;
    public ButtonState Brake => brake;

    private float steering;
    public float Steering => steering;

    private ButtonState leftDrift;
    public ButtonState LeftDrift => leftDrift;

    private ButtonState rightDrift;
    public ButtonState RightDrift => rightDrift;


    //Combat Buttons
    private ButtonState attack;
    public ButtonState Attack => attack;


    //Camera buttons
    private float look;
    public float Look => look;


    //Menu Buttons
    private ButtonState menu_Up;
    public ButtonState Menu_Up => menu_Up;

    private ButtonState menu_Down;
    public ButtonState Menu_Down => menu_Down;

    private ButtonState menu_Left;
    public ButtonState Menu_Left => menu_Left;

    private ButtonState menu_Right;
    public ButtonState Menu_Right => menu_Right;


    private ButtonState menu_Toggle;
    public ButtonState Menu_Toggle => menu_Toggle;

    private ButtonState menu_Confirm;
    public ButtonState Menu_Confirm => menu_Confirm;

    private ButtonState menu_Back;
    public ButtonState Menu_Back => menu_Back;

    // Update is called once per frame
    void LateUpdate()
    {
        //Rest direction buttons
        accelerateBoost.Reset();
        brake.Reset();
        //steering = 0;

        leftDrift.Reset();
        rightDrift.Reset();

        //reset attack inputs
        attack.Reset();

        //reset camera buttons
        //look = 0;

        //reset menu buttons
        menu_Up.Reset();
        menu_Down.Reset();
        menu_Left.Reset();
        menu_Right.Reset();

        menu_Toggle.Reset();
        menu_Confirm.Reset();
        menu_Back.Reset();
    }

    public int DriftAxis()
    {
        int toReturn = 0;
        if (leftDrift.held)
            toReturn -= 1;
        if (rightDrift.held)
            toReturn += 1;

        return toReturn;
    }

    //Set movement
    public void Button_AccelerateBoost(InputAction.CallbackContext ctx)
    {
        accelerateBoost.Set(ctx);
    }
    public void Button_Brake(InputAction.CallbackContext ctx)
    {
        brake.Set(ctx);
    }

    public void Axis_Steering(InputAction.CallbackContext ctx)
    {
        steering = ctx.ReadValue<float>();
    }

    public void Button_LeftDrift(InputAction.CallbackContext ctx)
    {
        leftDrift.Set(ctx);
    }
    public void Button_RightDrift(InputAction.CallbackContext ctx)
    {
        rightDrift.Set(ctx);
    }


    //Set Combat Buttons
    public void Button_Attack(InputAction.CallbackContext ctx)
    {
        attack.Set(ctx);
    }


    //Set Camera Buttons
    public void Axis_Look(InputAction.CallbackContext ctx)
    {
        look = ctx.ReadValue<float>();
    }


    //Set Menu buttons
    public void Button_Menu_Up(InputAction.CallbackContext ctx)
    {
        menu_Up.Set(ctx);
    }
    public void Button_Menu_Down(InputAction.CallbackContext ctx)
    {
        menu_Down.Set(ctx);
    }
    public void Button_Menu_Left(InputAction.CallbackContext ctx)
    {
        menu_Left.Set(ctx);
    }
    public void Button_Menu_Right(InputAction.CallbackContext ctx)
    {
        menu_Right.Set(ctx);
    }

    public void Button_Menu_Toggle(InputAction.CallbackContext ctx)
    {
        menu_Toggle.Set(ctx);
    }
    public void Button_Menu_Confirm(InputAction.CallbackContext ctx)
    {
        menu_Confirm.Set(ctx);
    }
    public void Button_Menu_Back(InputAction.CallbackContext ctx)
    {
        menu_Back.Set(ctx);
    }
}
