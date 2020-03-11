using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCamera : MonoBehaviour
{ 
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Canvas>().worldCamera = GameObject.Find("Player").GetComponentInChildren<Camera>();
        Debug.Log("Set "+name+"'s worldCamera");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
