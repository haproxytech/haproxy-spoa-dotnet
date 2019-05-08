using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class NotifyFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the NotifyFrame class. HAProxy sends NOTIFY
        /// frames that contain messages (the data it is sending to the agent).
        /// </summary>
        public NotifyFrame() : base(FrameType.Notify)
        {
            this.Payload = new ListOfMessagesPayload();
        }
    }
}