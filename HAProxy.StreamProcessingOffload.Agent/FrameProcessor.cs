﻿//-----------------------------------------------------------------------
// <copyright file="FrameProcessor.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HAProxy.StreamProcessingOffload.Agent.Frames;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public class FrameProcessor : IFrameProcessor
    {
        private const string SUPPORTED_SPOE_VERSION = "2.0";
        private readonly string[] agentCapabilities;

        /// <summary>
        /// Initializes a new instance of the FrameProcessor class.
        /// </summary>
        public FrameProcessor()
        {
            this.LogFunc = (s) => { };
            this.MaxFrameSize = 16380;
            this.agentCapabilities = new string[] { "fragmentation" };
        }

        /// <summary>
        /// Gets or sets the agent's max-frame-size.
        /// </summary>
        public uint MaxFrameSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging should be
        /// enabled to see diagnostic information.
        /// </summary>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets a logging function that takes a string.
        /// </summary>
        public Action<string> LogFunc { get; set; }

        /// <summary>
        /// Handles receiving and sending frames on the given stream.
        /// </summary>
        /// <param name="stream">The stream to receive and send frames on</param>
        /// <param name="notifyHandler">Function to invoke when a NOTIFY frame is received</param>
        public void HandleStream(Stream stream, Func<NotifyFrame, IList<SpoeAction>> notifyHandler)
        {
            this.HandleStream(stream, stream, notifyHandler);
        }

        /// <summary>
        /// Handles receiving and sending frames on the given stream.
        /// </summary>
        /// <param name="inputStream">The stream to receive frames from</param>
        /// <param name="outputStream">The stream to send frames on</param>
        /// <param name="notifyHandler">Function to invoke when a NOTIFY frame is received</param>
        public void HandleStream(Stream inputStream, Stream outputStream, Func<NotifyFrame, IList<SpoeAction>> notifyHandler)
        {
            if (inputStream == null)
            {
                throw new ApplicationException("Start method requires 'inputStream' parameter.");
            }

            if (outputStream == null)
            {
                throw new ApplicationException("Start method requires 'outputStream' parameter.");
            }

            if (notifyHandler == null)
            {
                throw new ApplicationException("Start method requires 'notifyHandler' parameter.");
            }

            var fragments = new FragmentCatalogue();
            string[] haproxyCapabilities = new string[0];

            while (true)
            {
                bool sendAgentDisconnect = false;
                string disconnectReason = string.Empty;
                Status disconnectStatus = Status.Normal;
                bool closeConnection = false;
                Frame frame = null;
                var responseFrames = new ConcurrentQueue<Frame>();

                try
                {
                    byte[] frameBytes = GetBytesForNextFrame(inputStream);
                    frame = ParseFrame(frameBytes);

                    if (this.EnableLogging)
                    {
                        if (frame != null)
                        {
                            this.LogFunc(frame.ToString());
                        }
                    }

                    switch (frame.Type)
                    {
                        case FrameType.HaproxyHello:
                            var agentHelloFrame = HandleHandshake(frame);
                            responseFrames.Enqueue(agentHelloFrame);

                            // if this is only a SPOP health check (option spop-check), then close the connection
                            if (((KeyValueListPayload)frame.Payload).KeyValueItems.Any(item => item.Key == "healthcheck" && (bool)item.Value.Value))
                            {
                                closeConnection = true;
                            }
                            else
                            {
                                string haproxyCapabilitiesString =
                                    (string)((KeyValueListPayload)frame.Payload).KeyValueItems.First(item => item.Key == "capabilities").Value.Value;
                                haproxyCapabilities = haproxyCapabilitiesString.Split(',');
                            }
                            break;
                        case FrameType.HaproxyDisconnect:
                            sendAgentDisconnect = true;
                            disconnectStatus = Status.Normal;
                            disconnectReason = "HAProxy disconnected";
                            break;
                        case FrameType.Notify:
                            if (frame.Metadata.Flags.Fin)
                            {
                                var actions = notifyHandler((NotifyFrame)frame);

                                var ackFrame = new AckFrame(
                                    frame.Metadata.StreamId.Value,
                                    frame.Metadata.FrameId.Value,
                                    actions ?? new List<SpoeAction>());

                                // fragment if necessary
                                if (haproxyCapabilities.Contains("fragmentation"))
                                {
                                    List<Frame> fragmentedFrames = ((AckFrame)ackFrame).FragmentFrame(this.MaxFrameSize);

                                    foreach (var f in fragmentedFrames)
                                    {
                                        responseFrames.Enqueue(f);
                                    }
                                }
                                else
                                {
                                    responseFrames.Enqueue(ackFrame);
                                }
                            }
                            else
                            {
                                fragments.Push(
                                    frame.Metadata.StreamId.Value,
                                    frame.Metadata.FrameId.Value,
                                    frameBytes);
                            }
                            break;
                        case FrameType.Unset:
                            if (frame.Metadata.Flags.Abort)
                            {
                                fragments.Discard(frame.Metadata.StreamId.Value, frame.Metadata.FrameId.Value);
                            }
                            else
                            {
                                // The Unset frame continues the data, but we only need
                                // to append its payload, not its metadata
                                fragments.Push(
                                    frame.Metadata.StreamId.Value,
                                    frame.Metadata.FrameId.Value,
                                    frame.Payload.Bytes);

                                if (frame.Metadata.Flags.Fin)
                                {
                                    byte[] data = fragments.Pop(frame.Metadata.StreamId.Value, frame.Metadata.FrameId.Value);
                                    Frame newNotifyFrame = ParseFrame(data);
                                    newNotifyFrame.Metadata.Flags.Fin = true;

                                    if (this.EnableLogging)
                                    {
                                        this.LogFunc(newNotifyFrame.ToString());
                                    }

                                    var actions = notifyHandler((NotifyFrame)newNotifyFrame);

                                    var ackFrame = new AckFrame(
                                        frame.Metadata.StreamId.Value,
                                        frame.Metadata.FrameId.Value,
                                        actions ?? new List<SpoeAction>());

                                    // fragment if necessary
                                    if (haproxyCapabilities.Contains("fragmentation"))
                                    {
                                        var fragmentedFrames = ((AckFrame)ackFrame).FragmentFrame(this.MaxFrameSize);

                                        foreach (var f in fragmentedFrames)
                                        {
                                            responseFrames.Enqueue(f);
                                        }
                                    }
                                    else
                                    {
                                        responseFrames.Enqueue(ackFrame);
                                    }
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.LogFunc(ex.ToString());
                    sendAgentDisconnect = true;
                    disconnectStatus = Status.UnknownError;
                    disconnectReason = ex.Message;
                    closeConnection = true;
                }

                if (this.EnableLogging)
                {
                    if (responseFrames.Any())
                    {
                        foreach (var rf in responseFrames)
                        {
                            this.LogFunc(rf.ToString());
                        }
                    }
                }

                if (responseFrames.Any())
                {
                    Frame dequeuedFrame;

                    while (responseFrames.TryDequeue(out dequeuedFrame))
                    {
                        outputStream.Write(dequeuedFrame.Bytes, 0, dequeuedFrame.Bytes.Length);
                    }
                }

                if (sendAgentDisconnect)
                {
                    Frame disconnectFrame = Disconnect(disconnectStatus, disconnectReason);

                    if (this.EnableLogging)
                    {
                        this.LogFunc(disconnectFrame.ToString());
                    }

                    outputStream.Write(disconnectFrame.Bytes, 0, disconnectFrame.Bytes.Length);
                    closeConnection = true;
                }

                if (closeConnection)
                {
                    if (this.EnableLogging)
                    {
                        this.LogFunc("Closing connection.");
                    }

                    outputStream.Close();
                    inputStream.Close();
                    break;
                }
            }
        }

        public void CancelStream(Stream stream)
        {
            try{
                if (this.EnableLogging)
                {
                    this.LogFunc("Cancellation has been requested");
                }
                
                Frame disconnectFrame = Disconnect(Status.Normal, "Stream was cancelled");
                stream.Write(disconnectFrame.Bytes, 0, disconnectFrame.Bytes.Length);

                if (this.EnableLogging)
                {
                    this.LogFunc(disconnectFrame.ToString());
                }

                if (this.EnableLogging)
                {
                    this.LogFunc("Cancel - Closing connection");
                }

                stream.Close();
            }
            catch (ObjectDisposedException)
            {
                if (this.EnableLogging)
                {
                    this.LogFunc("Cancel - Connection already closed, moving on");
                }
            }
        }

        private Frame HandleHandshake(Frame frame)
        {
            if (frame.Type != FrameType.HaproxyHello)
            {
                throw new ApplicationException("Unimplemented frame type in handshake.");
            }

            var haproxyMaxFrameSize =
                (uint)((KeyValueListPayload)frame.Payload).KeyValueItems
                    .First(item => item.Key == "max-frame-size").Value.Value;

            if (haproxyMaxFrameSize < this.MaxFrameSize)
            {
                this.MaxFrameSize = haproxyMaxFrameSize;
            }

            var agentHelloFrame = new AgentHelloFrame(
                SUPPORTED_SPOE_VERSION,
                this.MaxFrameSize,
                this.agentCapabilities);

            if (agentHelloFrame.Bytes.Length > this.MaxFrameSize)
            {
                throw new ApplicationException("Frame size exceeded");
            }

            return agentHelloFrame;
        }

        private Frame ParseFrame(byte[] buffer)
        {
            FrameType frameType = ParseFrameType(buffer[0]);
            buffer = buffer.Skip(1).ToArray();
            Frame frame = NewFrameFromType(frameType);
            frame.Parse(buffer);
            return frame;
        }

        private Frame Disconnect(Status status = Status.Normal, string message = "")
        {
            var agentDisconnectFrame = new AgentDisconnectFrame(status, message);
            byte[] bytes = agentDisconnectFrame.Bytes;

            if (bytes.Length > this.MaxFrameSize)
            {
                throw new ApplicationException("Frame size exceeded");
            }

            return agentDisconnectFrame;
        }

        private byte[] GetBytesForNextFrame(Stream stream)
        {
            int length = ParseFrameLength(stream);

            if (length == 0)
            {
                return new byte[0];
            }

            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        private Frame NewFrameFromType(FrameType frameType, Status status = Status.Normal, string message = null)
        {
            Frame frame;

            switch (frameType)
            {
                case FrameType.HaproxyDisconnect:
                    frame = new HaproxyDisconnectFrame();
                    break;
                case FrameType.HaproxyHello:
                    frame = new HaproxyHelloFrame();
                    break;
                case FrameType.Notify:
                    frame = new NotifyFrame();
                    break;
                case FrameType.Unset:
                    frame = new UnsetFrame();
                    break;
                default:
                    throw new ApplicationException("Unacceptable frame type in NewFrame: " + frameType.ToString());
            }

            return frame;
        }

        private int ParseFrameLength(Stream stream)
        {
            var buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);

            if (bytesRead == 0)
            {
                // 'Read' returns 0 when there is no more data in
                // the stream and no more is expected
                return 0;
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                int length = BitConverter.ToInt32(buffer, 0);

                if (length > this.MaxFrameSize)
                {
                    length = (int)this.MaxFrameSize;
                }

                return length;
            }
        }

        private FrameType ParseFrameType(int frameTypeByte)
        {
            if (Enum.IsDefined(typeof(FrameType), frameTypeByte))
            {
                return (FrameType)frameTypeByte;
            }
            else
            {
                throw new ApplicationException("Unable to parse frame type. Got value: " + frameTypeByte);
            }
        }
    }
}
