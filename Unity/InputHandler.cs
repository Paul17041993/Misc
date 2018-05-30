using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;
using System.Collections.Generic;

// if the statement above has an error, you need to add the reference to the DLL,
// < references on the left, add, browse, its in assets > plugins

public class InputHandler : MonoBehaviour
{

    /// <summary>
    /// used by everything that needs to check the state of an axie
    /// </summary>
    public enum InputAxies
    {
        MOVEX,
        MOVEY,
        ACTION1,
        ACTION2,

        size
    };



    public enum InputTypes
    {
        KEYBOARD, CONTROLLER
    };



    public abstract class InputNode
    {
        private readonly float[] lastAxies = new float[(int)InputAxies.size];
        private readonly float[] currAxies = new float[(int)InputAxies.size];

        //to override
        public abstract float GetAxis(InputAxies axis);

        public float GetLastAxis(InputAxies axis)
        {
            return lastAxies[(int)axis];
        }

        public bool GetState(InputAxies axis)
        {
            return GetAxis(axis) > 0.1f ? true : false;
        }

        public bool GetLastState(InputAxies axis)
        {
            return GetLastAxis(axis) > 0.1f ? true : false;
        }

        public bool GetDown(InputAxies axis)
        {
            return !GetLastState(axis) && GetState(axis);
        }

        public bool GetUp(InputAxies axis)
        {
            return GetLastState(axis) && !GetState(axis);
        }

        public virtual void Update() 
        {
            //push and update axis
            Array.Copy(currAxies, lastAxies, (int)InputAxies.size);
            for (int i = 0; i < (int)InputAxies.size; i++)
                currAxies[i] = GetAxis((InputAxies)i);
        }

        public virtual void ForceFeedback(float scaleL, float scaleH, float time = 0f) { }
    }

    public class KeyboardInputNode : InputNode
    {
        KeyAxie[] Keys;

        public class KeyAxie
        {
            public virtual float GetValue() { return 0f; }
        };

        public class OneKeyAxie : KeyAxie
        {
            KeyCode KeyA;
            public OneKeyAxie(KeyCode keyA) { this.KeyA = keyA; }
            public override float GetValue() { return Input.GetKey(KeyA) ? 1f : 0f; }
        };

        public class TwoKeyAxie : KeyAxie
        {
            KeyCode KeyA;
            KeyCode KeyB;
            public TwoKeyAxie(KeyCode keyA, KeyCode keyB) { this.KeyA = keyA; this.KeyB = keyB; }
            public override float GetValue() { return (Input.GetKey(KeyA) ? 1f : 0f) - (Input.GetKey(KeyB) ? 1f : 0f); }
        };


        public KeyboardInputNode(KeyAxie[] keybinds)
        {
            Keys = keybinds;

        }

        public override float GetAxis(InputAxies axis)
        {
            try
            {
                return Keys[(int)axis].GetValue();
            }
            catch (Exception exc)
            {
                Debug.LogError(string.Format("Fault in getting axis state {0} for keyboard+mouse!", axis));
                Debug.LogError(exc.Message);
            }

            // only if the above try fails
            return 0f;
        }

        //public void Update() { }
    }

    public class Controller
    {
        public Controller(PlayerIndex id)
        {
            ControllerID = id;
            vibes = new List<VibeState>();
        }

        PlayerIndex ControllerID = 0;

        public GamePadState CurrentState;

        public class VibeState
        {
            public VibeState(float sL, float sH, float t) { scaleL = sL; scaleH = sH; time = t; }
            public float scaleL, scaleH, time;
            public void Update(float delta) { this.time -= delta; }
        }

        List<VibeState> vibes;

        public void AddVibe(VibeState vibe)
        {
            vibes.Add(vibe);
        }

        public void Update()
        {
            CurrentState = GamePad.GetState(ControllerID);

            float L = 0f, H = 0f;
            for (int i = 0; i < vibes.Count; )
            {
                L += vibes[i].scaleL;
                H += vibes[i].scaleH;
                vibes[i].Update(Time.deltaTime);
                if (vibes[i].time <= 0f)
                    vibes.RemoveAt(i);
                else
                    ++i;
            }
            GamePad.SetVibration(ControllerID, Mathf.Min(L, 1f), Mathf.Min(H, 1f));
        }
    }

    public class ControllerInputNode : InputNode
    {
        public enum ControllerAxies
        {
            A, B, X, Y,
            LTX, LTY, LTZ,
            RTX, RTY, RTZ,
            LTr, RTr,
            LB, RB,
            HU, HD, HL, HR,
            Start, Back,

            none,
            size
        };

        //PlayerIndex ControllerID = 0;

        //GamePadState CurrentState;

        ControllerAxies[] InputAxies;

        delegate float del(GamePadState pad);
        static del[] PadCalls;


        //class VibeState
        //{
        //    public VibeState(float sL, float sH, float t) { scaleL = sL; scaleH = sH; time = t; }
        //    public float scaleL, scaleH, time;
        //    public void Update(float delta) { this.time -= delta; }
        //}

        //List<VibeState> vibes;

        Controller controller;

        public ControllerInputNode(Controller con, ControllerAxies[] axies)
        {
            //ControllerID = (PlayerIndex)id;
            controller = con;
            InputAxies = axies;

            if (PadCalls == null)
            {
                PadCalls = new del[(int)ControllerAxies.size];

                PadCalls[(int)ControllerAxies.none] = pad => 0f;

                PadCalls[(int)ControllerAxies.LTX] = pad => pad.ThumbSticks.Left.X;
                PadCalls[(int)ControllerAxies.LTY] = pad => pad.ThumbSticks.Left.Y;

                PadCalls[(int)ControllerAxies.RTX] = pad => pad.ThumbSticks.Right.X;
                PadCalls[(int)ControllerAxies.RTY] = pad => pad.ThumbSticks.Right.Y;

                PadCalls[(int)ControllerAxies.LTr] = pad => pad.Triggers.Left;
                PadCalls[(int)ControllerAxies.RTr] = pad => pad.Triggers.Right;

                PadCalls[(int)ControllerAxies.A] = pad => pad.Buttons.A == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.B] = pad => pad.Buttons.B == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.X] = pad => pad.Buttons.X == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.Y] = pad => pad.Buttons.Y == ButtonState.Pressed ? 1f : 0f;

                PadCalls[(int)ControllerAxies.Start] = pad => pad.Buttons.Start == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.Back] = pad => pad.Buttons.Back == ButtonState.Pressed ? 1f : 0f;

                PadCalls[(int)ControllerAxies.LTZ] = pad => pad.Buttons.LeftStick == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.RTZ] = pad => pad.Buttons.RightStick == ButtonState.Pressed ? 1f : 0f;

                PadCalls[(int)ControllerAxies.LB] = pad => pad.Buttons.LeftShoulder == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.RB] = pad => pad.Buttons.RightShoulder == ButtonState.Pressed ? 1f : 0f;

                PadCalls[(int)ControllerAxies.HU] = pad => pad.DPad.Up == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.HD] = pad => pad.DPad.Down == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.HL] = pad => pad.DPad.Left == ButtonState.Pressed ? 1f : 0f;
                PadCalls[(int)ControllerAxies.HR] = pad => pad.DPad.Right == ButtonState.Pressed ? 1f : 0f;

            }

            //vibes = new List<VibeState>();
        }

        public override float GetAxis(InputAxies axis)
        {
            if (controller.CurrentState.IsConnected != true) return 0f;

            try
            {
                return PadCalls[(int)InputAxies[(int)axis]](controller.CurrentState);
            }
            catch (Exception exc)
            {
                Debug.LogError(string.Format("Fault in getting axis state {0} for gamepad {1}!", axis, controller));
                Debug.LogError(exc.Message);
            }

            // only if the above try fails
            return 0f;
        }

        //public override void Update()
        //{
        //    CurrentState = GamePad.GetState(ControllerID);
        //    base.Update();
        //
        //    float L = 0f, H = 0f;
        //    for(int i = 0; i < vibes.Count;)
        //    {
        //        L += vibes[i].scaleL;
        //        H += vibes[i].scaleH;
        //        vibes[i].Update(Time.deltaTime);
        //        if (vibes[i].time <= 0f)
        //            vibes.RemoveAt(i);
        //        else
        //            ++i;
        //    }
        //    GamePad.SetVibration(ControllerID, Mathf.Min(L, 1f), Mathf.Min(H, 1f));
        //}

        public override void ForceFeedback(float scaleL, float scaleH, float time = 0f)
        {
            //GamePad.SetVibration(ControllerID, CurrentState.Triggers.Left, CurrentState.Triggers.Right);
            //vibes.Add(new VibeState(scaleL, scaleH, time));
            controller.AddVibe(new Controller.VibeState(scaleL, scaleH, time));
        }
    }

    


    static List<InputNode> CurrentNodes;

    static List<Controller> Controllers;

    void Awake()
    {

        string[] names = Input.GetJoystickNames();
        foreach(string name in names)
            Debug.Log(name);

        CurrentNodes = new List<InputNode>();

        Controllers = new List<Controller>();
        for (int i = 0; i < 4; ++i)
            Controllers.Add(new Controller((PlayerIndex)i));
	}


    //void OnMouseDown() { Screen.lockCursor = true; }
    //void OnApplicationFocus(bool focusStatus) { Screen.lockCursor = focusStatus; }

    //bool wasLocked = false;

    void Update()
    {
        foreach (Controller con in Controllers)
            con.Update();

        foreach (InputNode handle in CurrentNodes)
            handle.Update();


        //if (Input.GetKeyDown("escape"))
        //    Screen.lockCursor = false;
        //
        //if (!Screen.lockCursor && wasLocked)
        //{
        //    wasLocked = false;
        //    Debug.Log("Unlocking cursor");
        //}
        //else
        //    if (Screen.lockCursor && !wasLocked)
        //    {
        //        wasLocked = true;
        //        Debug.Log("Locking cursor");
        //    }

        //GamePad.SetVibration(0, Mathf.Sin(Time.time*5f), Mathf.Cos(Time.time*5f));
	}




    /// <summary>
    /// makes a new input node that uses keyboard keys
    /// </summary>
    /// <param name="keybinds">array of size of InputAxies containing the key structs for each</param>
    /// <returns>the new node</returns>
    public static InputNode NewKeyboardNode(KeyboardInputNode.KeyAxie[] keybinds)
    {
        InputNode node = new KeyboardInputNode(keybinds);
        CurrentNodes.Add(node);
        return node;
    }

    /// <summary>
    /// makes a new input node that uses a controller map
    /// </summary>
    /// <param name="id">controller number to use (1-4)</param>
    /// <param name="axieBinds">array of size of InputAxies containing the ControllerAxies for each</param>
    /// <returns>the new node</returns>
    public static InputNode NewControllerNode(int id, ControllerInputNode.ControllerAxies[] axieBinds)
    {
        InputNode node = new ControllerInputNode(Controllers[id], axieBinds);
        CurrentNodes.Add(node);
        return node;
    }

    /// <summary>
    /// destroys a node
    /// </summary>
    /// <param name="node"></param>
    public static void DestroyControllerNode(InputNode node)
    {
        CurrentNodes.Remove(node);
    }



    public enum KeyboardMaps
    {
        WASDQE, TFGHRY, IJKLUO, NUMPAD
    };

    public static KeyboardInputNode.KeyAxie[] GetKeyboardMap(KeyboardMaps map)
    {
        KeyboardInputNode.KeyAxie[] axies = new KeyboardInputNode.KeyAxie[(int)InputAxies.size];

        for (int i = 0; i < axies.Length; ++i)
            axies[i] = new KeyboardInputNode.KeyAxie(); //just as a precaution in early dev, shouldnt be needed persay

        switch (map)
        {
            case KeyboardMaps.WASDQE:

                axies[(int)InputAxies.MOVEX] = new KeyboardInputNode.TwoKeyAxie(KeyCode.D,KeyCode.A);
                axies[(int)InputAxies.MOVEY] = new KeyboardInputNode.TwoKeyAxie(KeyCode.W, KeyCode.S);

                axies[(int)InputAxies.ACTION1] = new KeyboardInputNode.OneKeyAxie(KeyCode.Q);
                axies[(int)InputAxies.ACTION2] = new KeyboardInputNode.OneKeyAxie(KeyCode.E);

                break;

            case KeyboardMaps.TFGHRY:

                axies[(int)InputAxies.MOVEX] = new KeyboardInputNode.TwoKeyAxie(KeyCode.H, KeyCode.F);
                axies[(int)InputAxies.MOVEY] = new KeyboardInputNode.TwoKeyAxie(KeyCode.T, KeyCode.G);

                axies[(int)InputAxies.ACTION1] = new KeyboardInputNode.OneKeyAxie(KeyCode.R);
                axies[(int)InputAxies.ACTION2] = new KeyboardInputNode.OneKeyAxie(KeyCode.Y);

                break;

            case KeyboardMaps.IJKLUO:

                axies[(int)InputAxies.MOVEX] = new KeyboardInputNode.TwoKeyAxie(KeyCode.L, KeyCode.J);
                axies[(int)InputAxies.MOVEY] = new KeyboardInputNode.TwoKeyAxie(KeyCode.I, KeyCode.K);

                axies[(int)InputAxies.ACTION1] = new KeyboardInputNode.OneKeyAxie(KeyCode.U);
                axies[(int)InputAxies.ACTION2] = new KeyboardInputNode.OneKeyAxie(KeyCode.O);

                break;

            case KeyboardMaps.NUMPAD:

                axies[(int)InputAxies.MOVEX] = new KeyboardInputNode.TwoKeyAxie(KeyCode.Keypad6, KeyCode.Keypad4);
                axies[(int)InputAxies.MOVEY] = new KeyboardInputNode.TwoKeyAxie(KeyCode.Keypad8, KeyCode.Keypad5);

                axies[(int)InputAxies.ACTION1] = new KeyboardInputNode.OneKeyAxie(KeyCode.Keypad7);
                axies[(int)InputAxies.ACTION2] = new KeyboardInputNode.OneKeyAxie(KeyCode.Keypad9);

                break;

            default:
                Debug.LogWarning("NO KEYS ASSIGNED");
                break;
        }

        return axies;
    }


    public enum ControllerMaps
    {
        DEFAULT, LEFTHAND, RIGHTHAND,
    };

    public static ControllerInputNode.ControllerAxies[] GetControllerMap(ControllerMaps map)
    {
        ControllerInputNode.ControllerAxies[] axies = new ControllerInputNode.ControllerAxies[(int)InputAxies.size];

        for (int i = 0; i < axies.Length; ++i)
            axies[i] = ControllerInputNode.ControllerAxies.none;

        switch (map)
        {
            case ControllerMaps.DEFAULT:

                axies[(int)InputAxies.MOVEX] = ControllerInputNode.ControllerAxies.LTX;
                axies[(int)InputAxies.MOVEY] = ControllerInputNode.ControllerAxies.LTY;

                axies[(int)InputAxies.ACTION1] = ControllerInputNode.ControllerAxies.A;
                axies[(int)InputAxies.ACTION2] = ControllerInputNode.ControllerAxies.B;

                break;

            case ControllerMaps.LEFTHAND:

                axies[(int)InputAxies.MOVEX] = ControllerInputNode.ControllerAxies.LTX;
                axies[(int)InputAxies.MOVEY] = ControllerInputNode.ControllerAxies.LTY;

                axies[(int)InputAxies.ACTION1] = ControllerInputNode.ControllerAxies.LTr;
                axies[(int)InputAxies.ACTION2] = ControllerInputNode.ControllerAxies.LB;

                break;

            case ControllerMaps.RIGHTHAND:

                axies[(int)InputAxies.MOVEX] = ControllerInputNode.ControllerAxies.RTX;
                axies[(int)InputAxies.MOVEY] = ControllerInputNode.ControllerAxies.RTY;

                axies[(int)InputAxies.ACTION1] = ControllerInputNode.ControllerAxies.RTr;
                axies[(int)InputAxies.ACTION2] = ControllerInputNode.ControllerAxies.RB;

                break;

            default:
                break;
        }

        return axies;
    }


    
}
