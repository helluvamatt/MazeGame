using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MazeGame
{
    internal class KeyboardManager
    {
        private static readonly TimeSpan KEY_DELAY = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan KEY_REPEAT = TimeSpan.FromSeconds(1.0 / 20.0); // 20hz

        private readonly Dictionary<Keys, PressedKey> _PressedKeys;
        private readonly List<IKeyListener> _Listeners;
        private readonly Dictionary<Keys, List<Action<bool>>> _KeyDownListeners;
        private readonly Dictionary<Keys, List<Action>> _KeyUpListeners;
        private readonly Dictionary<Keys, List<Action>> _KeyPressListeners;

        private KeyboardState _LastKeyboardState;

        public KeyboardManager()
        {
            _PressedKeys = new Dictionary<Keys, PressedKey>();
            _Listeners = new List<IKeyListener>();
            _KeyDownListeners = new Dictionary<Keys, List<Action<bool>>>();
            _KeyUpListeners = new Dictionary<Keys, List<Action>>();
            _KeyPressListeners = new Dictionary<Keys, List<Action>>();
        }

        public void RegisterKeyDown(Action<bool> action, params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (!_KeyDownListeners.TryGetValue(key, out List<Action<bool>> actions))
                {
                    actions = new List<Action<bool>>();
                    _KeyDownListeners.Add(key, actions);
                }
                actions.Add(action);
            }
        }

        public void RegisterKeyUp(Action action, params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (!_KeyUpListeners.TryGetValue(key, out List<Action> actions))
                {
                    actions = new List<Action>();
                    _KeyUpListeners.Add(key, actions);
                }
                actions.Add(action);
            }
        }

        public void RegisterKeyPress(Action action, params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (!_KeyPressListeners.TryGetValue(key, out List<Action> actions))
                {
                    actions = new List<Action>();
                    _KeyPressListeners.Add(key, actions);
                }
                actions.Add(action);
            }
        }

        public void RegisterListener(IKeyListener keyListener)
        {
            _Listeners.Add(keyListener);
        }

        public void HandleKeys(GameTime gameTime, KeyboardState keyState)
        {

            var keysUp = new HashSet<Keys>(_LastKeyboardState.GetPressedKeys());
            keysUp.ExceptWith(keyState.GetPressedKeys());

            var keysDown = new HashSet<Keys>(keyState.GetPressedKeys());
            keysDown.ExceptWith(_LastKeyboardState.GetPressedKeys());

            foreach (var key in keysUp)
            {
                _Listeners.ForEach(l => l.KeyUp(key));
                InvokeListener(_KeyUpListeners, key);
                if (_PressedKeys.Remove(key))
                {
                    _Listeners.ForEach(l => l.KeyPress(key));
                    InvokeListener(_KeyPressListeners, key);
                }
            }
            foreach (var key in keysDown)
            {
                _PressedKeys[key] = new PressedKey(key, gameTime.TotalGameTime);
                InvokeKeyDown(key, false);
            }

            // Simulate key repeat
            var delayThreshold = gameTime.TotalGameTime.Subtract(KEY_DELAY);
            var repeatThreshold = gameTime.TotalGameTime.Subtract(KEY_REPEAT);
            var pressedKeys = new List<PressedKey>(_PressedKeys.Values);
            foreach (var pressedKey in pressedKeys)
            {
                if (pressedKey.ProcessRepeat(gameTime.TotalGameTime, delayThreshold, repeatThreshold))
                {
                    InvokeKeyDown(pressedKey.Key, true);
                }
            }

            _LastKeyboardState = keyState;
        }

        private void InvokeListener(Dictionary<Keys, List<Action>> listenerCollection, Keys key)
        {
            if (listenerCollection.TryGetValue(key, out List<Action> actions))
            {
                foreach (var action in actions)
                {
                    action.Invoke();
                }
            }
        }

        private void InvokeKeyDown(Keys key, bool repeat)
        {
            _Listeners.ForEach(l => l.KeyDown(key, repeat));
            if (_KeyDownListeners.TryGetValue(key, out List<Action<bool>> actions)) actions.ForEach(a => a.Invoke(repeat));
        }

        private class PressedKey
        {
            private bool _IsPastDelay;

            public PressedKey(Keys key, TimeSpan keyDown)
            {
                Key = key;
                LastRepeat = KeyDown = keyDown;
            }

            public Keys Key { get; }

            public TimeSpan KeyDown { get; }

            public TimeSpan LastRepeat { get; private set; }

            public bool ProcessRepeat(TimeSpan now, TimeSpan delayThreshold, TimeSpan repeatThreshold)
            {
                if (_IsPastDelay && LastRepeat < repeatThreshold)
                {
                    LastRepeat = now;
                    return true;
                }
                else if (LastRepeat < delayThreshold)
                {
                    _IsPastDelay = true;
                    LastRepeat = now;
                    return true;
                }
                return false;
            }
        }
    }

    internal interface IKeyListener
    {
        void KeyDown(Keys key, bool repeat);
        void KeyUp(Keys key);
        void KeyPress(Keys key);
    }
}
