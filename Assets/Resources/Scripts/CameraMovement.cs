using UnityEngine;
using System;

//Concept and code for Follow camera from https://www.youtube.com/playlist?list=PLKFvhfT4QOqlEReJ2lSZJk_APVq5sxZ-x
//Free Camera written by Alex Carnot

public class CameraMovement : MonoBehaviour
{
    //public GameController gameMaster; // reference to the game controller
    [SerializeField]
    private Transform Target; //the object the camera should rotate around, set in editor.

    [SerializeField]
    private float distanceAway; //distance camera tries to stay away from player
    [SerializeField]
    private float distanceUp; //
    private Vector3 targetPosition; //the target position for the camera to move to.
    private Vector3 lookDirection;
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float smoothTime;
    private enum cameraStates { autoFollow, freeCam } //the states the camera can be in.
    private int cameraState; //the current state the camera is in
    private bool axisUsed;

    float cameraYSpeed;
    float cameraXSpeed; //the speed of the camera's movement.
    //private bool inWater; //whether or not the camera is in water.
    float cameraYInvert;
    float cameraXInvert;
    float cameraSensitivity;
    //public ColorCorrectionLookup waterCorrect; //reference to the color correction for the water.
    //public ColorCorrectionLookup lavaCorrect;
    //public GlobalFog waterFog;

    // Unity Start() method
    void Start()
    {
        //targetPosition = new Vector3(0.0f, 1.5f, -1.5f);
        //transform.localPosition = targetPosition;
        //gameMaster = GameController.gameMaster;
        cameraXSpeed = 200.0f;
        cameraYSpeed = 3.5f;
        //inWater = false;
        //if (waterCorrect)
        //    waterCorrect.enabled = false;
        //if (lavaCorrect)
        //    lavaCorrect.enabled = false;
        //if (waterFog)
        //    waterFog.enabled = false;
        //setQuality();
        cameraState = 0;
        axisUsed = false;

        cameraYInvert = -1;
        cameraXInvert = 1;
        cameraSensitivity = 1;
    }

    void LateUpdate()
    {
        if (Target)
        {
            float cameraVertical = Input.GetAxis("CameraVertical");
            float cameraHorizontal = Input.GetAxis("CameraHorizontal");

            if ((Input.GetButtonDown("CameraSwitch")) && !axisUsed)
            {
                Debug.Log(cameraState);
                if (cameraState == Enum.GetNames(typeof(cameraStates)).Length - 1)
                    cameraState = 0;
                else
                    ++cameraState;
                axisUsed = true;
                Debug.Log(cameraState);
            }
            if (!Input.GetButtonDown("CameraSwitch"))
                axisUsed = false;

            Vector3 originalPos = transform.position;

            RaycastHit wallHit;
            Vector3 direction;
            float distance;
            float distanceOffset;
            float angleOffset;

            direction = transform.position - Target.position;
            distance = direction.magnitude;

            wallHit = new RaycastHit();
            if (Physics.Raycast(Target.position, direction, out wallHit, distanceAway + 0.5f, 1 << 8))
            {
                Debug.DrawLine(Target.position, wallHit.point, Color.red, 120);
                direction = wallHit.point - Target.position;
                distanceOffset = Mathf.Clamp(distanceAway - direction.magnitude + 0.13f, 0, distanceAway);
            }
            else
                distanceOffset = 0;

            switch (cameraState)
            {
                //auto follow state
                case (int)cameraStates.autoFollow:
                    transform.parent = null;

                    //find the vector between the target and the camera's position, remove it's y component, then normalize it.
                    lookDirection = Vector3.Normalize(Target.position - transform.position);
                    lookDirection.y = 0;

                    //set the target position to be properly offset from the character.
                    targetPosition = Target.position + Vector3.up * distanceUp - lookDirection * (distanceAway - distanceOffset);

                    //camera horizontal movement
                    if (cameraHorizontal > 0 || cameraHorizontal < 0)
                        transform.RotateAround(Target.position, Vector3.up, cameraXSpeed * cameraHorizontal * cameraXInvert * cameraSensitivity * Time.deltaTime);

                    //camera horizontal movement
                    if (cameraVertical > 0 || cameraVertical < 0)
                    {
                        //if the angle is above 75 degrees push it back before letting it move further
                        if (transform.rotation.eulerAngles.x > 75 && transform.rotation.eulerAngles.x < 95)
                            transform.rotation = Quaternion.Euler(75, transform.rotation.y, transform.rotation.z);
                        else
                            transform.RotateAround(Target.position, transform.right, cameraXSpeed * cameraVertical * cameraYInvert * cameraSensitivity * Time.deltaTime);
                    }

                    //smooth the camera's position to it's new target position then look at the target.
                    smoothCamera(transform.position, targetPosition);

                    break;
                case (int)cameraStates.freeCam:
                    transform.parent = Target;

                    //lookDirection = Vector3.Normalize(Target.position - transform.position);
                    lookDirection.y = 0;

                    direction = transform.position - Target.position;
                    distance = direction.magnitude;

                    // If wall found, set camera distance to be equal to the distance to the wall-offset, but with a min and max distance
                    wallHit = new RaycastHit();
                    if (Physics.Raycast(Target.position, direction, out wallHit, distanceAway, 1 << 8))
                    {
                        direction = wallHit.point - Target.position;
                        distanceOffset = Mathf.Clamp(direction.magnitude - 0.3f, 0.2f, distanceAway);
                    }
                    else
                        distanceOffset = distanceAway;

                    targetPosition = Target.position + Vector3.up * (transform.position.y - Target.position.y) - (Vector3.ProjectOnPlane(transform.forward, Vector3.up) * (distanceOffset));

                    direction = targetPosition - Target.position;
                    distance = direction.magnitude;

                    // Check if the position we just put it in, happens to be behind a wall...
                    // This can occur on slopes, because the camera can be pushed really close to the player, which changes the player->camera angle
                    // Then, on the next ray check, it sees no ramp due to the angle, and shoves the camera behind a wall
                    wallHit = new RaycastHit();
                    if (Physics.Raycast(Target.position, direction, out wallHit, distanceAway, 1 << 8))
                    {
                        direction = wallHit.point - Target.position;
                        distanceOffset = Mathf.Clamp(direction.magnitude - 0.3f, 0.2f, distanceAway);
                        targetPosition = Target.position + Vector3.up * (transform.position.y - Target.position.y) - (Vector3.ProjectOnPlane(transform.forward, Vector3.up) * (distanceOffset));
                    }

                    transform.position = targetPosition;

                    //camera horizontal movement
                    if (cameraHorizontal > 0 || cameraHorizontal < 0)
                        transform.RotateAround(Target.position, Vector3.up, cameraXSpeed * cameraHorizontal * cameraXInvert * cameraSensitivity * Time.deltaTime);

                    // Basically this code treats things like the ZY plane is a circle rotated on the ZX plane
                    // I simply undo the rotation so I can modify the ZY circle, then I redo the rotation
                    float movAngle = (cameraXSpeed * cameraVertical * cameraYInvert * cameraSensitivity * Time.deltaTime);                            // How far to move (in degrees)
                    Vector3 tPos = transform.position - Target.position;                                                                                                    // Set sphere to origin
                    float xAng = Mathf.Atan(tPos.x / tPos.z) + ((1 - Mathf.Sign(tPos.z)) / 2 * Mathf.PI);                                                                   // Find the angle(in radians) between x and z, if tPos.z is negative, add pi to the radians, this gets us to the other half of the circle
                    float xR = Mathf.Sqrt(Mathf.Pow(tPos.z, 2) + Mathf.Pow(tPos.x, 2));                                                                                     // Find the radius of the circle x and z lie on
                    float Zy = xR;                                                                                                                                          // Follow the xz circle until x == 0 (angle will also be 0), so we can rotate around the x axis. Zy == xR because cos0 == 1, so cos0 * xR == xR == Z on the zy axis
                    float yAng = Mathf.Rad2Deg * (Mathf.Atan(tPos.y / Zy) + ((1 - Mathf.Sign(Zy)) / 2 * Mathf.PI));                                                         // Find the angle(in degrees) between y and z, if tPos.z is negative, add pi to the radians, this gets us to the other half of the circle
                    float yAng1 = Mathf.Deg2Rad * ((yAng + movAngle) - 10);                                                                                                 // Add the movAng+bias to the yAng, then convert this new angle to radians for further use
                    float yR = Mathf.Sqrt(Mathf.Pow(Zy, 2) + Mathf.Pow(tPos.y, 2));                                                                                         // Find the radius of the circle y and z lie on, should be the same as the radius for the sphere
                    float y1 = Mathf.Sin(yAng1) * yR;                                                                                                                       // Use the new angle and the sphere radius to find y
                    float Zx = Mathf.Cos(yAng1) * yR;                                                                                                                       // Use the new angle and the sphere radius to find z
                    float xR1 = Zx;                                                                                                                                         // Same as before, Zx lies on a circle used to find x, since x is 0, Zx == xR1
                    float x1 = Mathf.Sin(xAng) * xR1;                                                                                                                       // Use the xAng to return to the original 3D orientation
                    float z1 = Mathf.Cos(xAng) * xR1;                                                                                                                       // Find the new z that comes from the rotation by xAng
                    Vector3 newPos = new Vector3(Target.position.x + x1, Target.position.y + y1, Target.position.z + z1);

                    angleOffset = 0;

                    // Check if there's a wall between the camera and the projected position, or if there's a wall between the player and the expected new position
                    // If there is, find the angle to the wall, and find how far the camera should move to be 10 degrees away from it
                    if (Physics.Linecast(transform.position, newPos, out wallHit, 1 << 8) || Physics.Linecast(Target.position, newPos, out wallHit, 1 << 8))
                    {
                        tPos = wallHit.point - Target.position;
                        xAng = Mathf.Atan(tPos.x / tPos.z) + ((1 - Mathf.Sign(tPos.z)) / 2 * Mathf.PI);
                        xR = Mathf.Sqrt(Mathf.Pow(tPos.z, 2) + Mathf.Pow(tPos.x, 2));
                        Zy = xR;
                        float wallyAng = Mathf.Rad2Deg * (Mathf.Atan(tPos.y / Zy) + ((1 - Mathf.Sign(Zy)) / 2 * Mathf.PI));
                        float wallHitAngle = wallyAng - yAng;   // Angle needed to move the camera to the wall, always negative
                        if (Mathf.Abs(wallHitAngle) < 10)
                            angleOffset = wallHitAngle + 10;
                    }
                    else if (yAng + movAngle > 80)
                        angleOffset = movAngle - ((yAng + movAngle) - 80);
                    else
                        angleOffset = movAngle;

                    //camera vertical movement
                    transform.RotateAround(Target.position, transform.right, angleOffset);
                    targetPosition = transform.position;

                    break;
            }
        }

        //make the camera look at the Target
        transform.LookAt(Target);
    }

    // Unity Update() method
    void Update()
    {
        ////if refresh quality settings is true, refresh quality settings.
        //if (gameMaster.refreshQuality)
        //    setQuality();
    }

    //Camera smoothing method.
    //Sets its new position to be a interpolated value between it's current position (from) and it's target (to) based of a time.
    private void smoothCamera(Vector3 from, Vector3 to)
    {
        transform.position = Vector3.SmoothDamp(from, to, ref velocityCamSmooth, smoothTime);
    }

    //Camera clipping prevention method.
    //if a linecast between the camera's position and target position hits an environment object, 
    // change the target position to be where the linecast hit plus it's forward to prevent it from
    // clipping into the wall.
    private void wallBlocking(Vector3 from, ref Vector3 to)
    {
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(from, to, out wallHit, 1 << 8))
        {
            Debug.DrawLine(to, wallHit.point, Color.red);
            to = new Vector3(wallHit.point.x + transform.forward.x, to.y, wallHit.point.z + transform.forward.z);
        }
    }

    //set the quality settings important for this object
    //private void setQuality()
    //{
    //    if (GetComponent<SMAA>())
    //    {
    //        // **  Set the quality settings for the camera scripts  ** //
    //        //SMAA, 0 = off, 1 = low, 2 = medium, 3 = high, 4 = ultra 
    //        if (gameMaster.SMAAQuality == 0)
    //            GetComponent<SMAA>().enabled = false;
    //        else if (gameMaster.SMAAQuality == 1)
    //        {
    //            GetComponent<SMAA>().enabled = true;
    //            GetComponent<SMAA>().Quality = QualityPreset.Low;
    //        }
    //        else if (gameMaster.SMAAQuality == 2)
    //        {
    //            GetComponent<SMAA>().enabled = true;
    //            GetComponent<SMAA>().Quality = QualityPreset.Medium;
    //        }
    //        else if (gameMaster.SMAAQuality == 3)
    //        {
    //            GetComponent<SMAA>().enabled = true;
    //            GetComponent<SMAA>().Quality = QualityPreset.High;
    //        }
    //        else if (gameMaster.SMAAQuality == 4)
    //        {
    //            GetComponent<SMAA>().enabled = true;
    //            GetComponent<SMAA>().Quality = QualityPreset.Ultra;
    //        }
    //    }

    //    if (GetComponent<ScreenSpaceAmbientOcclusion>())
    //    {
    //        //SSAO, 0 = off, 1 = low, 2 = medium, 3 = high
    //        if (gameMaster.SSAOQuality == 0)
    //            GetComponent<ScreenSpaceAmbientOcclusion>().enabled = false;
    //        else if (gameMaster.SSAOQuality == 1)
    //        {
    //            GetComponent<ScreenSpaceAmbientOcclusion>().gameObject.SetActive(true);
    //            GetComponent<ScreenSpaceAmbientOcclusion>().enabled = true;
    //            GetComponent<ScreenSpaceAmbientOcclusion>().m_SampleCount = ScreenSpaceAmbientOcclusion.SSAOSamples.Low;
    //        }
    //        else if (gameMaster.SSAOQuality == 2)
    //        {
    //            GetComponent<ScreenSpaceAmbientOcclusion>().gameObject.SetActive(true);
    //            GetComponent<ScreenSpaceAmbientOcclusion>().enabled = true;
    //            GetComponent<ScreenSpaceAmbientOcclusion>().m_SampleCount = ScreenSpaceAmbientOcclusion.SSAOSamples.Medium;
    //        }
    //        else if (gameMaster.SSAOQuality == 3)
    //        {
    //            GetComponent<ScreenSpaceAmbientOcclusion>().gameObject.SetActive(true);
    //            GetComponent<ScreenSpaceAmbientOcclusion>().enabled = true;
    //            GetComponent<ScreenSpaceAmbientOcclusion>().m_SampleCount = ScreenSpaceAmbientOcclusion.SSAOSamples.High;
    //        }
    //    }

    //    if (GetComponent<BloomOptimized>() && GetComponent<SunShafts>())
    //    {
    //        //Post Processing, 0 = no bloom or sun shafts, 1 = bloom, 2 = sun shafts
    //        if (gameMaster.postProcessingQuality == 0)
    //        {
    //            GetComponent<BloomOptimized>().enabled = false;
    //            GetComponent<SunShafts>().enabled = false;
    //        }
    //        else if (gameMaster.postProcessingQuality == 1)
    //        {
    //            GetComponent<BloomOptimized>().enabled = true;
    //            GetComponent<SunShafts>().enabled = false;
    //        }
    //        else if (gameMaster.postProcessingQuality == 2)
    //        {
    //            GetComponent<BloomOptimized>().enabled = true;
    //            GetComponent<SunShafts>().enabled = true;
    //        }
    //    }
    //}


    //void OnTriggerExit(Collider iOther)
    //{
    //    if (iOther.gameObject.tag == "Water")
    //    {
    //        if (transform.position.y > iOther.transform.position.y)
    //        {
    //            inWater = false;
    //            waterCorrect.enabled = false;
    //            if (waterFog)
    //                waterFog.enabled = false;
    //        }
    //        else if (transform.position.y < iOther.transform.position.y)
    //        {
    //            inWater = true;
    //            waterCorrect.enabled = true;
    //            if (waterFog)
    //                waterFog.enabled = true;
    //        }
    //    }
    //    if (iOther.gameObject.tag == "Lava")
    //    {
    //        if (transform.position.y > iOther.transform.position.y)
    //        {
    //            inWater = false;
    //            lavaCorrect.enabled = false;
    //            if (waterFog)
    //                waterFog.enabled = false;
    //        }
    //        else if (transform.position.y < iOther.transform.position.y)
    //        {
    //            inWater = true;
    //            lavaCorrect.enabled = true;
    //            if (waterFog)
    //                waterFog.enabled = true;
    //        }
    //    }
    //}
}


////prevent camera from clipping into environment objects.
//wallHit = new RaycastHit();
//if (Physics.Linecast(Target.position, transform.position, out wallHit, 1 << 8))
//{
//    direction = wallHit.point - Target.position;
//    distance = direction.magnitude;
//    point = new Ray(Target.position, direction);
//    transform.position = point.GetPoint(distance - 0.3f);
//}

//// Get the direction to the new position of the camera, then look a little ahead of it, if there's a floor, don't move
//direction = transform.position - originalPos;
//distance = direction.magnitude;
//point = new Ray(originalPos, direction);

////// This straight up stops the camera from moving when it goes into a wall
////if (Physics.Linecast(originalPos, point.GetPoint(distance + 0.3f), out wallHit, 1 << 8))
////    transform.position = originalPos;

//infPrevent = 10;
//// This attempts to move the camera out of the wall only on the axes it breaches with, this allows the camera to somewhat slide across walls, rather than come to a dead stop
//while (Physics.Linecast(originalPos, point.GetPoint(distance + 0.3f), out wallHit, 1 << 8) && !(transform.position == originalPos) && infPrevent > 0)
//{
//    Vector3 p = point.GetPoint(distance + 0.3f);
//    Vector3 pBreach = new Vector3(Mathf.Abs(p.x - wallHit.point.x), Mathf.Abs(p.y - wallHit.point.y), Mathf.Abs(p.z - wallHit.point.z));
//    if (pBreach.x > pBreach.y && pBreach.x > pBreach.z)
//        transform.position = new Vector3(originalPos.x, transform.position.y, transform.position.z);    // Breach in x, reset x
//    else if (pBreach.y > pBreach.x && pBreach.y > pBreach.z)
//        transform.position = new Vector3(transform.position.x, originalPos.y, transform.position.z);    // Breach in y, reset y
//    else if (pBreach.z > pBreach.y && pBreach.z > pBreach.x)
//        transform.position = new Vector3(transform.position.x, transform.position.y, originalPos.z);    // Breach in z, reset z

//    direction = transform.position - originalPos;
//    distance = direction.magnitude;
//    point = new Ray(originalPos, direction);
//    infPrevent--;
//}