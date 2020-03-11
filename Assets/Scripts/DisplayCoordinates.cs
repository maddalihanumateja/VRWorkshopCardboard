using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCoordinates : MonoBehaviour
{
    public Camera main_camera;
    public Text coordinate_text;


    private float radius;
    private RaycastHit hit; 

    // Start is called before the first frame update
    void Start()
    {
        radius = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(main_camera.transform.position + radius*main_camera.transform.forward, -1* main_camera.transform.forward, out hit, 30))
        {
            coordinate_text.text = "(X: "+ hit.point.x.ToString("0.00") +", Y: " + hit.point.y.ToString("0.00") + ", Z: " + hit.point.z.ToString("0.00") + ")";
            //Debug.Log("RayCast hit something");
        }
        else
        {
            coordinate_text.text = "(X: , Y: , Z: )";
            //Debug.Log("Did not Hit");
        }
    }
}
