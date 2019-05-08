//-----------------------------------------------------------------------
// <copyright file="HaproxyDisconnectFrame.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class HaproxyDisconnectFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the HaproxyDisconnectFrame class. HAProxy 
        /// may send a Disconnect frame in case of an error.
        /// </summary>
        public HaproxyDisconnectFrame() : base(FrameType.HaproxyDisconnect)
        {
            this.Payload = new KeyValueListPayload();
        }
    }
}