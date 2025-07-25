﻿using SocketIO;
using UnityEngine;

public class ServerTest : MonoBehaviour
{
    public CommandServer server;

    SocketIOEvent socketEvent;
    JSONObject json;

    void Start()
    {
        json = new JSONObject();
        json.AddField("steering_angle", "0");
        json.AddField("throttle", "1");
        json.AddField("vert_angle", "0");
        socketEvent = new SocketIOEvent("OnSteer", json);
    }

    void Update()
    {
        json["steering_angle"].str = Random.Range(-1f, 1f).ToString();
//		json [ "throttle" ].str = 1f.ToString ();
        server.SendMessage("OnSteer", socketEvent);
    }
}