using System;
using UnityEngine;

namespace StateMachine
{
    public class StateMachineEvents: MonoBehaviour
    {
        public static StateMachineEvents Instance { get; private set; }
        
        public Action updateAction;
        public Action fixedUpdateAction;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            updateAction?.Invoke();
        }

        private void FixedUpdate()
        {
            fixedUpdateAction?.Invoke();
        }
    }
}