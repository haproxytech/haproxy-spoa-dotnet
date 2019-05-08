//-----------------------------------------------------------------------
// <copyright file="SpoeAction.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
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