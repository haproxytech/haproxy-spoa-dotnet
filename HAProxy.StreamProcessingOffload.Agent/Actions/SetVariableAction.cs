using System.Collections.Generic;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent.Actions
{
    public class SetVariableAction : SpoeAction
    {
        private const byte NUMBER_OF_ARGS = 3;
        private VariableScope variableScope;
        private string variableName;
        private TypedData value;

        /// <summary>
        /// Initializes a new instance of the SetVariableAction class.
        /// </summary>
        /// <param name="variableScope">The scope of the variable to set</param>
        /// <param name="variableName">The name of the variable to set</param>
        /// <param name="value">The value of the variable to set</param>
        public SetVariableAction(VariableScope variableScope, string variableName, TypedData value) : base(ActionType.SetVar)
        {
            this.variableScope = variableScope;
            this.variableName = variableName;
            this.value = value;
        }

        /// <summary>
        /// Gets the bytes for this action.
        /// </summary>
        public override byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();
                bytes.Add((byte)this.Type);
                bytes.Add(NUMBER_OF_ARGS);

                // argument 1: variable scope
                bytes.Add((byte)this.variableScope);

                // argument 2: variable name
                VariableInt lengthOfName = VariableInt.EncodeVariableInt(this.variableName.Length);
                bytes.AddRange(lengthOfName.Bytes);
                bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(this.variableName));

                // argument 3: variable value
                bytes.AddRange(this.value.Bytes);

                return bytes.ToArray();
            }
        }

        /// <summary>
        /// Gets a string representing the action.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("(action) {0}", this.Type.ToString()));
            sb.AppendLine(string.Format("{0} = {1}", this.variableName, this.value.ToString()));
            return sb.ToString();
        }
    }
}