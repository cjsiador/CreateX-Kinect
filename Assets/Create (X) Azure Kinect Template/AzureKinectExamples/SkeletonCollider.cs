using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class SkeletonCollider : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Game object used to represent the body joints.")]
        public GameObject jointPrefab;

        [Tooltip("Line object used to represent the bones between joints.")]
        //public LineRenderer linePrefab;
        public GameObject linePrefab;

        [Tooltip("Scene object that will be used to represent the sensor's position and rotation in the scene.")]
        public Transform sensorTransform;   //Note: Using this would would not have the accurate Joint Rotation (Joint Orientation) if this Transform rotated... Use with cautious. You can use the SkeletonColider to move instead. 

        [Tooltip("Body scale factors in X,Y,Z directions.")]
        public Vector3 scaleFactors = Vector3.one;

        public SkeletonBody skeletonBody;

        [Header("Direction of the Skeleton's x Joint.")]
        public JointDirectionType MirrorXJointDirection = JointDirectionType.MirrorCameraBehind;
        public enum JointDirectionType : int    //Note: It depends on the camera based on Azure4Kinect0 if Multicamera Configuration was used. 
        {
            MirrorCameraBehind = 1,         // If the camera is behind of the player and you want ot mirror their movement. 
            MirrorCameraInfront = 2         // If the camera is infront of the player and you want to mirror their movement.
        }

        [Tooltip("Whenever to ignore the miror Z Joint Direction if you're moving towards are back of the camera.")]
        public bool InvertZJointDirection;

        private GameObject[] tempLineGameObject;
        private GameObject[] tempJoints;


        void Start()
        {
            InstanceSkeletonObj();
        }

        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (kinectManager && kinectManager.IsInitialized() && skeletonBody.joints != null && skeletonBody.lineGameObject != null)
            {
                // overlay all joints in the skeleton
                if (kinectManager.IsUserDetected(playerIndex))
                {
                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);
                    int jointsCount = kinectManager.GetJointCount();

                    for (int i = 0; i < jointsCount; i++)
                    {
                        int joint = i;

                        if (kinectManager.IsJointTracked(userId, joint))
                        {
                            Vector3 positionJoint = !sensorTransform ? kinectManager.GetJointPosition(userId, joint) : kinectManager.GetJointKinectPosition(userId, joint, true);
                            positionJoint = new Vector3(positionJoint.x * scaleFactors.x, positionJoint.y * scaleFactors.y, positionJoint.z * scaleFactors.z);

                            if (sensorTransform)    // If sensorTransform was set.
                            {
                                positionJoint = sensorTransform.transform.TransformPoint(positionJoint);
                            }

                            if (skeletonBody.joints != null)
                            {
                                // overlay the joint
                                if (positionJoint != Vector3.zero)
                                {
                                    skeletonBody.joints[i].SetActive(true);

                                    // Camera Behind of the Player.
                                    if (MirrorXJointDirection == JointDirectionType.MirrorCameraBehind)
                                    {
                                        if (InvertZJointDirection)    // Don't Ignore Z Mirror Direction.
                                        {
                                            skeletonBody.joints[i].transform.localPosition = new Vector3(-positionJoint.x, positionJoint.y, -positionJoint.z);
                                        }
                                        if (!InvertZJointDirection)     // Ignore Z Mirror Direciton.
                                        {
                                            skeletonBody.joints[i].transform.localPosition = new Vector3(-positionJoint.x, positionJoint.y, positionJoint.z);
                                        }

                                    }

                                    // Camera Infront of the Player
                                    if (MirrorXJointDirection == JointDirectionType.MirrorCameraInfront)
                                    {
                                        if (InvertZJointDirection)    // Don't Ignore Z Mirror Direction.
                                        {
                                            skeletonBody.joints[i].transform.localPosition = new Vector3(positionJoint.x, positionJoint.y, positionJoint.z);
                                        }

                                        if (!InvertZJointDirection)     // Ignore Z Mirror Direciton.
                                        {
                                            skeletonBody.joints[i].transform.localPosition = new Vector3(positionJoint.x, positionJoint.y, -positionJoint.z);
                                        }
                                    }
                                }
                                else
                                {
                                    skeletonBody.joints[i].SetActive(false);
                                }
                            }

                            if (skeletonBody.lineGameObject[i] == null && linePrefab != null)
                            {
                                skeletonBody.lineGameObject[i] = Instantiate(linePrefab);  // as LineRenderer;
                                skeletonBody.lineGameObject[i].transform.parent = transform;
                                skeletonBody.lineGameObject[i].name = ((KinectInterop.JointType)i).ToString() + "_Line"; // name line object based on their joints.
                                skeletonBody.lineGameObject[i].SetActive(false);
                            }

                            if (skeletonBody.lineGameObject[i] != null)
                            {
                                // overlay the line to the parent joint
                                int jointParent = (int)kinectManager.GetParentJoint((KinectInterop.JointType)joint);
                                Vector3 positionParent = Vector3.zero;

                                if (kinectManager.IsJointTracked(userId, jointParent))
                                {
                                    positionParent = !sensorTransform ? kinectManager.GetJointPosition(userId, jointParent) : kinectManager.GetJointKinectPosition(userId, jointParent, true);
                                    
                                    positionJoint = new Vector3(positionJoint.x * scaleFactors.x, positionJoint.y * scaleFactors.y, positionJoint.z * scaleFactors.z);

                                    if (sensorTransform)
                                    {
                                        positionParent = sensorTransform.transform.TransformPoint(positionParent);
                                    }
                                }

                                if (positionJoint != Vector3.zero && positionParent != Vector3.zero)
                                {
                                    skeletonBody.lineGameObject[i].SetActive(true);

                                    // If the camera is behind the player, then this would change the position of the x direction 
                                    // and z direction of the joints (player movement) as if the player is looking at a mirror. 
                                    if (MirrorXJointDirection == JointDirectionType.MirrorCameraBehind)
                                    {
                                        positionJoint.x = -positionJoint.x;
                                        positionParent.x = -positionParent.x;

                                        if (InvertZJointDirection)
                                        {
                                            positionJoint.z = -positionJoint.z;
                                            positionParent.z = -positionParent.z;
                                        }
                                    }

                                    // If the camera is infront the player, then this would leave the x position as positive
                                    // and the z direction of the joints as positive.
                                    if (MirrorXJointDirection == JointDirectionType.MirrorCameraInfront)
                                    {
                                        if (!InvertZJointDirection) // Set the z position to a negative z position.
                                        {
                                            positionJoint.z = -positionJoint.z;
                                            positionParent.z = -positionParent.z;
                                        }
                                    }

                                    // Align the lines (or bones) from a joint to its parent joint. 
                                    Vector3 dirFromParent = positionJoint - positionParent;
                                    skeletonBody.lineGameObject[i].transform.localPosition = positionParent + dirFromParent / 2f;
                                    skeletonBody.lineGameObject[i].transform.up = transform.rotation * dirFromParent.normalized;
                                    Vector3 lineScale = skeletonBody.lineGameObject[i].transform.localScale;
                                    skeletonBody.lineGameObject[i].transform.localScale = new Vector3(lineScale.x, dirFromParent.magnitude / 2f, lineScale.z);
                                }
                                else
                                {
                                    skeletonBody.lineGameObject[i].SetActive(false);
                                }
                            }

                        }
                        else
                        {
                            if (skeletonBody.joints[i] != null)
                            {
                                skeletonBody.joints[i].SetActive(false);
                            }

                            if (skeletonBody.lineGameObject[i] != null)
                            {
                                skeletonBody.lineGameObject[i].SetActive(false);
                            }
                        }
                    }

                }
                else
                {
                    // user not detected - hide joints and lines
                    int jointsCount = kinectManager.GetJointCount();

                    for (int i = 0; i < jointsCount; i++)
                    {
                        if (skeletonBody.joints[i] != null && skeletonBody.joints[i].activeSelf)
                        {
                            skeletonBody.joints[i].SetActive(false);
                        }

                        if (skeletonBody.lineGameObject[i] != null && skeletonBody.lineGameObject[i].activeSelf)
                        {
                            skeletonBody.lineGameObject[i].SetActive(false);
                        }
                    }
                }

            }
        }

        ///<summary>
        /// This funciton would instantiate Skeleton Object if joint prefab or line prefab had been set.
        ///</summary>
        public void InstanceSkeletonObj()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (kinectManager && kinectManager.IsInitialized())
            {
                int jointsCount = kinectManager.GetJointCount();

                if (jointPrefab)
                {
                    if (skeletonBody.joints.Length < jointsCount)
                    {
                        tempJoints = skeletonBody.joints;

                        // array holding the skeleton joints
                        skeletonBody.joints = new GameObject[jointsCount];
                        for (int i = 0; i < tempJoints.Length; i++)
                        {
                            if (tempJoints[i] != null)
                                skeletonBody.joints[i] = tempJoints[i];
                        }
                    }
                    
                    // array holding the skeleton joints
                    //skeletonBody.joints = new GameObject[jointsCount];

                    for (int i = 0; i < skeletonBody.joints.Length; i++)
                    {
                        if (skeletonBody.joints[i] == null)
                        {
                            skeletonBody.joints[i] = Instantiate(jointPrefab) as GameObject;
                            skeletonBody.joints[i].transform.parent = transform;
                            skeletonBody.joints[i].name = ((KinectInterop.JointType)i).ToString();
                            skeletonBody.joints[i].SetActive(false);
                        }
                    }
                }

                // If the lineGameObject length is less that 32 joint, add a new list until it reaches 32. If there is a gameobject on the array, then copy and paste into a fixed 32 array. 
                if (skeletonBody.lineGameObject.Length < jointsCount)
                {
                    tempLineGameObject = skeletonBody.lineGameObject;
                    skeletonBody.lineGameObject = new GameObject[jointsCount];
    
                    for (int i = 0; i < tempLineGameObject.Length; i++)
                    {
                        if (tempLineGameObject[i] != null)
                            skeletonBody.lineGameObject[i] = tempLineGameObject[i];
                    }
                }
                // array holding the skeleton lines
                //skeletonBody.lineGameObject = new GameObject[jointsCount];
            }
        }

        [System.Serializable]
        public struct SkeletonBody
        {
            public GameObject[] joints;
            //public LineRenderer[] lineRenderer;
            public GameObject[] lineGameObject;
        }
    }
}

