﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class SoundEffects : SmartObjectSyncListener
    {
        public AudioSource[] audioSources;

        [Header("Default State Sounds")]
        public AudioClip[] respawnSounds;
        public AudioClip[] collisionSounds;
        public AudioClip[] grabSounds;
        public AudioClip[] throwSounds;
        public float respawnVol = 1.0f;
        public float collisionVol = 1.0f;
        public float grabVol = 1.0f;
        public float throwVol = 1.0f;
        public float minVolumeCollisionVelocity = 0.0f;
        public float maxVolumeCollisionVelocity = 5f;
        public float cooldown = 0.1f;

        [Header("Custom State Sounds")]
        public AudioClip[] attachToPlayerSounds;
        public AudioClip[] customState1Sounds;
        public AudioClip[] customState2Sounds;
        public AudioClip[] customState3Sounds;
        public float attachToPlayerVol = 1.0f;
        public float custom1Vol = 1.0f;
        public float custom2Vol = 1.0f;
        public float custom3Vol = 1.0f;
        public string customState1 = "";
        public string customState2 = "";
        public string customState3 = "";
        public void Start()
        {
            if (minVolumeCollisionVelocity > maxVolumeCollisionVelocity)
            {
                var temp = maxVolumeCollisionVelocity;
                maxVolumeCollisionVelocity = minVolumeCollisionVelocity;
                minVolumeCollisionVelocity = temp;
            }
        }

        public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            //do nothing
        }

        AudioClip nextClip;
        float volumeScale;
        float lastSound = -1001f;
        string typeName;
        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            if (lastSound + cooldown > Time.timeSinceLevelLoad)
            {
                return;
            }
            lastSound = Time.timeSinceLevelLoad;
            sync = s;
            if (newState == SmartObjectSync.STATE_TELEPORTING)
            {
                nextClip = RandomClip(respawnSounds);
                volumeScale = respawnVol;
            }
            else if (sync.IsHeld())
            {
                nextClip = RandomClip(grabSounds);
                volumeScale = grabVol;
            }
            else if (oldState == SmartObjectSync.STATE_LEFT_HAND_HELD || oldState == SmartObjectSync.STATE_RIGHT_HAND_HELD || oldState == SmartObjectSync.STATE_NO_HAND_HELD)
            {
                nextClip = RandomClip(throwSounds);
                volumeScale = throwVol;
            }
            else if((newState == SmartObjectSync.STATE_FALLING || newState == SmartObjectSync.STATE_INTERPOLATING)) {
                nextClip = RandomClip(collisionSounds);
                if (nextClip)
                {
                    if (sync.rigid.velocity.magnitude < minVolumeCollisionVelocity)
                    {
                        nextClip = null;//don't play anything
                    } else if (sync.rigid.velocity.magnitude >= maxVolumeCollisionVelocity)
                    {
                        volumeScale = collisionVol;
                    } else
                    {
                        volumeScale = collisionVol * ((sync.rigid.velocity.magnitude - minVolumeCollisionVelocity) / (maxVolumeCollisionVelocity - minVolumeCollisionVelocity));
                    }
                }
            } else if (newState < 0 || newState == SmartObjectSync.STATE_WORLD_LOCK || newState == SmartObjectSync.STATE_ATTACHED_TO_PLAYSPACE)
            {

            } else if(Utilities.IsValid(sync.customState))
            {
                typeName = sync.customState.GetUdonTypeName();
                if (typeName == customState1){
                    nextClip = RandomClip(customState1Sounds);
                    volumeScale = custom1Vol;
                }
                else if (typeName == customState2)
                {
                    nextClip = RandomClip(customState2Sounds);
                    volumeScale = custom2Vol;
                } else if (typeName == customState3)
                {
                    nextClip = RandomClip(customState3Sounds);
                    volumeScale = custom3Vol;
                }
            }
            if (nextClip)
            {
                audioSource.PlayOneShot(nextClip, volumeScale);
                if (clipEnd < Time.timeSinceLevelLoad)
                {
                    //start the loop again if the last clip ended
                    clipEnd = Time.timeSinceLevelLoad + nextClip.length;//in both branches because it needs to happen after the if statement, but before the call to StayWithObject()
                    StayWithObject();
                } else
                {
                    clipEnd = Time.timeSinceLevelLoad + nextClip.length;
                }
            }
        }

        float clipEnd = -1001f;
        SmartObjectSync sync;
        public void StayWithObject()
        {
            transform.position = sync.transform.position;
            if (clipEnd > Time.timeSinceLevelLoad)
            {
                SendCustomEventDelayedFrames(nameof(StayWithObject), 0);
            }
        }

        int audioSourceIndex = 0;
        AudioSource lastAudioSource = null;
        public AudioSource audioSource
        {
            get
            {
                audioSourceIndex = (audioSourceIndex + 1) % audioSources.Length;
                lastAudioSource = audioSources[audioSourceIndex];
                return lastAudioSource;
            }
        }

        public AudioClip RandomClip(AudioClip[] clips)
        {
            if (clips.Length == 0)
            {
                return null;
            }
            return clips[Random.Range(0, clips.Length)];
        }
    }
}