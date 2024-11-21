using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    private Transform _camTransform;
    // Start is called before the first frame update
    void Start()
    {
        _camTransform = this.gameObject.transform;
    }
    void update()
    {
        Vector3 v = new Vector3(0,0,100);
        _camTransform.position += v;
    }
}
