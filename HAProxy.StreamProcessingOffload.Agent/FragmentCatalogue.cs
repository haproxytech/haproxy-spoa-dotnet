using System.Collections.Generic;
using System.Linq;

namespace HAProxy.StreamProcessingOffload.Agent
{
    internal class FragmentCatalogue
    {
        private IDictionary<string, byte[]> fragments { get; set; }
        private object lockObj;

        public FragmentCatalogue()
        {
            this.fragments = new Dictionary<string, byte[]>();
            this.lockObj = new object();
        }

        public void Push(long streamId, long frameId, byte[] data)
        {
            lock (this.lockObj)
            {
                string key = CombineStreamIdAndFrameId(streamId, frameId);

                // if data for this fragment is already in the collection,
                // combine the new data to it
                if (this.fragments.Any(f => f.Key == key))
                {
                    data = this.fragments[key].Concat(data).ToArray();
                }

                this.fragments[key] = data;
            }
        }

        public byte[] Pop(long streamId, long frameId)
        {
            lock (this.lockObj)
            {
                string key = CombineStreamIdAndFrameId(streamId, frameId);

                if (this.fragments.Any(f => f.Key == key))
                {
                    var data = this.fragments[key];
                    DeleteKeyAndData(streamId, frameId);
                    return data;
                }

                return new byte[0];
            }
        }

        private void DeleteKeyAndData(long streamId, long frameId)
        {
            string key = CombineStreamIdAndFrameId(streamId, frameId);
            this.fragments.Remove(key);
        }

        private string CombineStreamIdAndFrameId(long streamId, long frameId)
        {
            return streamId.ToString() + frameId.ToString();
        }
    }
}