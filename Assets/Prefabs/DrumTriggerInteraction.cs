using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumTriggerInteraction : MonoBehaviour
{
    public delegate void DrumTouchedHandler(GameObject drum);
    public static event DrumTouchedHandler OnDrumTouched;

    public bool isTouched = false;

    private void OnTriggerEnter(Collider other)
    {
        if(isTouched == false)
        {
            OnHandTouch();
            isTouched = true;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        isTouched = false;
    }

    private void OnHandTouch()
    {
        // Notify subscribers that this drum was touched
        OnDrumTouched?.Invoke(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
