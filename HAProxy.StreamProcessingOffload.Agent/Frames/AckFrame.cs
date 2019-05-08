using System.Collections.Generic;
using System.Linq;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace HAProxy.StreamProcessingOffload.Agent.Frames
{
    public class AckFrame : Frame
    {
        /// <summary>
        /// Initializes a new instance of the AckFrame class. An agent sends an ACK frame
        /// in response to a NOTIFY. An ACK may contain actions for HAProxy to perform.
        /// </summary>
        /// <param name="streamId">The stream ID to include in the ACK frame.</param>
        /// <param name="frameId">The frame ID to include in the ACK frame.</param>
        /// <param name="actions">A list of actions for HAProxy to apply.</param>
        public AckFrame(long streamId, long frameId, IList<SpoeAction> actions) : base(FrameType.Ack)
        {
            this.Metadata.Flags.Fin = true;
            this.Metadata.Flags.Abort = false;
            this.Metadata.StreamId = VariableInt.EncodeVariableInt(streamId);
            this.Metadata.FrameId = VariableInt.EncodeVariableInt(frameId);
            this.Payload = new ListOfActionsPayload()
            {
                Actions = actions
            };
        }

        /// <summary>
        /// Attempts to split a large ACK frame into fragments consisting of an ACK frame
        /// and multiple UNSET frames. Note that this SPOA will only use this if HAProxy supports this capability.
        /// </summary>
        /// <param name="maxFrameSize">The max size a frame can be</param>
        public List<Frame> FragmentFrame(uint maxFrameSize)
        {
            var frames = new List<Frame>();

            if (this.Bytes.Length <= maxFrameSize)
            {
                frames.Add(this);
                return frames;
            }

            int payloadBytesTaken = 0;
            int offset = 0;

            // truncated ACK frame
            var ackFrame = new AckFrame(0, 0, new List<SpoeAction>());
            ackFrame.Metadata = this.Metadata;
            ackFrame.Metadata.Flags.Fin = false;
            ackFrame.Payload = new RawDataPayload();
            ackFrame.Payload.Parse(this.Payload.Bytes.Take((int)maxFrameSize - ackFrame.Metadata.Bytes.Length - 5).ToArray(), ref offset); // subtract 5 for length and type
            payloadBytesTaken += ((int)maxFrameSize - ackFrame.Metadata.Bytes.Length - 5);
            frames.Add(ackFrame);

            while (payloadBytesTaken < this.Payload.Bytes.Length)
            {
                var unsetFrame = new UnsetFrame(this.Metadata.StreamId.Value, this.Metadata.FrameId.Value, false, false);
                offset = 0;
                unsetFrame.Payload.Parse(this.Payload.Bytes.Skip(payloadBytesTaken).Take((int)maxFrameSize - unsetFrame.Metadata.Bytes.Length - 5).ToArray(), ref offset);
                payloadBytesTaken += ((int)maxFrameSize - unsetFrame.Metadata.Bytes.Length - 5);
                frames.Add(unsetFrame);
            }

            frames.Last().Metadata.Flags.Fin = true;

            return frames;
        }
    }
}