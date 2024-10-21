using System;
using System.Collections;
using System.Collections.Generic;
using Net.Common;

namespace Net.Entities
{
    public class Transform : Component, IEnumerable
    {
        internal List<Transform> childs = new List<Transform>();

        internal Transform()
        {
        }

        public Vector3 position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Vector3 localPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                return this.rotation.eulerAngles;
            }
            set
            {
                this.rotation = Quaternion.Euler(value);
            }
        }

        public Vector3 localEulerAngles
        {
            get
            {
                return this.localRotation.eulerAngles;
            }
            set
            {
                this.localRotation = Quaternion.Euler(value);
            }
        }

        public Vector3 right
        {
            get
            {
                return this.rotation * Vector3.right;
            }
            set
            {
                this.rotation = Quaternion.FromToRotation(Vector3.right, value);
            }
        }

        public Vector3 up
        {
            get
            {
                return this.rotation * Vector3.up;
            }
            set
            {
                this.rotation = Quaternion.FromToRotation(Vector3.up, value);
            }
        }

        public Vector3 forward
        {
            get
            {
                return this.rotation * Vector3.forward;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Quaternion rotation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Quaternion localRotation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Vector3 localScale
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Transform parent
        {
            get;
            set;
        }

        public void SetParent(Transform p)
        {
            this.SetParent(p, true);
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            this.parent = parent;
        }

        public Matrix4x4 worldToLocalMatrix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Matrix4x4 localToWorldMatrix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void SetLocalPositionAndRotation(Vector3 localPosition, Quaternion localRotation)
        {
            this.position = localPosition;
            this.rotation = localRotation;
        }


        public void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            throw new NotImplementedException();
        }


        public void GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation)
        {
            throw new NotImplementedException();
        }

        public void Translate(Vector3 translation, [DefaultValue("Space.Self")] Space relativeTo)
        {
            bool flag = relativeTo == Space.World;
            if (flag)
            {
                this.position += translation;
            }
            else
            {
                this.position += this.TransformDirection(translation);
            }
        }

        public void Translate(Vector3 translation)
        {
            this.Translate(translation, Space.Self);
        }

        public void Translate(float x, float y, float z, [DefaultValue("Space.Self")] Space relativeTo)
        {
            this.Translate(new Vector3(x, y, z), relativeTo);
        }

        public void Translate(float x, float y, float z)
        {
            this.Translate(new Vector3(x, y, z), Space.Self);
        }

        public void Translate(Vector3 translation, Transform relativeTo)
        {
            if (relativeTo != null)
            {
                this.position += relativeTo.TransformDirection(translation);
            }
            else
            {
                this.position += translation;
            }
        }

        public void Translate(float x, float y, float z, Transform relativeTo)
        {
            this.Translate(new Vector3(x, y, z), relativeTo);
        }

        public void Rotate(Vector3 eulers, [DefaultValue("Space.Self")] Space relativeTo)
        {
            Quaternion rhs = Quaternion.Euler(eulers.x, eulers.y, eulers.z);
            bool flag = relativeTo == Space.Self;
            if (flag)
            {
                this.localRotation *= rhs;
            }
            else
            {
                this.rotation *= Quaternion.Inverse(this.rotation) * rhs * this.rotation;
            }
        }

        public void Rotate(Vector3 eulers)
        {
            this.Rotate(eulers, Space.Self);
        }

        public void Rotate(float xAngle, float yAngle, float zAngle, [DefaultValue("Space.Self")] Space relativeTo)
        {
            this.Rotate(new Vector3(xAngle, yAngle, zAngle), relativeTo);
        }

        public void Rotate(float xAngle, float yAngle, float zAngle)
        {
            this.Rotate(new Vector3(xAngle, yAngle, zAngle), Space.Self);
        }

        public void Rotate(Vector3 axis, float angle, [DefaultValue("Space.Self")] Space relativeTo)
        {
            bool flag = relativeTo == Space.Self;
            if (flag)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Rotate(Vector3 axis, float angle)
        {
            this.Rotate(axis, angle, Space.Self);
        }

        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            throw new NotImplementedException();
        }

        public void LookAt(Transform target, [DefaultValue("Vector3.up")] Vector3 worldUp)
        {
            if (target != null)
            {
                this.LookAt(target.position, worldUp);
            }
        }

        public void LookAt(Transform target)
        {
            if (target != null)
            {
                this.LookAt(target.position, Vector3.up);
            }
        }

        public void LookAt(Vector3 worldPosition, [DefaultValue("Vector3.up")] Vector3 worldUp)
        {
            this.Internal_LookAt(worldPosition, worldUp);
        }

        public void LookAt(Vector3 worldPosition)
        {
            this.Internal_LookAt(worldPosition, Vector3.up);
        }

        private void Internal_LookAt(Vector3 worldPosition, Vector3 worldUp)
        {
            throw new NotImplementedException();
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            throw new NotImplementedException();
        }

        public Vector3 TransformDirection(float x, float y, float z)
        {
            return this.TransformDirection(new Vector3(x, y, z));
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            throw new NotImplementedException();
        }

        public Vector3 InverseTransformDirection(float x, float y, float z)
        {
            return this.InverseTransformDirection(new Vector3(x, y, z));
        }

        public Vector3 TransformVector(Vector3 vector)
        {
            throw new NotImplementedException();
        }

        public Vector3 TransformVector(float x, float y, float z)
        {
            return this.TransformVector(new Vector3(x, y, z));
        }

        public Vector3 InverseTransformVector(Vector3 vector)
        {
            throw new NotImplementedException();
        }

        public Vector3 InverseTransformVector(float x, float y, float z)
        {
            return this.InverseTransformVector(new Vector3(x, y, z));
        }

        public Vector3 TransformPoint(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Vector3 TransformPoint(float x, float y, float z)
        {
            return this.TransformPoint(new Vector3(x, y, z));
        }

        public Vector3 InverseTransformPoint(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Vector3 InverseTransformPoint(float x, float y, float z)
        {
            return this.InverseTransformPoint(new Vector3(x, y, z));
        }

        public Transform root
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int childCount { get; internal set; }

        public void DetachChildren()
        {
            throw new NotImplementedException();
        }


        public void SetAsFirstSibling()
        {
            throw new NotImplementedException();
        }


        public void SetAsLastSibling()
        {
            throw new NotImplementedException();
        }


        public void SetSiblingIndex(int index)
        {
            throw new NotImplementedException();
        }

        public int GetSiblingIndex()
        {
            throw new NotImplementedException();
        }

        public Transform Find(string n)
        {
            throw new NotImplementedException();
        }

        public Vector3 lossyScale
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsChildOf([NotNull("ArgumentNullException")] Transform parent)
        {
            throw new NotImplementedException();
        }

        public bool hasChanged { get; set; }

        [Obsolete("FindChild has been deprecated. Use Find instead (UnityUpgradable) -> Find([mscorlib] System.String)", false)]
        public Transform FindChild(string n)
        {
            return this.Find(n);
        }

        public IEnumerator GetEnumerator()
        {
            return new Transform.Enumerator(this);
        }

        public Transform GetChild(int index)
        {
            throw new NotImplementedException();
        }

        public int GetChildCount()
        {
            throw new NotImplementedException();
        }

        public int hierarchyCapacity
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int hierarchyCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private class Enumerator : IEnumerator
        {
            private Transform outer;

            private int currentIndex = -1;

            internal Enumerator(Transform outer)
            {
                this.outer = outer;
            }

            public object Current
            {
                get
                {
                    return this.outer.GetChild(this.currentIndex);
                }
            }

            public bool MoveNext()
            {
                int childCount = this.outer.childCount;
                int num = this.currentIndex + 1;
                this.currentIndex = num;
                return num < childCount;
            }

            public void Reset()
            {
                this.currentIndex = -1;
            }
        }
    }
}
