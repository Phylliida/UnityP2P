using UnityEngine;
using UnityP2P;

public class ExampleUsage : MonoBehaviour
{
    public Peer peer;

    // Use this for initialization
    void Start()
    {
        peer.OnConnection += Peer_OnConnection;
        peer.OnDisconnection += Peer_OnDisconnection;
        peer.OnTextFromPeer += Peer_OnMessage;
        peer.OnBytesFromPeer += Peer_OnBytesFromPeer;
        peer.OnGetID += Peer_OnGetID;
    }

    private void Peer_OnBytesFromPeer(string peer, byte[] bytes)
    {
        Debug.Log("got bytes: " + bytes.Length);
    }

    private void Peer_OnGetID(string id)
    {
        Debug.Log("my id is: " + id);
    }

    void Peer_OnMessage(string id, string message)
    {
        Debug.Log(id + " sent message: " + message);
    }

    void Peer_OnDisconnection(string id)
    {
        Debug.Log(id + " disconnected");
    }

    void Peer_OnConnection(string id)
    {
        Debug.Log(id + " connected");
        peer.Send(id, "Hi there");
        peer.Send(id, new byte[] { 27, 28 });
    }
}