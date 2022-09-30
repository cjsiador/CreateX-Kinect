using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorSkeleton : MonoBehaviour
{
    
    [Header("As of right now, mirror is automatically defaulted. Unmirror would reverse the default mirror")]
    public bool mirror = true;

    [Header("If the Kinect Camera is behind the player and you want to mirror the movement")]
    public bool backMirror = false;

    [Header("Kinect that is behind the player Mirrored")]

    public Transform sensorTransform;

    private bool switchedMirror = false;

    private bool switchedBackMirror = false;

    private IEnumerator mirrorCoroutine;

    private IEnumerator backMirrorCoroutine;


    // Update is called once per frame
    void Start()
    {
        if(!mirror && !switchedMirror)
        {
            mirrorCoroutine = WaitAndMirror(1f);
            StartCoroutine(mirrorCoroutine);
        }

        if(backMirror && !switchedBackMirror)
        {
            backMirrorCoroutine = WaitAndReverseMirror(1.5f);
            StartCoroutine(backMirrorCoroutine);
        }

    }

    //<summary>
    // As of right now, the skeleton's data mirror the player by default. Therefore, this function would un-mirror the skeleton to match the acctual joints.
    //</summary>
    private IEnumerator WaitAndMirror(float WaitTime)
    {
        while(!switchedMirror)
        {
            yield return new WaitForSeconds(WaitTime);
            Vector3 localTemp = sensorTransform.transform.localScale;
            localTemp.x *= -1;
            localTemp.z *= -1;
            sensorTransform.transform.localScale = localTemp;
            switchedMirror = true;
        }
    }

    //<summary>
    // This would be applicable when the camera is behind the player.
    //</summary>
    private IEnumerator WaitAndReverseMirror(float WaitTime)
    {
        while(!switchedBackMirror)
        {
            yield return new WaitForSeconds(WaitTime);
            
            sensorTransform.transform.localRotation = Quaternion.Euler(0f, 180f, 0);
            switchedBackMirror = true;
        }
    }

}
