using System;
using Net.Common;

namespace Net.Component
{
    [Serializable]
    [Obsolete("请使用EntityTransform替代", false)]
    public class NTransform : EntityTransform 
    {
        public Vector3 position
        {
            get => Position;
            set => Position = value;
        }

        public Quaternion rotation
        {
            get => Rotation;
            set => Rotation = value;
        }

        public UnityEngine.Vector3 localScale
        {
            get => Scale;
        }

        public UnityEngine.Quaternion localRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 eulerAngles
        {
            get => rotation.eulerAngles;
            set => rotation = Quaternion.Euler(value);
        }

        public Vector3 left
        {
            get => Left;
        }

        public Vector3 right
        {
            get => Right;
        }

        public Vector3 up
        {
            get => Up;
        }

        public Vector3 down
        {
            get => Down;
        }

        public Vector3 forward
        {
            get => Forward;
        }

        public Vector3 back
        {
            get => Back;
        }

        public NTransform()
        {
        }

        public NTransform(Vector3 position, Quaternion rotation) : base(position, rotation)
        {
        }
    }
}