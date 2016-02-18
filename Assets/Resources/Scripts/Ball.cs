using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    bool isHit;
    float velCount;
    private Rigidbody physicsBody;

    [SerializeField]
    Aimer gameAimer;

    public bool IsHit { get { return isHit; } }

    // Use this for initialization
    void Start()
    {
        velCount = 0;
        physicsBody = GetComponent<Rigidbody>();
        isHit = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (isHit)
        {
            if (physicsBody.velocity.magnitude < 0.1f)
            {
                if (velCount > 3.0f)
                {
                    sleepRigidbody();
                }
                velCount += Time.fixedDeltaTime;
            }
            else
                velCount = 0;
        }
    }

    public void hitBall(Vector3 velocity)
    {
        wakeRigidbody();
        physicsBody.AddForce(velocity * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    public void wakeRigidbody()
    {
        velCount = 0;
        isHit = true;
    }

    public void sleepRigidbody()
    {
        isHit = false;
        physicsBody.velocity = new Vector3(0, 0, 0);
        gameAimer.transform.localEulerAngles = new Vector3(0, 0, 0);
        gameAimer.transform.position = this.transform.localPosition + new Vector3(0,0, -0.25f);
    }
}
