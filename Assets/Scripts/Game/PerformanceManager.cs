﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static WallManager;

namespace Assets.Scripts.Game
{
    public class PerformanceManager : MonoBehaviour
    {

        private BasicPointer pointerData;
        private DiskMole moleData;
        private WallManager wallData;
        private PatternPlayer patternData;
        private float timeSinceLastShot = 0f;
        private bool isTimerRunning = false;
        private Vector3 lastPosition = Vector3.zero;
        private float speed = 0f;
        private float lastDistance = 0f;
        private float feedback = 0f;
        private float averageSpeed = 0f;
        private int nbShoot = 0;
        private Queue<float> lastSpeeds = new Queue<float>();
        private void Awake()
        {
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                timeSinceLastShot += Time.deltaTime;
            }

            CalculateSpeed();
        }

        private void ResetShoot()
        {
            timeSinceLastShot = 0f;
            speed = 0f;
            lastDistance = 0f;
        }

        public void OnPointerShoot()
        {
            isTimerRunning = false;
            CalculateFeedback();
        }

        public void onMoleActivated()
        {
            isTimerRunning = true;
            timeSinceLastShot = 0f;
            lastDistance= 0f;
        }


        public void UpdatePointerData(BasicPointer pointer)
        {
            // Now you have access to all public variables and methods of the BasicPointer instance
            pointerData = pointer;

        }

        public void UpdateMoleData(DiskMole mole)
        {
           
        }

        public void UpdateWallData(WallManager wall)
        {
            // Update information based on the wall
        }

        public void UpdatePatternData(PatternPlayer pattern)
        {
            // Update information based on the pattern
        }

        public void CalculateSpeed()
        {

            Vector3 position = pointerData.MappedPosition;
            if (lastPosition == Vector3.zero)
            {
                  lastPosition = position;
            }
            if (isTimerRunning)
            {
                float distance = Vector3.Distance(position, lastPosition);
                lastPosition = position;
                lastDistance = lastDistance + distance;
                speed = lastDistance / timeSinceLastShot;
            }
        }

        public float GetFeedback()
        {
            //Debug.Log("Feedback : " + feedback + " nbShoot " + nbShoot);
            return feedback;
        }

        public void CalculateFeedback()
        {
            float minDistance = 0.3f;
            lastSpeeds.Enqueue(speed);

            if (lastSpeeds.Count > 20)
            {
                lastSpeeds.Dequeue();
            }
            if (nbShoot < 5)
            {
                feedback = 1;
                averageSpeed = speed;
                nbShoot++;
            }
            else if (lastDistance <= minDistance)
            {
                feedback = 1;
            }
            else
            {
                averageSpeed = lastSpeeds.Average();
                //Debug.Log("Average speed : " + averageSpeed + " nbShoot " + nbShoot + " lastDistance : " + lastDistance + "timeSinceLastShot : " + timeSinceLastShot);
                nbShoot++;
                float thresholdUp = 1.50f * averageSpeed;
                float thresholdDown = 0.50f * averageSpeed;

                if (speed <= thresholdDown)
                {
                    feedback = 0;
                }
                else if (speed >= thresholdUp)
                {
                    feedback = 1;
                }
                else
                {
                    //Debug.Log(" In the scale : " + speed + " thresholdUp : " + thresholdUp + "thresholDown : " + thresholdDown + " nbShoot " + nbShoot);
                    feedback = (speed - thresholdDown) / (thresholdUp - thresholdDown);
                }

            }

        ResetShoot();
        }
    }
}