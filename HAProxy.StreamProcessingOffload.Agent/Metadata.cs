//-----------------------------------------------------------------------
// <copyright file="Metadata.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public class Metadata
    {
        /// <summary>
        /// Initializes a new instance of the Metadata class.
        /// </summary>
        public Metadata()
        {
            this.Flags = new MetadataFlags();
        }

        /// <summary>
        /// Gets or sets the metadata flags.
        /// </summary>
        public MetadataFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the the stream ID.
        /// </summary>
        public VariableInt StreamId { get; set; }

        /// <summary>
        /// Gets or sets the frame ID.
        /// </summary>
        public VariableInt FrameId { get; set; }

        /// <summary>
        /// Gets the bytes for the metadata portion of the frame.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();
                bytes.AddRange(this.Flags.Bytes);
                bytes.AddRange(this.StreamId.Bytes);
                bytes.AddRange(this.FrameId.Bytes);
                return bytes.ToArray();
            }
        }

        internal void Parse(byte[] buffer, ref int bufferOffset)
        {
            // Flags always use 4 bytes
            this.Flags = ParseMetadataFlags(buffer.Skip(bufferOffset).Take(4).ToArray());
            bufferOffset += 4;

            // Stream ID
            this.StreamId = VariableInt.DecodeVariableInt(buffer.Skip(bufferOffset).ToArray());
            bufferOffset += this.StreamId.Length;

            // Frame ID
            this.FrameId = VariableInt.DecodeVariableInt(buffer.Skip(bufferOffset).ToArray());
            bufferOffset += this.FrameId.Length;
        }

        private MetadataFlags ParseMetadataFlags(byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            var bits = new BitArray(buffer);
            var flags = new MetadataFlags()
            {
                Fin = bits[0],
                Abort = bits[1]
            };

            return flags;
        }
    }
}