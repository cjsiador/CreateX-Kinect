using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.rfilkov.components
{
    public class GetJointOrientation : MonoBehaviour
    {
        public int playerIndex = 0;

        public float smoothFactor;

        [Tooltip("Set a object that would rotate based on the joint asigned.")]
        public JointOrientation[] jointOrientation;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < jointOrientation.Length; i++)
            {
                if (jointOrientation[i].GameObjectTarget != null)
                {
                    jointOrientation[i].initialRotation = jointOrientation[i].GameObjectTarget.transform.rotation;
                    jointOrientation[i].qRotJoint = Quaternion.identity;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (kinectManager && kinectManager.IsInitialized())
            {
                if (kinectManager.IsUserDetected(playerIndex))
                {
                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);

                    for (int i = 0; i < jointOrientation.Length; i++)
                    {
                        int jointNumber = (int)jointOrientation[i].JointOrientationType;

                        if (kinectManager.IsJointTracked(userId, jointNumber) && (jointOrientation[i].GameObjectTarget != null))
                        {
                            jointOrientation[i].qRotJoint = kinectManager.GetJointOrientation(userId, jointNumber, !jointOrientation[i].mirroredView);  //GetJointOrientation(UserID, JointNumber, IfMirror true or false)
                            jointOrientation[i].qRotJoint = jointOrientation[i].initialRotation * jointOrientation[i].qRotJoint;
                            
                            if(smoothFactor != 0f)
                            {
                                jointOrientation[i].GameObjectTarget.transform.localRotation = Quaternion.Slerp(jointOrientation[i].GameObjectTarget.transform.localRotation, jointOrientation[i].qRotJoint, smoothFactor * Time.deltaTime);
                            }
                            else
                                jointOrientation[i].GameObjectTarget.transform.localRotation = jointOrientation[i].qRotJoint;
                        }
                    }
                }
            }
        }
    }

    // //A structure that holds JointOrientation information.
    [System.Serializable]    
    public struct JointOrientation
    {
        public KinectInterop.JointType JointOrientationType;    // The direction of the joint is facing.
        public GameObject GameObjectTarget;
        public bool mirroredView;
        
        [HideInInspector]
        public Quaternion initialRotation;
        [HideInInspector]
        public Quaternion qRotJoint;
    }
}