namespace PsyPuppets.Utils.Automata
{
    /// <summary>
    /// Represents a state to be used in a state machine.
    /// </summary>
    /// <typeparam name="TMachine">The type of the machine.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IState<TMachine, TContext>
    {
        /// <summary>
        /// The state machine this state belongs to.
        /// </summary>
        TMachine Machine { get; set; }

        /// <summary>
        /// The context for this state.
        /// </summary>
        TContext Context { get; set; }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void OnBegin();

        /// <summary>
        /// Called every time the state machine updates.
        /// </summary>
        void OnAct(float deltaTime);

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void OnEnd();

    }
}