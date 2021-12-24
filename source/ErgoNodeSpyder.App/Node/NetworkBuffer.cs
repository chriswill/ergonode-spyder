namespace ErgoNodeSpyder.App.Node
{
    /// <summary>
    /// This class helps organize the data required for
    /// reading and writing to a network stream
    /// </summary>
    public class NetworkBuffer
    {
        public byte[] WriteBuffer { get; set; }
        public byte[] ReadBuffer { get; set; }
        public int CurrentWriteByteCount { get; set; }

        public NetworkBuffer()
        {
            ReadBuffer = new byte[8192];
            WriteBuffer = new byte[8192];
        }
    }
}