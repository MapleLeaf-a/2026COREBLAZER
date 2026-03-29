using System;
using UnityEngine;

namespace StateMachine
{
    public class StateManager<T> where T : class,ITag
    {
        private IState _currentState;
        

        private StateManager(IState startState)
        {
            if (startState == null)
            {
                throw new UnityException("startState is null");
            }
            if (startState.Tag is not T)
            {
                throw new UnityException("startState.Tag is not " + typeof(T).FullName);
            }
            StartStateMachine(startState);
        }

        private void StartStateMachine(IState state)
        {
            state.OnEnter();
            _currentState = state;
            StateMachineEvents.Instance.updateAction+= state.OnUpdate;
            StateMachineEvents.Instance.fixedUpdateAction+= state.OnFixedUpdate;
        }

        private void EndStateMachine()
        {
            StateMachineEvents.Instance.updateAction-= _currentState.OnUpdate;
            StateMachineEvents.Instance.fixedUpdateAction-= _currentState.OnFixedUpdate;
            _currentState.OnExit();
            _currentState = null;
        }
        
        public void ChangeState(IState newState)
        {
            if (newState.Tag is not T)
            {
                throw new UnityException("newState.Tag is not " + typeof(T).FullName);
            }
            EndStateMachine();
            StartStateMachine(newState);
        }
    }
}