//-----------------------------------------------------------------------
// <copyright file="HaproxyHelloFrame.cs" company="HAProxy Technologies">
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
    public class HaproxyHelloFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the HaproxyHelloFrame class. HAProxy sends a HELLO
        /// frame during the initial handshake.
        /// </summary>
        public HaproxyHelloFrame() : base(FrameType.HaproxyHello)
        {
            this.Payload = new KeyValueListPayload();
        }
    }
}