//-----------------------------------------------------------------------
// <copyright file="Frame.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public abstract class Frame
    {
        private int length;

        protected int bufferOffset;

        /// <summary>
        /// Initializes a new instance of the Frame class.
        /// </summary>
        /// <param name="frameType">The type of frame</param>
        protected Frame(FrameType frameType)
        {
            this.length = 0;
            this.bufferOffset = 0;
            this.Type = frameType;
            this.Metadata = new Metadata();
            this.Status = Status.Normal;
        }

        /// <summary>
        /// Gets the type of frame.
        /// </summary>
        public FrameType Type { get; protected set; }

        /// <summary>
        /// Gets the frame's metadata.
        /// </summary>
        public Metadata Metadata { get; protected set; }

        /// <summary>
        /// Gets the frame's payload.
        /// </summary>
        public Payload Payload { get; protected set; }

        /// <summary>
        /// Gets the status of the frame.
        /// </summary>
        public Status Status { get; protected set; }

        /// <summary>
        /// Gets the length of the frame.
        /// </summary>
        public int Length
        {
            get
            {
                if (this.length == 0)
                {
                    var bytes = this.Bytes;
                }

                return this.length;
            }
        }

        /// <summary>
        /// Gets the frame's bytes.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();
                bytes.Add((byte)this.Type);
                bytes.AddRange(this.Metadata.Bytes);
                bytes.AddRange(this.Payload.Bytes);

                int length = bytes.Count;
                this.length = length;
                byte[] lengthBytes = BitConverter.GetBytes(length);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);
                }

                bytes.InsertRange(0, lengthBytes);
                return bytes.ToArray();
            }
        }

        /// <summary>
        /// Gets a string representation of the frame.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Type: {0}", this.Type.ToString()));
            sb.AppendLine(string.Format("Length: {0}", this.Length));
            sb.AppendLine(string.Format("Flags - Fin: {0}", this.Metadata.Flags.Fin));
            sb.AppendLine(string.Format("Flags - Abort: {0}", this.Metadata.Flags.Abort));
            sb.AppendLine(string.Format("StreamID: {0}", this.Metadata.StreamId.Value));
            sb.AppendLine(string.Format("FrameID: {0}", this.Metadata.FrameId.Value));
            sb.Append(this.Payload.ToString());
            return sb.ToString();
        }

        internal void Parse(byte[] buffer)
        {
            this.Metadata.Parse(buffer, ref this.bufferOffset);
            this.Payload.Parse(buffer, ref this.bufferOffset);
        }
    }
}