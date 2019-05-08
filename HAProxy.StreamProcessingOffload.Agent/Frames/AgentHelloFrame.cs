//-----------------------------------------------------------------------
// <copyright file="AgentHelloFrame.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class AgentHelloFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the AgentHelloFrame. The agent sends a HELLO frame
        /// during the initial handshake with HAProxy.
        /// </summary>
        /// <param name="supportedSpopVersion">The SPOP version that the agent supports</param>
        /// <param name="maxFrameSize">The max size of a frame supported</param>
        public AgentHelloFrame(string supportedSpopVersion, uint maxFrameSize, string[] capabilities) : base(FrameType.AgentHello)
        {
            this.Metadata.Flags.Fin = true;
            this.Metadata.Flags.Abort = false;
            this.Metadata.StreamId = VariableInt.EncodeVariableInt(0);
            this.Metadata.FrameId = VariableInt.EncodeVariableInt(0);

            var payload = new KeyValueListPayload();
            payload.KeyValueItems.Add("version", new TypedData(DataType.String, supportedSpopVersion));
            payload.KeyValueItems.Add("max-frame-size", new TypedData(DataType.Uint32, maxFrameSize));
            payload.KeyValueItems.Add("capabilities", new TypedData(DataType.String, string.Join(",", capabilities)));
            this.Payload = payload;
        }
    }
}