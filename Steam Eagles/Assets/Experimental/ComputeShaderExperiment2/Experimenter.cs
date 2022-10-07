using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Experimental.ComputeShaderExperiment2
{
    public abstract class Experimenter : MonoBehaviour
    {
        class Button
        {
            private readonly string _name;
            private readonly Action _onPressed;

            public Button(string name, Action onPressed)
            {
                _name = name;
                _onPressed = onPressed;
            }

            public void Draw(Rect rect)
            {
                if (GUI.Button(rect, _name))
                {
                    _onPressed?.Invoke();
                }
            }
        }

        private List<Button> _buttons;

        protected abstract IEnumerable<(string, Action)> GetButtonActions();

        protected abstract void Init();


        protected virtual bool isInited
        {
            get;
            set;
        }


        private void OnDestroy()
        {
            isInited = false;
        }


        private void OnGUI()
        {
            var w = 1000;
            var h = 50;
            Rect buttonRect = new Rect(0, 0, w, h);
            if (!isInited)
            {
                if (GUI.Button(buttonRect, $"Initialize {GetType().Name}"))
                {
                    Init();
                    _buttons = GetButtonActions().Select(t => new Button(t.Item1, t.Item2)).ToList();
                    isInited = true;
                }
            }
            else
            {
                foreach (var button in _buttons)
                {
                    button.Draw(buttonRect);
                    buttonRect.y += h;
                }
            }
        }
    }
}