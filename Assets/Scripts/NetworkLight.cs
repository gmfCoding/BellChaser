using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class NetworkLight : NetworkBehaviour
{
    private float timer = 0.0f;

    [Header("Colour:")]
    public Color activeColour = Color.white;
    public Color inactiveColour = Color.black;

    [Header("References:")]
    [SerializeField]
    private new Light light;

    [SerializeField]
    private new MeshRenderer renderer;

    [SerializeField]
    private int emmisionMaterialIndex = 0;

    private Material targetMaterial;

    [Header("Settings:")]
    [SerializeField]
    private bool canModify = true;
    [SerializeField]
    private bool startsOn = false;

    // Representative of whether or not the light has power,
    // if the light is blinking, it's still always on even when it's on.
    // Even in a inactive blink phase the light is still considered "on" network-wise.
    private NetworkVariable<bool> isOn = new NetworkVariable<bool>(false);

    [Header("Blinking:")]
   
    public bool blink = false;
    public float blinkActive = 0.1f;
    public float blinkInactive = 0.5f;

    private bool blinkOn = false;
    public override void OnNetworkSpawn()
    {
        //targetMaterial = renderer.materials.Where(x => x.name == material.name).FirstOrDefault();
        targetMaterial = renderer.materials[emmisionMaterialIndex];
        isOn.OnValueChanged += OnIsOnNetvarChanged;
        if (startsOn)
            SetVisualsOn();
        else
            SetVisualsOff();
        if (!IsServer)
            return;
        isOn.Value = startsOn;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void requestActivateLightServerRpc()
    {
        if (!canModify || isOn.Value == true)
            return;
        timer = 0.0f;
        isOn.Value = true;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void requestDeactivateLightServerRpc()
    {
        if (!canModify || isOn.Value == false)
            return;
        timer = 0.0f;
        isOn.Value = false;
    }

    public void OnIsOnNetvarChanged(bool prev, bool state)
    {
        // Should we only update the state if it changed, it's probably not important for lights.
        //if (prev == state)
        //    return;
        if (state)
            SetVisualsOn();
        else
            SetVisualsOff();
    }

    public void Update()
    {
        if (isOn.Value && blink)
        {
            timer += Time.deltaTime;
            if (blinkOn && timer > blinkActive)
            {
                SetVisualsOff();
                blinkOn = !blinkOn;
                timer = 0.0f;
            }
            else if (!blinkOn && timer > blinkInactive)
            {
                SetVisualsOn();
                blinkOn = !blinkOn;
                timer = 0.0f;
            }
        }
    }

    void SetVisualsOn()
    {
        light.enabled = true;
        targetMaterial?.SetColor("_EmissionColor", inactiveColour);
    }

    void SetVisualsOff()
    {
        light.enabled = false;
        targetMaterial?.SetColor("_EmissionColor", inactiveColour);
    }
}
