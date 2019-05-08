using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class UnsetFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the UnsetFrame class. A fragmented
        /// Notify frame will be segmented into multiple Unset frames.
        /// </summary>
        public UnsetFrame() : base(FrameType.Unset)
        {
            this.Payload = new RawDataPayload();
        }

        public UnsetFrame(long streamId, long frameId, bool fin, bool abort) : this()
        {
            this.Metadata.StreamId = VariableInt.EncodeVariableInt(streamId);
            this.Metadata.FrameId = VariableInt.EncodeVariableInt(frameId);
            this.Metadata.Flags.Fin = fin;
            this.Metadata.Flags.Abort = abort;
        }
    }
}