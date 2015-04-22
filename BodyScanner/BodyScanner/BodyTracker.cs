﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Author: Jack Roper
 * Created: 09/03/2015
 * Singleton Class for checking body pose prior to scan
*/


namespace BodyScanner
{
    sealed class BodyTracker
    {
        const double distanceTolerance = 0.1, jointTolerance = 0.2, targetDistanceFromCam = 2;

        private static BodyTracker _instance; // Static instance reference, implementing the Singleton pattern
        private static Object _dummyLock = new Object();

        private BodyFrame _bf; // should be updated on-access to this class
        private Body subject;

        private BodyTracker() { }

        public static BodyTracker UpdateInstance(BodyFrame bodyFrame)
        {
            // lock used to protect singleton from multithreaded access
            lock(_dummyLock)
            {
                if (_instance == null) { _instance = new BodyTracker(); }
            }
            _instance._bf = bodyFrame;
            return _instance;
        }

        // Control method that is interfaced with from outside the class
        public bool CorrectPose()
        {
            subject = retrieveBody();

            if (subject == null || !correctDistance(subject)) { return false; }

            if (!handsCheck(subject) || !feetCheck(subject)) { return false; }

            return true;
        }

        // Checks that hands are within a certain margin of each other on the Y axis
        private bool handsCheck(Body body)
        {
            Joint rightElbow = subject.Joints[JointType.ElbowRight]; // used for calculating arm length to measure tolerance
            Joint rightHand = subject.Joints[JointType.HandRight], leftHand = subject.Joints[JointType.HandLeft];
            Joint rightHip = subject.Joints[JointType.HipRight], leftHip = subject.Joints[JointType.HipLeft];

            if (rightHip.Position.Y < rightHand.Position.Y || leftHip.Position.Y < leftHand.Position.Y) { return false; }

            double forearmLength = jointDistance(rightElbow, rightHand);

            if (compareJointsAxis(PointCloud.Axis.Y, leftHand, rightHand) > jointTolerance * forearmLength) { return false; }

            return true;
        }

        // Checks that feet are within a certain margin of each other on the Y axis
        private bool feetCheck(Body body)
        {
            Joint rightKnee = subject.Joints[JointType.ElbowRight];
            Joint rightFoot = subject.Joints[JointType.FootRight]; 
            Joint leftFoot = subject.Joints[JointType.FootLeft];

            double lowerLegLength = jointDistance(rightKnee, rightFoot);

            if (compareJointsAxis(PointCloud.Axis.Y, leftFoot, rightFoot) > jointTolerance * lowerLegLength) { return false; }

            return true;
        }

        // Refreshes body data and calls to return 
        private Body retrieveBody()
        {
            Body[] bodyArray = new Body[_bf.BodyFrameSource.BodyCount];
            _bf.GetAndRefreshBodyData(bodyArray);
            Body subject = findClosestBody(bodyArray);
            return subject;
        }

        // Distance from camera (just depth) to ensure subject isn't too far or too close
        private bool correctDistance(Body body)
        {
            double actualDistanceFromCam = getDistance(subject);
            double difference = actualDistanceFromCam - targetDistanceFromCam;

            if (Math.Abs(difference) > distanceTolerance)
            {
                // Replace with some sort of visual feedback for liveness
                Log.Write("INCORRECT DISTANCE FROM CAMERA! You're " + difference + " metres away from target.");
                return false;
            }
            else
                Log.Write("CORRECT DISTANCE: " + actualDistanceFromCam);

            return true;
        }

        // Simple look-up method just for quick access
        private double getDistance(Body body)
        {
            return body.Joints[JointType.SpineBase].Position.Z;
        }

        // Finds distance between two joints on the specified axis
        private double compareJointsAxis(PointCloud.Axis axis, Joint jointA,  Joint jointB)
        {
            double result = 0;
            switch(axis)
            {
                case (PointCloud.Axis.X):
                    result = Math.Abs(jointA.Position.X - jointB.Position.X); break;
                case (PointCloud.Axis.Y):
                    result = Math.Abs(jointA.Position.Y - jointB.Position.Y); break;
                case (PointCloud.Axis.Z):
                    result = Math.Abs(jointA.Position.Z - jointB.Position.Z); break;
            }
            return result;
        }

        // Calculates distance for each tracked body and returns one with shortest
        private Body findClosestBody(Body[] bodies)
        {
            Body closestBody = null;
            double lowestDist = double.MaxValue;
            foreach (Body body in bodies)
            {
                if (body != null && body.IsTracked)
                {
                    CameraSpacePoint bodyPosition = body.Joints[JointType.SpineBase].Position;

                    double dist = Math.Pow(bodyPosition.X, 2) + Math.Pow(bodyPosition.Y, 2) + Math.Pow(bodyPosition.Z, 2);
                    if (dist < lowestDist)
                    {
                        lowestDist = dist;
                        closestBody = body;
                    }
                }
            }
            return closestBody;
        }

        // Returns euclidean distance between two joints
        private double jointDistance(Joint jointA, Joint jointB)
        {
            return Math.Sqrt(   Math.Pow((jointA.Position.X - jointB.Position.X), 2) +
                                Math.Pow((jointA.Position.Y - jointB.Position.Y), 2) +
                                Math.Pow((jointA.Position.Z - jointB.Position.Z), 2)        );
        }
    }
}
