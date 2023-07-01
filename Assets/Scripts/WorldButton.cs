using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldButton : InteractionRegion
{
    public KeyCode key = KeyCode.E;

    public GameObject activeCanvas;

    public UnityEvent activate;

    public void Update()
    {
        if (activeCanvas != null)
            activeCanvas.SetActive(HasClient());
        if (!HasClient())
            return;
        if (Input.GetKeyDown(key))
        {
            activate.Invoke();
        }
    }
}
