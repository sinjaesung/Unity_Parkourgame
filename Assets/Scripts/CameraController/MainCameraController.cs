using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public float rotationY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        //Get the current camera state
        var state = vcam.State;

        //Extract the rotation quaternion from the state
        var rotation = state.FinalOrientation;

      //  Debug.Log("[[MainCamraController]] vcam CinemachineVirtualCamera FinalOrientation:" + rotation);
        //Convert the rotation to Euler angles
        var euler = rotation.eulerAngles;

        //Get the y-axis value from the Euler angles
        rotationY = euler.y;

       // Debug.Log("[[MainCamraController]] get eulearAngles eulerxyz" + euler.x + "," + euler.y + "," + euler.z);

        //Round the rotation y value to the nearest integer
        var roundedRotationY = Mathf.RoundToInt(rotationY);
    }

    public Quaternion flatRotation => Quaternion.Euler(0, rotationY, 0);
}
