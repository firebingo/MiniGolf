using UnityEngine;
using System.Collections;

public class Aimer : MonoBehaviour
{
    float rotateSpeed = 15.0f;
    float hitSpeed = 500f;
    [SerializeField]
    Ball playerBall;

    [SerializeField]
    Transform rotatePoint;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerBall && rotatePoint)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                this.transform.RotateAround(rotatePoint.position, Vector3.up, -(rotateSpeed * Input.GetAxis("Horizontal")) * Time.deltaTime);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                this.transform.RotateAround(rotatePoint.position, Vector3.up, -(rotateSpeed * Input.GetAxis("Horizontal")) * Time.deltaTime);
            }

            if (Input.GetButtonDown("Confirm"))
            {
                if(!playerBall.IsHit)
                    playerBall.hitBall(this.transform.forward * hitSpeed);
            }

            this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
        }
    }
}
