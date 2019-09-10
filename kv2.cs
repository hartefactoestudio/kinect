using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    public GameObject RotateObject;
    public Transform angle;
    public float Rangle;
    public float LastRangle;

    public float zangle;
    public float zLastRangle;

    public float speed= .1f;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private void Start()
    {
        RotateObject = GameObject.Find("rotate");
        RotateObject.transform.rotation = angle.rotation;
        
    } 
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
                
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

               
                 
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

            
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)

    {
       
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
                
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            Transform jointObj2 = bodyObject.transform.Find(Kinect.JointType.Head.ToString());

          
            Vector3 headposition = new Vector3(jointObj2.position.x,jointObj2.position.y,jointObj2.position.z);
            //Debug.Log(headposition.x + "=head vector");
            if (headposition.x >= -6 && headposition.x <= 6)
            {
                // Debug.Log(headposition.x + "=head vector");
                Debug.Log(headposition.y);
                //  Debug.Log("sumar n al angulo ||");

                if (headposition.x >= -6 && headposition.x <= -5.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                    
                }
                else if (headposition.x >= -5.5 && headposition.x <= -5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= -5 && headposition.x <= -4.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
               
                else if (headposition.x >= -4.5 && headposition.x <= -4)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= -4 && headposition.x <= -3.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;

                    
                }
                else if (headposition.x >= -3.5 && headposition.x <= -3)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= -3 && headposition.x <= -2.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= -2.5 && headposition.x <= -2)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= -2 && headposition.x <= -1.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= -1.5 && headposition.x <= -1)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= -1 && headposition.x <= -.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= -.5 && headposition.x <= 0)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                  
                }

                else if (headposition.x >= 0 && headposition.x <= .5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= .5 && headposition.x <= 1)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= 1 && headposition.x <= 1.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= 1.5 && headposition.x <= 2)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= 2 && headposition.x <= 2.5)

                {
                    Rangle = LastRangle;
                    Rangle =-headposition.x;
                   
                }
                else if (headposition.x >= 2.5 && headposition.x <= 3)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= 3 && headposition.x <= 3.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }
                else if (headposition.x >= 3.5 && headposition.x <= 4)

                {
                    Rangle = LastRangle;
                    Rangle =-headposition.x;
                   
                }
                else if (headposition.x >= 4 && headposition.x <= 4.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    Debug.Log(Rangle);
                }
                else if (headposition.x >= 4.5 && headposition.x <= 5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= 5 && headposition.x <= 5.5)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                   
                }
                else if (headposition.x >= 5.5 && headposition.x <= 6)

                {
                    Rangle = LastRangle;
                    Rangle = -headposition.x;
                    
                }

                angle.rotation = Quaternion.Lerp(Quaternion.Euler(0, LastRangle, 0), Quaternion.Euler(0, Rangle, 0), Time.time *1f); 
            }
            else {
                Debug.Log("head out of range");
            }
            

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
