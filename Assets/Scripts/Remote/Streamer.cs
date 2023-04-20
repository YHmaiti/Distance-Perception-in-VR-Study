using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System;

public class TestMessage
{
    public int id;
    public string type = "test";
    public string image;
    public string input;

    public TestMessage(int id, string image, string input)
    {
        this.id = id;
        this.image = image;
        this.input = input;
    }
}


public class Streamer : MonoBehaviour
{
    private int msgId = 1;
    public int skipFrame = 90;


    private int f;

    void FixedUpdate()
    {
        f += 1;
        if (f > skipFrame)
        {
            f = 0;
            SendTestImage();
        }
    }


    private void SendJson<T>(T msg)
    {
        gameObject.GetComponent<Connection>().sendJson(msg);
    }


    public void SendTestImage()
    {
        //Debug.Log("Sending Image " + "trial:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTrial + 1)
        // + " FOV:" + (GameObject.Find("fovLimiter").GetComponent<fovlimiter>().currentFoV)
        //  + " DISTANCE:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTargetIndex)
        //  + " CLUTTER:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentClutterIndex + 1));
        Debug.Log("Sending Image " + "trial:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTrial + 1)
        + " Target Index:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTargetIndex)
        + " CLUTTER Index:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentClutterIndex + 1));
        byte[] bytes = gameObject.GetComponent<CameraCapture>().Capture();
        TestMessage msg = new TestMessage(msgId, Convert.ToBase64String(bytes),
           "trial:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTrial + 1)
        + " Target Index:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentTargetIndex)
        + " CLUTTER Index:" + (GameObject.Find("ArrowWrapper").GetComponent<ArrowPointer>().currentClutterIndex + 1));
        msgId += 1;
        SendJson(msg);
    }
}
