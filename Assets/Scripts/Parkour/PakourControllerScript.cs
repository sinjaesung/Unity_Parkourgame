using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PakourControllerScript : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    bool playerInAction;
    public Animator animator;
    public PlayerScript playerScript;
    [SerializeField] NewParkourAction jumpDownParkourAction;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    private void Update()
    {
        if(Input.GetButton("Jump") && !playerScript.playerInAction && !playerScript.playerHanging)
        {
            var hitData = environmentChecker.CheckObstacle();

            if (hitData.hitFound)
            {
                //Debug.Log("Object Founded||" + hitData.hitInfo.transform.name);

                foreach(var action in newParkourAction)
                {
                    if(action.CheckIfAvailable(hitData, transform))
                    {
                        //perform parkour action
                        StartCoroutine(PerformParkourAction(action));
                        break;
                    }
                }
            }
        }

        if(playerScript.playerOnLedge && !playerScript.playerInAction && Input.GetButtonDown("Jump"))
        {
           // Debug.Log("playerScript LedgeInfo angle,height" + playerScript.LedgeInfo.angle+","+ playerScript.LedgeInfo.height);
            if(playerScript.LedgeInfo.angle <= 50)
            {
                playerScript.playerOnLedge = false;
                StartCoroutine(PerformParkourAction(jumpDownParkourAction));
            }
        }
    }

    IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerScript.SetControl(false);

        CompareTargetParameter compareTargetParameter = null;
        if (action.AllowTargetMatching)
        {
            compareTargetParameter = new CompareTargetParameter()
            {
                position = action.ComparePosition,
                bodyPart = action.CompareBodyPart,
                positionWeight = action.ComparePositionWeight,
                StartTime = action.CompareStartTime,
                endTime = action.CompareEndTime
            };
        }

        yield return playerScript.PerformAction(action.AnimationName, compareTargetParameter, action.RequiredRotation,
            action.LookAtObstacle, action.ParkourActionDelay);
        playerScript.SetControl(true);
    }
}
