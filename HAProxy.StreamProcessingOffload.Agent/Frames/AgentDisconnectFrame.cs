//-----------------------------------------------------------------------
// <copyright file="AgentDisconnectFrame.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class AgentDisconnectFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the AgentDisconnectFrame class. The agent sends
        /// a Disconnect frame when it wants to stop communicating with HAProxy.
        /// </summary>
        /// <param name="status">The status when disconnecting</param>
        /// <param name="message">The message to include when disconnecting</param>
        public AgentDisconnectFrame(Status status, string message) : base(FrameType.AgentDisconnect)
        {
            this.Metadata.Flags.Fin = true;
            this.Metadata.Flags.Abort = false;
            this.Metadata.StreamId = VariableInt.EncodeVariableInt(0);
            this.Metadata.FrameId = VariableInt.EncodeVariableInt(0);

            var payload = new KeyValueListPayload();
            payload.KeyValueItems.Add("status-code", new TypedData(DataType.Uint32, (uint)status));
            payload.KeyValueItems.Add("message", new TypedData(DataType.String, message));
            this.Payload = payload;
            this.Status = status;
        }
    }
}