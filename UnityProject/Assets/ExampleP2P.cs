using Byn.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class ExampleP2P : MonoBehaviour {

    P2PClient client;
    P2PServer server;

    public string roomName = "test";
    public string signalingServer = "wss://nameless-scrubland-88927.herokuapp.com"; // Please use a different server, this is mine but I'll let people use it for testing
    public bool isServer = false;
    bool isActuallyServer;
    // Use this for initialization
    void Start () {
        // Don't let them change it once we start
        isActuallyServer = isServer;

        // Please use a different server, this is mine but I'll let people use it for testing
        if (isActuallyServer)
        {
            server = new P2PServer(signalingServer, roomName);
            server.OnReceivedMessage += Server_OnReceivedMessage;
            server.OnConnection += Server_OnConnection;
            server.OnDisconnection += Server_OnDisconnection;
        }
        else
        {
            client = new P2PClient(signalingServer, roomName);
            client.OnReceivedMessage += Client_OnReceivedMessage;
            client.OnConnection += Client_OnConnection;
            client.OnDisconnection += Client_OnDisconnection;
        }
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

    void Server_OnConnection(ConnectionId connectionId)
    {
        Debug.Log("Client: " + connectionId.ToString() + " connected");
    }

    void Server_OnDisconnection(ConnectionId connectionId)
    {
        Debug.Log("Client: " + connectionId.ToString() + " disconnected");
    }



    void Client_OnReceivedMessage(NetworkEvent message)
    {
        string msg = Encoding.UTF8.GetString(message.MessageData.Buffer, message.MessageData.Offset, message.MessageData.ContentLength);
        Debug.Log("Client received message: " + msg + " from client " + message.ConnectionId.ToString());
    }

    void Server_OnReceivedMessage(NetworkEvent message)
    {
        string msg = Encoding.UTF8.GetString(message.MessageData.Buffer, message.MessageData.Offset, message.MessageData.ContentLength);
        Debug.Log("Server received message: " + msg + " from client " + message.ConnectionId.ToString());
    }

    float curTime = 0;
    void Update()
    {
        isServer = isActuallyServer;
        if (Time.time - curTime > 10)
        {
            if (isActuallyServer)
            {
                server.SendMessageToAll(Encoding.UTF8.GetBytes("Hi there I'm the server"), true);
            }
            else
            {
                client.SendMessageToAll(Encoding.UTF8.GetBytes("Hi there I'm a client"), true);
            }
            curTime = Time.time;
        }
        if (isActuallyServer)
        {
            server.UpdateServer();
        }
        else
        {
            client.UpdateClient();
        }
    }

    bool cleanedUp = false;
    void Cleanup()
    {
        if (!cleanedUp)
        {
            cleanedUp = true;
            if (isActuallyServer)
            {
                server.Dispose();
            }
            else
            {
                client.Dispose();
            }
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
