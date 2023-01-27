
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public abstract class GenericAttachmentState : SmartObjectSyncState
    {
        public Vector3 startPos;
        public Quaternion startRot;
        public Vector3 parentPos;
        public Quaternion parentRot;
        
        //these values are arbitrary, but they work pretty good for most pickups
        public float positionResyncThreshold = 0.015f;
        public float rotationResyncThreshold = 0.995f;
        public float lastResync = -1001f;

        public override void OnEnterState()
        {
            
        }

        public override void OnExitState()
        {

        }


        public override void OnInterpolationStart()
        {
            CalcParentTransform();
            startPos = CalcPos();
            startRot = CalcRot();
        }
        public override void Interpolate(float interpolation)
        {
            CalcParentTransform();
            transform.position = sync.HermiteInterpolatePosition(parentPos + parentRot * startPos, Vector3.zero, parentPos + parentRot * sync.pos, Vector3.zero, interpolation);
            transform.rotation = sync.HermiteInterpolateRotation(parentRot * startRot, Vector3.zero, parentRot * sync.rot, Vector3.zero, interpolation);
            if (sync.rigid)
            {
                sync.rigid.velocity = Vector3.zero;
                sync.rigid.angularVelocity = Vector3.zero;
            }
        }
        public override bool OnInterpolationEnd()
        {
            if (sync.IsLocalOwner())
            {
                if (ObjectMoved())
                {
                    if (lastResync + sync.lerpTime < Time.timeSinceLevelLoad)
                    {
                        sync.RequestSerialization();
                    }
                } else
                {
                    lastResync = Time.timeSinceLevelLoad;
                }
            }
            return true;
        }

        public override void OnSmartObjectSerialize()
        {
            CalcParentTransform();
            sync.pos = CalcPos();
            sync.rot = CalcRot();
            sync.vel = CalcVel();
            sync.spin = CalcSpin();
            lastResync = Time.timeSinceLevelLoad;
        }
        public abstract void CalcParentTransform();
        
        public Vector3 CalcPos()
        {
            return Quaternion.Inverse(parentRot) * (transform.position - parentPos);
        }
        public Quaternion CalcRot()
        {
            return Quaternion.Inverse(parentRot) * transform.rotation;
        }
        public Vector3 CalcVel()
        {
            return Vector3.zero;
        }
        public Vector3 CalcSpin()
        {
            return Vector3.zero;
        }

        public bool ObjectMoved()
        {
            return Vector3.Distance(CalcPos(), sync.pos) > positionResyncThreshold || Quaternion.Dot(CalcRot(), sync.rot) < rotationResyncThreshold;//arbitrary values to account for pickups wiggling a little in your hand
        }
    }
}