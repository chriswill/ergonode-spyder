namespace ErgoNodeSharp.Models
{
    public enum NetworkType
    {
        Testnet,
        Mainnet
    }

    public enum MessageType : byte
    {
        GetPeers = 1,
        Peers = 2,
        RequestModifier = 22,
        Modifier = 33,
        Inv = 55,
        Sync = 65,
        Handshake = 75
    }
    public enum StateType
    {
        Utxo,
        Digest
    }

    public enum FeatureType : byte
    {
        Address = 2,
        Peer = 3,
        Mode = 16
    }
}
