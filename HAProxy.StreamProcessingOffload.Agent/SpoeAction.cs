namespace HAProxy.StreamProcessingOffload.Agent
{
    public abstract class SpoeAction
    {
        /// <summary>
        /// Initializes a new instance of the SpoeAction class. Actions allow
        /// the agent to tell HAProxy to perform an operation, such as setting a variable.
        /// </summary>
        /// <param name="type">The type of action</param>
        protected SpoeAction(ActionType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the bytes for the action.
        /// </summary>
        public abstract byte[] Bytes { get; }

        /// <summary>
        /// Gets the type of action.
        /// </summary>
        public ActionType Type { get; private set; }
    }
}