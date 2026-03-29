namespace StateMachine
{
    public interface IState
    {
        ITag Tag { get; }
        
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
        
        
    }
}