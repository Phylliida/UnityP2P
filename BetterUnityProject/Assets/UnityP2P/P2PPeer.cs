using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityP2P
{
    public class P2PPeer : MonoBehaviour
    {

        UnityP2P.P2PPeerClass peer;

        public delegate void OnConnectionCallback(string id);
        public event OnConnectionCallback OnConnection;

        public delegate void OnDisconnectionCallback(string id);
        public event OnDisconnectionCallback OnDisconnection;

        public delegate void OnMessageCallback(string id, string message);
        public event OnMessageCallback OnMessageFromPeer;

        public delegate void OnGetIDCallback(string id);
        public event OnGetIDCallback OnGetID;


        public string roomName = "beans";
        // Use this for initialization
        void Start()
        {
            peer = new UnityP2P.P2PPeerClass(roomName);
            peer.OnConnection += Peer_OnConnection; ;
            peer.OnDisconnection += Peer_OnDisconnection; ;
            peer.OnGetID += Peer_OnGetID; ;
            peer.OnMessage += Peer_OnMessage; ;
        }

        void OnApplicationQuit()
        {
            peer.Dispose(); // Is is okay to call this more than once but you only need to call it once, the rest do nothing
        }

        void OnDestroy()
        {
            peer.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            peer.Update();
        }

        public void Send(string id, string message)
        {
            peer.SendMessage(id, message);
        }

        void Peer_OnMessage(string id, string message)
        {
            if (OnMessageFromPeer != null)
            {
                OnMessageFromPeer(id, message);
            }
        }

        void Peer_OnGetID(string id)
        {
            if (OnGetID != null)
            {
                OnGetID(id);
            }
        }

        void Peer_OnDisconnection(string id)
        {
            if (OnDisconnection != null)
            {
                OnDisconnection(id);
            }
        }

        void Peer_OnConnection(string id)
        {
            if (OnConnection != null)
            {
                OnConnection(id);
            }
        }
    }
}