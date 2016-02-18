using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    [SerializeField]
    Transform followTransform;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (followTransform)
            this.transform.position = new Vector3(transform.position.x, transform.position.y);
    }
}
