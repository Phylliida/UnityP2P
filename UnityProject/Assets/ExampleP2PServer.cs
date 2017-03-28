using Byn.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class ExampleP2PServer : MonoBehaviour {

    P2PServer server;

    public string roomName = "test";
    public string signalingServer = "wss://nameless-scrubland-88927.herokuapp.com"; // Please use a different server, this is mine but I'll let people use it for testing
    // Use this for initialization
    void Start () {
        server = new P2PServer(signalingServer, roomName);
        server.OnReceivedMessage += Server_OnReceivedMessage;
        server.OnConnection += Server_OnConnection;
        server.OnDisconnection += Server_OnDisconnection;
        curTime = Time.time;
    }


    void Server_OnConnection(ConnectionId connectionId)
    {
        Debug.Log("Client: " + connectionId.ToString() + " connected");
    }

    void Server_OnDisconnection(ConnectionId connectionId)
    {
        Debug.Log("Client: " + connectionId.ToString() + " disconnected");
    }


    void Server_OnReceivedMessage(NetworkEvent message)
    {
        string msg = Encoding.UTF8.GetString(message.MessageData.Buffer, message.MessageData.Offset, message.MessageData.ContentLength);
        Debug.Log("Server received message: " + msg + " from client " + message.ConnectionId.ToString());
    }

    float curTime = 0;
    void Update()
    {
        if (Time.time - curTime > 10)
        {
            server.SendMessageToAll(Encoding.UTF8.GetBytes("Hi there I'm the server"), true);
            curTime = Time.time;
        }

        server.UpdateServer();
    }

    bool cleanedUp = false;
    void Cleanup()
    {
        if (!cleanedUp)
        {
            cleanedUp = true;
            server.Dispose();
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
