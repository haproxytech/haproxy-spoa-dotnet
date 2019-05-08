namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    public class HelperMethods
    {
        public static string ToBitString(byte[] buffer)
        {
            var bits = new System.Collections.BitArray(buffer);
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}