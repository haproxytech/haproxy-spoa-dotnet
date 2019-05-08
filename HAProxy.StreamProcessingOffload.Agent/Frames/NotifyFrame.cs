//-----------------------------------------------------------------------
// <copyright file="NotifyFrame.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class NotifyFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the NotifyFrame class. HAProxy sends NOTIFY
        /// frames that contain messages (the data it is sending to the agent).
        /// </summary>
        public NotifyFrame() : base(FrameType.Notify)
        {
            this.Payload = new ListOfMessagesPayload();
        }
    }
}