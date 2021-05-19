//-----------------------------------------------------------------------
// <copyright file="IFrameProcessor.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using HAProxy.StreamProcessingOffload.Agent.Frames;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public interface IFrameProcessor
    {
        /// <summary>
        /// Gets or sets the agent's max-frame-size.
        /// </summary>
        uint MaxFrameSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging should be
        /// enabled to see diagnostic information.
        /// </summary>
        bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets a logging function that takes a string.
        /// </summary>
        Action<string> LogFunc { get; set; }

        /// <summary>
        /// Handles receiving and sending frames on the given stream.
        /// </summary>
        /// <param name="stream">The stream to receive and send frames on</param>
        /// <param name="notifyHandler">Function to invoke when a NOTIFY frame is received</param>
        void HandleStream(Stream stream, Func<NotifyFrame, IList<SpoeAction>> notifyHandler);

        /// <summary>
        /// Handles receiving and sending frames on the given stream.
        /// </summary>
        /// <param name="inputStream">The stream to receive frames from</param>
        /// <param name="outputStream">The stream to send frames on</param>
        /// <param name="notifyHandler">Function to invoke when a NOTIFY frame is received</param>
        void HandleStream(Stream inputStream, Stream outputStream, Func<NotifyFrame, IList<SpoeAction>> notifyHandler);

        /// <summary>
        /// Cancels processing on the given stream.
        /// </summary>
        /// <param name="stream">The stream to cancel</param>
        void CancelStream(Stream stream);
    }
}