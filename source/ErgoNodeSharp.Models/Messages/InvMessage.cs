using System.Collections.Generic;
using System.IO;

namespace ErgoNodeSharp.Models.Messages
{
    public class InvMessage : NodeMessage
    {
        public InvMessage()
        {
            Headers = new List<byte[]>();
        }

        public override string MessageName => "Inv";
        public override MessageType MessageType => MessageType.Inv;

        public byte ModifierTypeId { get; set; }
        
        public IList<byte[]> Headers { get; set; }

        protected override byte[] SerializeBody()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(ModifierTypeId);
                    foreach (byte[] header in Headers)
                    {
                        writer.Write(header);
                    }
                }

                return ms.ToArray();
            }
        }

        public override void DeserializeBody(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    ModifierTypeId = reader.ReadByte();
                    byte headerCount = reader.ReadByte();
                    for (int i = 0; i < headerCount; i++)
                    {
                        byte[] headerBytes = reader.ReadBytes(32);
                        Headers.Add(headerBytes);
                        int bytesLeft = bytes.Length - (int)reader.BaseStream.Position;
                    }
                }
            }
        }
    }
}
