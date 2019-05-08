//-----------------------------------------------------------------------
// <copyright file="MetadataFlags.cs" company="HAProxy Technologies">
//     The contents of this file are Copyright (c) 2019. HAProxy Technologies. 
//     All rights reserved. This file is subject to the terms and conditions
//     defined in file 'LICENSE', which is part of this source code package.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public class MetadataFlags
    {
        /// <summary
        /// A value indicating whether this is the final payload fragment.
        /// The first fragment may also be the final fragment.
        /// </summary>
        public bool Fin;

        /// <summary>
        /// A value indicating whether the processing of the current frame
        /// must be cancelled. This should be set on frames with a fragmented
        /// payload.null It can be ignored for frames with an unfragmented payload.
        /// When it is set, Fin must also be set.
        /// </summary>
        public bool Abort;

        /// <summary>
        /// Gets the bytes for the flags portion of the frame's metadata.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                BitArray bits = new BitArray(32);
                bits[0] = this.Fin;
                bits[1] = this.Abort;

                for (int i = 2; i < 32; i++)
                {
                    bits[i] = false;
                }

                var bytes = new byte[4];
                bits.CopyTo(bytes, 0);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                return bytes;
            }
        }
    }
}