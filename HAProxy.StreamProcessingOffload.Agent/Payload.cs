using System;
using System.Collections.Generic;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public abstract class Payload
    {
        /// <summary>
        /// Initializes a new instance of the Payload class.
        /// </summary>
        /// <param name="type">The type of payload</param>
        protected Payload(PayloadType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type of payload.
        /// </summary>
        public PayloadType Type { get; private set; }

        /// <summary>
        /// Gets the bytes for the payload portion of the frame.
        /// </summary>
        public abstract byte[] Bytes { get; }

        /// <summary>
        /// Parses the payload from the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to parse to get the payload</param>
        /// <param name="offset">The offset at which to start reading the buffer</param>
        internal abstract void Parse(byte[] buffer, ref int offset);
    }
}