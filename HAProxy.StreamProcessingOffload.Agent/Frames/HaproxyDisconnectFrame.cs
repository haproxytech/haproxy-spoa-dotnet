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