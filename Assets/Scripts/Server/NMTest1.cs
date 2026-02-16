using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NMTest1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // NetManager.AddListener("Enter", OnEnter);
        // NetManager.AddListener("Move", OnMove);
        // NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect("127.0.0.1",33333);
    }

    private void OnLeave(string str)
    {
        Debug.Log("OnLeave " + str);
    }

    private void OnMove(string str)
    {
        Debug.Log("OnMove " + str);
    }

    private void OnEnter(string str)
    {
        Debug.Log("OnEnter " + str);
    }
}
