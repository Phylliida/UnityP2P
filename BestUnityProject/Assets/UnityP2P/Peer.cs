using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebRtc.NET;
using WebSocket4Net;
using LitJson;
using System.Collections.Concurrent;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace UnityP2P
{
    public class Peer : MonoBehaviour
    {


        public delegate void OnConnectionCallback(string peer);
        public event OnConnectionCallback OnConnection;

        public delegate void OnDisconnectionCallback(string peer);
        public event OnDisconnectionCallback OnDisconnection;

        public delegate void OnBytesFromPeerCallback(string peer, byte[] bytes, int offset, int len);
        public event OnBytesFromPeerCallback OnBytesFromPeer;

        public delegate void OnTextFromPeerCallback(string peer, string text);
        public event OnTextFromPeerCallback OnTextFromPeer;

        public delegate void GetIDCallback(string id);
        public event GetIDCallback OnGetID;

        public PeerClass myPeer;
        volatile bool needToCleanUp = false;

        public ConcurrentQueue<Tuple<string, string>> messagesToSend = new ConcurrentQueue<Tuple<string, string>>();
        public ConcurrentQueue<string> peersConnecting = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> peersDisconnecting = new ConcurrentQueue<string>();
        public ConcurrentQueue<Tuple<string, string>> peerMessages = new ConcurrentQueue<Tuple<string, string>>();

        public string room = "test";

        public void Start()
        {
            Thread bean = new Thread(() =>
            {
                myPeer = new PeerClass("ws://sample-bean.herokuapp.com", room);
                myPeer.OnConnection += Peer_OnConnection;
                myPeer.OnDisconnection += Peer_OnDisconnection;
                myPeer.OnTextFromPeer += Peer_OnTextFromPeer;

                while (!needToCleanUp)
                {
                    Thread.Sleep(10);
                    myPeer.Update();

                    Tuple<string, string> peerAndMessage;
                    while (messagesToSend.TryDequeue(out peerAndMessage))
                    {
                        myPeer.Send(peerAndMessage.left, peerAndMessage.right);
                    }
                }

                myPeer.Dispose();
            });
            bean.IsBackground = false;
            bean.Start();
        }

        bool sentMyId = false;
        public void Update()
        {
            if (myPeer != null && !sentMyId)
            {
                sentMyId = true;
                if (OnGetID != null)
                {
                    OnGetID(myPeer.myId);
                }
            }
            string peer;
            while (peersConnecting.TryDequeue(out peer))
            {
                if (OnConnection != null)
                {
                    OnConnection(peer);
                }
            }

            while (peersDisconnecting.TryDequeue(out peer))
            {
                if (OnDisconnection != null)
                {
                    OnDisconnection(peer);
                }
            }

            Tuple<string, string> peerAndMessage;
            while (peerMessages.TryDequeue(out peerAndMessage))
            {
                peer = peerAndMessage.left;
                string message = peerAndMessage.right;
                if (OnTextFromPeer != null)
                {
                    OnTextFromPeer(peer, message);
                }
            }
        }

        private void Peer_OnTextFromPeer(string peer, string text)
        {
            peerMessages.Enqueue(new Tuple<string, string>(peer, text));
        }

        private void Peer_OnDisconnection(string peer)
        {
            peersDisconnecting.Enqueue(peer);
        }

        private void Peer_OnConnection(string peer)
        {
            peersConnecting.Enqueue(peer);
        }

        void Cleanup()
        {
            needToCleanUp = true;
        }

        public void Send(string peer, string message)
        {
            messagesToSend.Enqueue(new Tuple<string, string>(peer, message));
        }

        public void OnApplicationQuit()
        {
            Cleanup();
        }

        public void OnDestroy()
        {
            Cleanup();
        }
    }
}