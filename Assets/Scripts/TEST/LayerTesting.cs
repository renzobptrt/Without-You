using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerTesting : MonoBehaviour
{
    BackgroundManager controller;

    public Texture texture;
    public float speed;
    public bool smooth;
    BackgroundManager.LAYER layer = null;
    void Start()
    {
        controller = BackgroundManager.instance;
        //texture = layer.GetTexture(BackgroundManager.backgroundStill.classroom);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.Q))
        {
            layer = controller.background;
        }
        if (Input.GetKey(KeyCode.W))
        {
            layer = controller.foreground;
        }
        if (Input.GetKey(KeyCode.T))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                layer.TransitionToTexture(layer.GetTexture(BackgroundManager.backgroundStill.classroom)
                    ,speed,smooth);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                layer.SetTexture(texture);
            }

        }
    }
}
