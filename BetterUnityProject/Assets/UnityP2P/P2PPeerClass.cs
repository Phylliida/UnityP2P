using System;
using UnityEngine;
using System.Collections;
using System.Text;
//using System.Diagnostics;
using MessageLibrary;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleWebBrowser;
using System.Text.RegularExpressions;

namespace UnityP2P
{
    public class P2PPeerClass : IDisposable
    {
        private BrowserEngine mainEngine;

        string MemoryFile = "MainSharedMem";

        int Port = 8885;


        public delegate void OnConnectionCallback(string id);
        public event OnConnectionCallback OnConnection;

        public delegate void OnDisconnectionCallback(string id);
        public event OnDisconnectionCallback OnDisconnection;

        public delegate void OnMessageCallback(string id, string message);
        public event OnMessageCallback OnMessage;

        public delegate void OnGetIDCallback(string id);
        public event OnGetIDCallback OnGetID;

        string roomName;
        public P2PPeerClass(string roomName)
        {
            this.roomName = roomName;
            mainEngine = new BrowserEngine();

            bool RandomMemoryFile = true;

            if (RandomMemoryFile)
            {
                Guid memid = Guid.NewGuid();
                MemoryFile = memid.ToString();
            }

            bool RandomPort = true;

            if (RandomPort)
            {
                System.Random r = new System.Random();
                Port = 8000 + r.Next(1000);
            }



            mainEngine.InitPlugin(10, 10, MemoryFile, Port, "https://phylliida.github.io", true);
            mainEngine.OnJavaScriptDialog += MainEngine_OnJavaScriptDialog;
            mainEngine.OnPageLoaded += MainEngine_OnPageLoaded;
            //run initialization
            //if (JSInitializationCode.Trim() != "")
            //    mainEngine.RunJSOnce(JSInitializationCode);
        }

        private void MainEngine_OnPageLoaded(string url)
        {
            //Debug.Log("loaded page");
            mainEngine.SendExecuteJSEvent("setRoom(\"" + Regex.Escape(roomName) + "\");");
        }

        string myId = null;
        public string GetId()
        {
            return myId;
        }

        private void MainEngine_OnJavaScriptDialog(string message, string prompt, MessageLibrary.DialogEventType type)
        {
            mainEngine.SendDialogResponse(true, "");
            if (message.Substring(0, "I am: ".Length)=="I am: ")
            {
                string myId = message.Substring("I am: ".Length).Trim();
                if (OnGetID != null)
                {
                    OnGetID(myId);
                }
            }
            else if(message.Substring(0, "connected to: ".Length) == "connected to: ")
            {
                string connectedId = message.Substring("connected to: ".Length);
                if (OnConnection != null)
                {
                    OnConnection(connectedId);
                }
            }
            else if(message.Substring(0, "failed to send message to ".Length) == "failed to send message to ")
            {
                string failedId = message.Substring("failed to send message to ".Length).Trim();
                if (OnDisconnection != null)
                {
                    OnDisconnection(failedId);
                }
            }
            else
            {
                string before = message.Split()[0].Trim();
                if (message.Substring(before.Length, " sent message ".Length) == " sent message ")
                {
                    if (OnMessage != null)
                    {
                        OnMessage(before.Trim(), Regex.Unescape(message.Substring(before.Length + " sent message ".Length)));
                    }
                }
                else
                {
                    Debug.Log("Unknown message: " + message);
                }
            }
        }

        public void SendMessage(string id, string message)
        {
           mainEngine.SendExecuteJSEvent("sendMessage(\"" + id + "\", \"" + Regex.Escape(message) + "\");");
        }


        private void SetUrl(string url)
        {
            mainEngine.SendNavigateEvent(url, false, false);
        }

        public void Update()
        {
            mainEngine.UpdateTexture();
        }


        public void Dispose()
        {
            Cleanup();
        }

        ~P2PPeerClass()
        {
            Cleanup();
        }

        object cleanupLock = new object();
        bool didCleanup = false;
        void Cleanup()
        {
            lock (cleanupLock)
            {
                if (!didCleanup)
                {
                    didCleanup = true;
                    mainEngine.Shutdown();
                }
            }
        }
    }
}