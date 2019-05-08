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