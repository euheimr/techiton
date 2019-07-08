using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
    Quaternion rotateUpToForward = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
    static Transform mainCameraTransform;
    // Use this for initialization
    void Start ()
    {
        mainCameraTransform = Camera.main.transform;
    }
    
    // Update is called once per frame
    void Update ()
    {
        transform.forward = rotateUpToForward * -mainCameraTransform.forward;
    }
}
