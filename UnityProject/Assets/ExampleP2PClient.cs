using Byn.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class ExampleP2PClient : MonoBehaviour {

    P2PClient client;

    public string roomName = "test";
    public string signalingServer = "wss://nameless-scrubland-88927.herokuapp.com"; // Please use a different server, this is mine but I'll let people use it for testing
    // Use this for initialization
    void Start () {
        client = new P2PClient(signalingServer, roomName);
        client.OnReceivedMessage += Client_OnReceivedMessage;
        client.OnConnection += Client_OnConnection;
        client.OnDisconnection += Client_OnDisconnection;
        curTime = Time.time;
    }

    void Client_OnDisconnection(ConnectionId connectionId)
    {
        Debug.Log("Peer: " + connectionId.ToString() + " disconnected");
    }

    void Client_OnConnection(ConnectionId connectionId)
    {
        Debug.Log("Peer: " + connectionId.ToString() + " connected");
    }
    



    void Client_OnReceivedMessage(NetworkEvent message)
    {
        string msg = Encoding.UTF8.GetString(message.MessageData.Buffer, message.MessageData.Offset, message.MessageData.ContentLength);
        Debug.Log("Client received message: " + msg + " from client " + message.ConnectionId.ToString());
    }
    

    float curTime = 0;
    void Update()
    {
        if (Time.time - curTime > 10)
        {
            client.SendMessageToAll(Encoding.UTF8.GetBytes("Hi there I'm a client"), true);
            curTime = Time.time;
        }

        client.UpdateClient();
    }

    bool cleanedUp = false;
    void Cleanup()
    {
        if (!cleanedUp)
        {
            cleanedUp = true;
            client.Dispose();
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
