﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position;

    #if UNITY_EDITOR || UNITY_STANDALONE

        // transform.rotation = player.rotation;

    #endif
    }
}
