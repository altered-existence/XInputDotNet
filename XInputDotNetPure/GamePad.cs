using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace XInputDotNetPure
{
    class Imports
    {
        internal const string DLLName = "XInputInterface";

        [DllImport(DLLName)]
        public static extern uint XInputGamePadGetState(uint playerIndex, out GamePadState.RawState state);
        [DllImport(DLLName)]
        public static extern void XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor);
    }

    public enum ButtonCode
    {
        A = 0,
        B,
        X,
        Y,
        ShoulderRight,
        ShoulderLeft,
        ThumbRight,
        ThumbLeft,
        Back,
        Start,
        Guide,
        DpadUp,
        DpadDown,
        DpadRight,
        DpadLeft
    }

    public enum AxisCode
    {
        StickLeft,
        StickRight
    }

    public enum TriggerCode
    {
        TriggerLeft,
        TriggerRight
    }

    public struct Axis
    {
        float x;
        float y;

        public Axis(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X {
            get {return x; }
        }

        public float Y {
            get { return y; }
        }
    }

    public enum PlayerIndex
    {
        One = 0,
        Two,
        Three,
        Four
    }

    public enum GamePadDeadZone
    {
        Circular,
        IndependentAxes,
        None
    }

    public class GamePadManager
    {
        //Later research if you can have more then 4 players on one machine

        GamePadState[] currentStates;
        GamePadState[] lastStates;
        const int controllersMax = 4;

        public GamePadManager()
        {
            currentStates = new GamePadState[controllersMax];
            lastStates = new GamePadState[controllersMax];

            for (int i = 0; i < controllersMax; i++)
            {
                currentStates[i] = new GamePadState();
                lastStates[i] = new GamePadState();
            }
        }

        //Pole for all indexs
        public void Pole()
        {
            for (int i = 0; i < controllersMax; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                lastStates[i].Set(currentStates[i]);
                currentStates[i].Update(testPlayerIndex);
            }
        }

        //Pole for a specific player index
        public void Pole(PlayerIndex playerIndex)
        {
            int index = (int)playerIndex;
            lastStates[index].Set(currentStates[index]);
            currentStates[index].Update(playerIndex);
        }

        public bool IsControllerConnected(PlayerIndex playerIndex)
        {
            return currentStates[(int)playerIndex].IsConnected;
        }

        public static void SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            Imports.XInputGamePadSetState((uint)playerIndex, leftMotor, rightMotor);
        }

        public bool GetButton(PlayerIndex index, ButtonCode button)
        {
            return currentStates[(int)index].GetButton(button);
        }

        public bool GetButtonDown(PlayerIndex index, ButtonCode button)
        {
            return lastStates[(int)index].GetButton(button) == false && GetButton(index, button) == true;
        }

        public bool GetButtonUp(PlayerIndex index, ButtonCode button)
        {
            return lastStates[(int)index].GetButton(button) == true && GetButton(index, button) == false;
        }

        public Axis GetAxis(PlayerIndex index, AxisCode axis)
        {
            return currentStates[(int)index].GetAxis(axis);
        }

        public float GetTrigger(PlayerIndex index, TriggerCode trigger)
        {
            return currentStates[(int)index].GetTrigger(trigger);
        }
    }

    public class GamePadState
    {
        Dictionary<ButtonCode, bool> _buttons;
        Dictionary<AxisCode, Axis> _axis;
        Dictionary<TriggerCode, float> _triggers;


        [StructLayout(LayoutKind.Sequential)]
        internal struct RawState
        {
            public uint dwPacketNumber;
            public GamePad Gamepad;

            [StructLayout(LayoutKind.Sequential)]
            public struct GamePad
            {
                public ushort wButtons;
                public byte bLeftTrigger;
                public byte bRightTrigger;
                public short sThumbLX;
                public short sThumbLY;
                public short sThumbRX;
                public short sThumbRY;
            }
        }

        bool _isConnected;
        uint _packetNumber;


        public uint PacketNumber
        {
            get { return _packetNumber; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        enum ButtonsConstants
        {
            DPadUp = 0x00000001,
            DPadDown = 0x00000002,
            DPadLeft = 0x00000004,
            DPadRight = 0x00000008,
            Start = 0x00000010,
            Back = 0x00000020,
            LeftThumb = 0x00000040,
            RightThumb = 0x00000080,
            LeftShoulder = 0x0100,
            RightShoulder = 0x0200,
            Guide = 0x0400,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000
        }

        internal GamePadState()
        {
            _buttons = new Dictionary<ButtonCode, bool>();
            _axis = new Dictionary<AxisCode, Axis>();
            _triggers = new Dictionary<TriggerCode, float>();


            RawState state;
            uint result = Imports.XInputGamePadGetState((uint)PlayerIndex.One, out state);
            Init(result == Utils.Success, state, GamePadDeadZone.IndependentAxes);
        }

        internal void Init(bool isConnected, RawState rawState, GamePadDeadZone deadZone)
        {
            this._isConnected = isConnected;

            if (!isConnected)
            {
                rawState.dwPacketNumber = 0;
                rawState.Gamepad.wButtons = 0;
                rawState.Gamepad.bLeftTrigger = 0;
                rawState.Gamepad.bRightTrigger = 0;
                rawState.Gamepad.sThumbLX = 0;
                rawState.Gamepad.sThumbLY = 0;
                rawState.Gamepad.sThumbRX = 0;
                rawState.Gamepad.sThumbRY = 0;
            }

            _packetNumber = rawState.dwPacketNumber;

            _buttons = new Dictionary<ButtonCode, bool>();
            _buttons.Add(ButtonCode.Start, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Start) != 0 ? true : false);
            _buttons.Add(ButtonCode.Back, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Back) != 0 ? true : false);
            _buttons.Add(ButtonCode.ThumbLeft, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftThumb) != 0 ? true : false);
            _buttons.Add(ButtonCode.ThumbRight, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightThumb) != 0 ? true : false);
            _buttons.Add(ButtonCode.ShoulderLeft, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftShoulder) != 0 ? true : false);
            _buttons.Add(ButtonCode.ShoulderRight, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightShoulder) != 0 ? true : false);
            _buttons.Add(ButtonCode.Guide, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Guide) != 0 ? true : false);

            _buttons.Add(ButtonCode.A, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.A) != 0 ? true : false);
            _buttons.Add(ButtonCode.B, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.B) != 0 ? true : false);
            _buttons.Add(ButtonCode.X, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.X) != 0 ? true : false);
            _buttons.Add(ButtonCode.Y, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Y) != 0 ? true : false);


            _buttons.Add(ButtonCode.DpadUp, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadUp) != 0 ? true : false);
            _buttons.Add(ButtonCode.DpadDown, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadDown) != 0 ? true : false);
            _buttons.Add(ButtonCode.DpadLeft, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadLeft) != 0 ? true : false);
            _buttons.Add(ButtonCode.DpadRight, (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadRight) != 0 ? true : false);

            _axis = new Dictionary<AxisCode, Axis>();
            _axis.Add(AxisCode.StickLeft, Utils.ApplyLeftStickDeadZone(rawState.Gamepad.sThumbLX, rawState.Gamepad.sThumbLY, deadZone));
            _axis.Add(AxisCode.StickRight, Utils.ApplyRightStickDeadZone(rawState.Gamepad.sThumbRX, rawState.Gamepad.sThumbRY, deadZone));

            _triggers = new Dictionary<TriggerCode, float>();
            _triggers.Add(TriggerCode.TriggerLeft, Utils.ApplyTriggerDeadZone(rawState.Gamepad.bLeftTrigger, deadZone));
            _triggers.Add(TriggerCode.TriggerRight, Utils.ApplyTriggerDeadZone(rawState.Gamepad.bRightTrigger, deadZone));
        }

        public void Update(PlayerIndex index)
        {
            Update(index, GamePadDeadZone.IndependentAxes);
        }

        public void Update(PlayerIndex index, GamePadDeadZone deadZone)
        {
            RawState state;
            uint result = Imports.XInputGamePadGetState((uint)index, out state);
            UpdateState(result == Utils.Success, state, deadZone);
        }

        void UpdateState(bool isConnected, RawState rawState, GamePadDeadZone deadZone)
        {
            this._isConnected = isConnected;

            if (!isConnected)
            {
                rawState.dwPacketNumber = 0;
                rawState.Gamepad.wButtons = 0;
                rawState.Gamepad.bLeftTrigger = 0;
                rawState.Gamepad.bRightTrigger = 0;
                rawState.Gamepad.sThumbLX = 0;
                rawState.Gamepad.sThumbLY = 0;
                rawState.Gamepad.sThumbRX = 0;
                rawState.Gamepad.sThumbRY = 0;
            }

            _packetNumber = rawState.dwPacketNumber;

            _buttons[ButtonCode.Start] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Start) != 0 ? true : false;
            _buttons[ButtonCode.Back] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Back) != 0 ? true : false;
            _buttons[ButtonCode.ThumbLeft] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftThumb) != 0 ? true : false;
            _buttons[ButtonCode.ThumbRight] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightThumb) != 0 ? true : false;
            _buttons[ButtonCode.ShoulderLeft] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftShoulder) != 0 ? true : false;
            _buttons[ButtonCode.ShoulderRight] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightShoulder) != 0 ? true : false;
            _buttons[ButtonCode.Guide] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Guide) != 0 ? true : false;

            _buttons[ButtonCode.A] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.A) != 0 ? true : false;
            _buttons[ButtonCode.B] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.B) != 0 ? true : false;
            _buttons[ButtonCode.X] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.X) != 0 ? true : false;
            _buttons[ButtonCode.Y] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Y) != 0 ? true : false;


            _buttons[ButtonCode.DpadUp] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadUp) != 0 ? true : false;
            _buttons[ButtonCode.DpadDown] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadDown) != 0 ? true : false;
            _buttons[ButtonCode.DpadLeft] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadLeft) != 0 ? true : false;
            _buttons[ButtonCode.DpadRight] = (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadRight) != 0 ? true : false;

            _axis[AxisCode.StickLeft] = Utils.ApplyLeftStickDeadZone(rawState.Gamepad.sThumbLX, rawState.Gamepad.sThumbLY, deadZone);
            _axis[AxisCode.StickRight] = Utils.ApplyRightStickDeadZone(rawState.Gamepad.sThumbRX, rawState.Gamepad.sThumbRY, deadZone);

            _triggers[TriggerCode.TriggerLeft] = Utils.ApplyTriggerDeadZone(rawState.Gamepad.bLeftTrigger, deadZone);
            _triggers[TriggerCode.TriggerRight] = Utils.ApplyTriggerDeadZone(rawState.Gamepad.bRightTrigger, deadZone);
        }

        public void Set(GamePadState other)
        {
            _isConnected = other._isConnected;
            _packetNumber = other._packetNumber;

            for(int i = 0; i < _buttons.Count; i++)
            {
                _buttons[(ButtonCode)i] = other._buttons[(ButtonCode)i];
            }
        }
        public bool GetButton(ButtonCode button)
        {
            return _buttons[button];
        }

        public Axis GetAxis(AxisCode axis)
        {
            return _axis[axis];
        }

        public float GetTrigger(TriggerCode trigger)
        {
            return _triggers[trigger];
        }
    }
}