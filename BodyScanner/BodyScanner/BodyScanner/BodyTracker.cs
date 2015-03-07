﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BodyScanner
{
    static class BodyTracker
    {
        const double tolerance = 0.2;

        public static bool CorrectPose(BodyFrame bf)
        {
            Body[] bodyArray = new Body[bf.BodyFrameSource.BodyCount];
            bf.GetAndRefreshBodyData(bodyArray);
            Body subject = FindClosestBody(bodyArray);

            if (subject == null) { return false; }

            Joint rightKnee = subject.Joints[JointType.KneeRight];
            Joint rightElbow = subject.Joints[JointType.ElbowRight];
            Joint rightHand = subject.Joints[JointType.HandRight], leftHand = subject.Joints[JointType.HandLeft];
            Joint rightFoot = subject.Joints[JointType.FootRight], leftFoot = subject.Joints[JointType.FootLeft];
            double forearmLength = DistanceBetween(rightElbow, rightHand);
            double lowerLegLength = DistanceBetween(rightKnee, rightFoot);

            // Check difference between hand heights relative to arm length && diff between feet height relative to leg length
            if (CompareJoints(PointCloud.Axis.Y, leftHand, rightHand) < tolerance * forearmLength &&
               CompareJoints(PointCloud.Axis.Y, leftFoot, rightFoot) < tolerance * lowerLegLength)
            {
                return true;
            }
          
            return false;
        }

        public static double CompareJoints(PointCloud.Axis axis, Joint jointA,  Joint jointB)
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

        public static Body FindClosestBody(Body[] bodies)
        {
            Body closestBody = null;
            double lowestDist = double.MaxValue;
            foreach (Body b in bodies)
            {
                if (b != null && b.IsTracked)
                {
                    CameraSpacePoint bodyPosition = b.Joints[JointType.SpineBase].Position;

                    double dist = Math.Pow(bodyPosition.X, 2) + Math.Pow(bodyPosition.Y, 2) + Math.Pow(bodyPosition.Z, 2);
                    if (dist < lowestDist)
                    {
                        lowestDist = dist;
                        closestBody = b;
                    }
                }
            }
            return closestBody;
        }

        public static Vector3D GetJointPosition(Body body, JointType joint)
        {
            return new Vector3D(body.Joints[joint].Position.X,
                                body.Joints[joint].Position.Y,
                                body.Joints[joint].Position.Z);
        }

        public static double DistanceBetween(Joint jointA, Joint jointB)
        {
            return Math.Sqrt(   Math.Pow((jointA.Position.X - jointB.Position.X), 2) +
                                Math.Pow((jointA.Position.Y - jointB.Position.Y), 2) +
                                Math.Pow((jointA.Position.Z - jointB.Position.Z), 2)        );
        }
    }
}