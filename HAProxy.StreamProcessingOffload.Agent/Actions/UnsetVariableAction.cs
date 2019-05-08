//-----------------------------------------------------------------------
// <copyright file="UnsetVariableAction.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent.Actions
{
    public class UnsetVariableAction : SpoeAction
    {
        private const byte NUMBER_OF_ARGS = 2;
        private VariableScope variableScope;
        private string variableName;

        /// <summary>
        /// Initializes a new instance of the UnsetVariableAction class.
        /// </summary>
        /// <param name="variableScope">The scope of the variable to unset</param>
        /// <param name="variableName">The name of the variable to unset</param>
        public UnsetVariableAction(VariableScope variableScope, string variableName) : base(ActionType.UnsetVar)
        {
            this.variableScope = variableScope;
            this.variableName = variableName;
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
            sb.AppendLine(this.variableName);
            return sb.ToString();
        }
    }
}