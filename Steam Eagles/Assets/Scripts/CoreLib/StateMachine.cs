using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StateMachine
{
    public class StateMachine<T>
    {
        private IState<T> _defaultState = null;
        private IState<T> _currentState = null;
        private IState<T> _previousState = null;
        private Dictionary<IState<T>, List<Transition>> states = new Dictionary<IState<T>, List<Transition>>();
        private List<Transition> anyStateTransitions = new List<Transition>();

        public enum CheckMode
        {
            Manual = 0,
            Tick = 1 << 1,
            FixedTick = 1 << 2,
            Always = (1 << 2) | (1 << 1)
        }

        public CheckMode TransitionCheckRate { get; set; } = CheckMode.Tick;

        private bool ShouldCheckTransition(CheckMode mode) => ((int) mode & (int)TransitionCheckRate) > 0;


      

        public IState<T> DefaultState
        {
            get => _defaultState;
            set => _defaultState = value;
        }

        public void AddState(IState<T> state)
        {
            if (state == null) return;
            if(states.ContainsKey(state))
                return;
            if (_defaultState == null) _defaultState = state;
            states.Add(state, new List<Transition>());
        }

        public void AddTransition(IState<T> from, IState<T> to, Func<T, bool> condition)
        {
            if (from == null || to == null) return;
            states[from].Add(new Transition(from, to, condition));
        }

        public void AddAnyTransition(IState<T> to, Func<T,bool> condition)
        {
            anyStateTransitions.Add(new Transition(null, to, condition));
        }
        
        public void AddTransition(IState<T> to, Func<T, bool> condition, params IState<T>[] from)
        {
            
            foreach (var f in from)
            {
                AddTransition(f, to, condition);
            }
        }

        public void ChangeState(IState<T> newState, T actor)
        {
            if (_currentState != newState)
            {
                _currentState?.OnExit(actor);
                _previousState = _currentState;
                newState.OnEnter(actor);
                _currentState = newState;
            }
        }
        
        public void Update(T actor)
        {
            if (_currentState == null)
            {
                _currentState = _defaultState;
                Update(actor);
            }
            if (ShouldCheckTransition(CheckMode.Tick))
                CheckForStateChanged(actor);
            _currentState?.Tick(actor);
        }

        private bool CheckForStateChanged(T actor)
        {
            var transitions = states[_currentState].Concat(anyStateTransitions);

            foreach (var transition in transitions)
            {
                if (transition.to != _currentState  && transition.Condition(actor))
                {
                    Debug.Log("Changing to: " + transition.to.GetType());
                    ChangeState(transition.to, actor);
                    return true;
                }
            }

            return false;
        }

        public void FixedUpdate(T actor)
        {
            if (ShouldCheckTransition(CheckMode.FixedTick))
                CheckForStateChanged(actor);
            _currentState?.FixedTick(actor);
        }
        
        
        public class Transition
        {
            public IState<T> from, to;
            public Func<T, bool> Condition;

            public Transition(IState<T> @from, IState<T> to, Func<T, bool> condition)
            {
                this.from = @from;
                this.to = to;
                Condition = condition;
            }
        }
        
        public void RunTests()
        {
            TransitionCheckRate = CheckMode.Tick;
            Debug.Assert(ShouldCheckTransition(CheckMode.Tick));
            Debug.Assert(!ShouldCheckTransition(CheckMode.FixedTick));
            TransitionCheckRate = CheckMode.FixedTick;
            Debug.Assert(!ShouldCheckTransition(CheckMode.Tick));
            Debug.Assert(ShouldCheckTransition(CheckMode.FixedTick));
            TransitionCheckRate = CheckMode.Always;
            Debug.Assert(ShouldCheckTransition(CheckMode.Tick));
            Debug.Assert(ShouldCheckTransition(CheckMode.FixedTick));
            TransitionCheckRate = CheckMode.Manual;
            Debug.Assert(!ShouldCheckTransition(CheckMode.Tick));
            Debug.Assert(!ShouldCheckTransition(CheckMode.FixedTick));
        }
    }
}