﻿
using MMMaellon;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.SmartObjectSyncExtra
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(SmartObjectSync)), DefaultExecutionOrder(101)]
    public class KinematicUntilDropped : SmartObjectSyncListener
    {
        [System.NonSerialized]
        public SmartObjectSync sync;
        bool kinematic = false;

        public bool startKinematic = true;
        public bool kinematicOnRespawn = true;
        public bool stopKinematicOnParticleCollision = true;
        void Start()
        {
            sync = GetComponent<SmartObjectSync>();
            sync.rigid.isKinematic = startKinematic;
            kinematic = startKinematic;
            sync.AddListener(this);
        }

        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            if (kinematic)
            {
                if (sync.IsHeld() && sync.kinematicWhileHeld)
                {
                    sync.lastKinematic = false;
                    kinematic = false;
                }
                else if (newState == SmartObjectSync.STATE_INTERPOLATING || newState == SmartObjectSync.STATE_FALLING)
                {
                    sync.rigid.isKinematic = false;
                    kinematic = false;
                }
            }
            else
            {
                if (newState == SmartObjectSync.STATE_TELEPORTING)
                {
                    kinematic = kinematicOnRespawn;
                    sync.rigid.isKinematic = kinematicOnRespawn;
                }
            }
        }

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {

        }
        public void OnParticleCollision(GameObject other)
        {
            if (!sync.IsLocalOwner() || !stopKinematicOnParticleCollision)
            {
                return;
            }
            if (kinematic)
            {
                sync.rigid.isKinematic = false;
                kinematic = false;
            }
        }
    }
}