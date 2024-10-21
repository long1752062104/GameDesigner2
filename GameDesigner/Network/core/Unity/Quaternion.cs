using Net.Component;
using System;

namespace Net
{
    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>
    {
        #region "源码"
        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float this[int index]
        {
            get
            {
                float result;
                switch (index)
                {
                    case 0:
                        result = x;
                        break;
                    case 1:
                        result = y;
                        break;
                    case 2:
                        result = z;
                        break;
                    case 3:
                        result = w;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
                return result;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    case 3:
                        w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }
        }

        public void Set(float newX, float newY, float newZ, float newW)
        {
            x = newX;
            y = newY;
            z = newZ;
            w = newW;
        }

        public static Quaternion identity
        {
            get
            {
                return identityQuaternion;
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                Matrix4x4 m = QuaternionToMatrix(this);
                return MatrixToEuler(m) * 180f / Mathf.PI;
            }
            set
            {
                this = Euler(value);
            }
        }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 result;
            result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
            return result;
        }

        private static bool IsEqualUsingDot(float dot)
        {
            return dot > 0.999999f;
        }

        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return lhs.x == rhs.x & lhs.y == rhs.y & lhs.z == rhs.z & lhs.w == rhs.w;
        }

        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return !(lhs == rhs);
        }

        public static float Dot(Quaternion a, Quaternion b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static float Angle(Quaternion a, Quaternion b)
        {
            float num = Dot(a, b);
            return (!IsEqualUsingDot(num)) ? (Mathf.Acos(Mathf.Min(Mathf.Abs(num), 1f)) * 2f * 57.29578f) : 0f;
        }

        private static Vector3 Internal_MakePositive(Vector3 euler)
        {
            float num = -0.005729578f;
            float num2 = 360f + num;
            if (euler.x < num)
            {
                euler.x += 360f;
            }
            else if (euler.x > num2)
            {
                euler.x -= 360f;
            }
            if (euler.y < num)
            {
                euler.y += 360f;
            }
            else if (euler.y > num2)
            {
                euler.y -= 360f;
            }
            if (euler.z < num)
            {
                euler.z += 360f;
            }
            else if (euler.z > num2)
            {
                euler.z -= 360f;
            }
            return euler;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            float num = Mathf.Sqrt(Dot(q, q));
            Quaternion result;
            if (num < Mathf.Epsilon)
            {
                result = identity;
            }
            else
            {
                result = new Quaternion(q.x / num, q.y / num, q.z / num, q.w / num);
            }
            return result;
        }

        public void Normalize()
        {
            this = Normalize(this);
        }

        [Newtonsoft_X.Json.JsonIgnore]
        public Quaternion normalized
        {
            get
            {
                return Normalize(this);
            }
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2 ^ w.GetHashCode() >> 1;
        }

        public override bool Equals(object other)
        {
            return other is Quaternion quaternion && Equals(quaternion);
        }

        public bool Equals(Quaternion other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
            {
                x,
                y,
                z,
                w
            });
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", new object[]
            {
                x.ToString(format),
                y.ToString(format),
                z.ToString(format),
                w.ToString(format)
            });
        }

        public float x;
        public float y;
        public float z;
        public float w;
        private static readonly Quaternion identityQuaternion = new Quaternion(0f, 0f, 0f, 1f);
        public static readonly Quaternion zero = new Quaternion(0, 0, 0, 0);

        public const float kEpsilon = 1E-06f;
        #endregion

        #region 网上代码

        public Quaternion(float angle, Vector3 rkAxis)
        {
            float num1 = angle * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            x = rkAxis.x * num2;
            y = rkAxis.y * num2;
            z = rkAxis.z * num2;
            w = num3;
        }

        public Quaternion(Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            Matrix4x4 identityM = Matrix4x4.identity;
            identityM[0, 0] = xaxis.x;
            identityM[1, 0] = xaxis.y;
            identityM[2, 0] = xaxis.z;
            identityM[0, 1] = yaxis.x;
            identityM[1, 1] = yaxis.y;
            identityM[2, 1] = yaxis.z;
            identityM[0, 2] = zaxis.x;
            identityM[1, 2] = zaxis.y;
            identityM[2, 2] = zaxis.z;
            CreateFromRotationMatrix(ref identityM, out this);
        }

        public Quaternion(float yaw, float pitch, float roll)
        {
            float num1 = roll * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin(num4);
            float num6 = (float)Math.Cos(num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin(num7);
            float num9 = (float)Math.Cos(num7);
            x = (float)(num9 * (double)num5 * num3 + num8 * (double)num6 * num2);
            y = (float)(num8 * (double)num6 * num3 - num9 * (double)num5 * num2);
            z = (float)(num9 * (double)num6 * num2 - num8 * (double)num5 * num3);
            w = (float)(num9 * (double)num6 * num3 + num8 * (double)num5 * num2);
        }

        public float LengthSquared()
        {
            return x * x + y * y + z * z + w * w;
        }

        public float Length()
        {
            return (float)Math.Sqrt(x * (double)x + y * (double)y + z * (double)z +
                                     w * (double)w);
        }

        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float)Math.Sqrt(quaternion.x * (double)quaternion.x + quaternion.y * (double)quaternion.y +
                                               quaternion.z * (double)quaternion.z + quaternion.w * (double)quaternion.w);
            result.x = quaternion.x * num;
            result.y = quaternion.y * num;
            result.z = quaternion.z * num;
            result.w = quaternion.w * num;
        }

        public static Quaternion Inverse(Quaternion quaternion)
        {
            float num = 1f / (float)(quaternion.x * (double)quaternion.x + quaternion.y * (double)quaternion.y +
                quaternion.z * (double)quaternion.z + quaternion.w * (double)quaternion.w);
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x * num;
            quaternion1.y = -quaternion.y * num;
            quaternion1.z = -quaternion.z * num;
            quaternion1.w = quaternion.w * num;
            return quaternion1;
        }

        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float)(quaternion.x * (double)quaternion.x + quaternion.y * (double)quaternion.y +
                quaternion.z * (double)quaternion.z + quaternion.w * (double)quaternion.w);
            result.x = -quaternion.x * num;
            result.y = -quaternion.y * num;
            result.z = -quaternion.z * num;
            result.w = quaternion.w * num;
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float num1 = angle * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            Quaternion quaternion;
            quaternion.x = axis.x * num2;
            quaternion.y = axis.y * num2;
            quaternion.z = axis.z * num2;
            quaternion.w = num3;
            return quaternion;
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float num1 = angle * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            result.x = axis.x * num2;
            result.y = axis.y * num2;
            result.z = axis.z * num2;
            result.w = num3;
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float num1 = roll * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin(num4);
            float num6 = (float)Math.Cos(num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin(num7);
            float num9 = (float)Math.Cos(num7);
            Quaternion quaternion;
            quaternion.x = (float)(num9 * (double)num5 * num3 + num8 * (double)num6 * num2);
            quaternion.y = (float)(num8 * (double)num6 * num3 - num9 * (double)num5 * num2);
            quaternion.z = (float)(num9 * (double)num6 * num2 - num8 * (double)num5 * num3);
            quaternion.w = (float)(num9 * (double)num6 * num3 + num8 * (double)num5 * num2);
            return quaternion;
        }

        public static Quaternion Euler(Vector3 eulerAngle)
        {
            //角度转弧度
            eulerAngle = Mathf.Deg2Rad(eulerAngle);

            float cX = (float)Math.Cos(eulerAngle.x / 2.0f);
            float sX = (float)Math.Sin(eulerAngle.x / 2.0f);

            float cY = (float)Math.Cos(eulerAngle.y / 2.0f);
            float sY = (float)Math.Sin(eulerAngle.y / 2.0f);

            float cZ = (float)Math.Cos(eulerAngle.z / 2.0f);
            float sZ = (float)Math.Sin(eulerAngle.z / 2.0f);

            var qX = new Quaternion(sX, 0, 0, cX);
            var qY = new Quaternion(0, sY, 0, cY);
            var qZ = new Quaternion(0, 0, sZ, cZ);

            var q = qY * qX * qZ;

            return q;
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            return Euler(new Vector3(x, y, z));
        }

        public static Quaternion FromToRotation(Vector3 a, Vector3 b)
        {
            Vector3 start = a.normalized;
            Vector3 dest = b.normalized;
            float cosTheta = Vector3.Dot(start, dest);
            Vector3 rotationAxis;
            Quaternion quaternion;
            if (cosTheta < -1 + 0.001f)
            {
                rotationAxis = Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), start);
                if (rotationAxis.sqrMagnitude < 0.01f)
                {
                    rotationAxis = Vector3.Cross(new Vector3(1.0f, 0.0f, 0.0f), start);
                }
                rotationAxis.Normalize();
                quaternion = new Quaternion((float)Math.PI, rotationAxis);
                quaternion.Normalize();
                return quaternion;
            }

            rotationAxis = Vector3.Cross(start, dest);
            float s = (float)Math.Sqrt((1 + cosTheta) * 2);
            float invs = 1 / s;

            quaternion = new Quaternion(rotationAxis.x * invs, rotationAxis.y * invs, rotationAxis.z * invs, s * 0.5f);
            quaternion.Normalize();
            return quaternion;
        }

        public static Quaternion LookRotation(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var matrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
            return matrix.GetRotation();
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
        {
            forward = Vector3.Normalize(forward);
            var right = Vector3.Normalize(Vector3.Cross(upwards, forward));
            upwards = Vector3.Cross(forward, right);

            var m00 = right.x;
            var m01 = upwards.x;
            var m02 = forward.x;
            var m10 = right.y;
            var m11 = upwards.y;
            var m12 = forward.y;
            var m20 = right.z;
            var m21 = upwards.z;
            var m22 = forward.z;

            var trace = m00 + m11 + m22;
            var q = new Quaternion();

            if (trace > 0f)
            {
                var num = Mathf.Sqrt(1f + trace);
                q.w = num * 0.5f;
                num = 0.5f / num;
                q.x = (m21 - m12) * num;
                q.y = (m02 - m20) * num;
                q.z = (m10 - m01) * num;
                return q;
            }

            if (m00 >= m11 && m00 >= m22)
            {
                float num = Mathf.Sqrt(1f + m00 - m11 - m22);
                float num4 = 0.5f / num;
                q.x = 0.5f * num;
                q.y = (m01 + m10) * num4;
                q.z = (m02 + m20) * num4;
                q.w = (m12 - m21) * num4;
                return q;
            }

            if (m11 > m22)
            {
                float num6 = Mathf.Sqrt(1f + m11 - m00 - m22);
                float num3 = 0.5f / num6;
                q.x = (m10 + m01) * num3;
                q.y = 0.5f * num6;
                q.z = (m21 + m12) * num3;
                q.w = (m02 - m20) * num3;
                return q;
            }

            var num5 = Mathf.Sqrt(1f + m22 - m00 - m11);
            var num2 = 0.5f / num5;
            q.x = (m20 + m02) * num2;
            q.y = (m21 + m12) * num2;
            q.z = 0.5f * num5;
            q.w = (m01 - m10) * num2;
            return q;
        }

        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float num1 = roll * 0.5f;
            float num2 = (float)Math.Sin(num1);
            float num3 = (float)Math.Cos(num1);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin(num4);
            float num6 = (float)Math.Cos(num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin(num7);
            float num9 = (float)Math.Cos(num7);
            result.x = (float)(num9 * (double)num5 * num3 + num8 * (double)num6 * num2);
            result.y = (float)(num8 * (double)num6 * num3 - num9 * (double)num5 * num2);
            result.z = (float)(num9 * (double)num6 * num2 - num8 * (double)num5 * num3);
            result.w = (float)(num9 * (double)num6 * num3 + num8 * (double)num5 * num2);
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            Quaternion quaternion = new Quaternion();
            if (num1 > 0.0)
            {
                float num2 = (float)Math.Sqrt(num1 + 1.0);
                quaternion.w = num2 * 0.5f;
                float num3 = 0.5f / num2;
                quaternion.x = (matrix.m21 - matrix.m12) * num3;
                quaternion.y = (matrix.m02 - matrix.m20) * num3;
                quaternion.z = (matrix.m10 - matrix.m01) * num3;
                return quaternion;
            }

            if (matrix.m00 >= (double)matrix.m11 && matrix.m00 >= (double)matrix.m22)
            {
                float num2 = (float)Math.Sqrt(1.0 + matrix.m00 - matrix.m11 - matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.x = 0.5f * num2;
                quaternion.y = (matrix.m10 + matrix.m01) * num3;
                quaternion.z = (matrix.m20 + matrix.m02) * num3;
                quaternion.w = (matrix.m21 - matrix.m12) * num3;
                return quaternion;
            }

            if (matrix.m11 > (double)matrix.m22)
            {
                float num2 = (float)Math.Sqrt(1.0 + matrix.m11 - matrix.m00 - matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.x = (matrix.m01 + matrix.m10) * num3;
                quaternion.y = 0.5f * num2;
                quaternion.z = (matrix.m12 + matrix.m21) * num3;
                quaternion.w = (matrix.m02 - matrix.m20) * num3;
                return quaternion;
            }

            float num4 = (float)Math.Sqrt(1.0 + matrix.m22 - matrix.m00 - matrix.m11);
            float num5 = 0.5f / num4;
            quaternion.x = (matrix.m02 + matrix.m20) * num5;
            quaternion.y = (matrix.m12 + matrix.m21) * num5;
            quaternion.z = 0.5f * num4;
            quaternion.w = (matrix.m10 - matrix.m01) * num5;
            return quaternion;
        }

        public static void CreateFromRotationMatrix(ref Matrix4x4 matrix, out Quaternion result)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            if (num1 > 0.0)
            {
                float num2 = (float)Math.Sqrt(num1 + 1.0);
                result.w = num2 * 0.5f;
                float num3 = 0.5f / num2;
                result.x = (matrix.m21 - matrix.m12) * num3;
                result.y = (matrix.m02 - matrix.m20) * num3;
                result.z = (matrix.m10 - matrix.m01) * num3;
            }
            else if (matrix.m00 >= (double)matrix.m11 && matrix.m00 >= (double)matrix.m22)
            {
                float num2 = (float)Math.Sqrt(1.0 + matrix.m00 - matrix.m11 - matrix.m22);
                float num3 = 0.5f / num2;
                result.x = 0.5f * num2;
                result.y = (matrix.m10 + matrix.m01) * num3;
                result.z = (matrix.m20 + matrix.m02) * num3;
                result.w = (matrix.m21 - matrix.m12) * num3;
            }
            else if (matrix.m11 > (double)matrix.m22)
            {
                float num2 = (float)Math.Sqrt(1.0 + matrix.m11 - matrix.m00 - matrix.m22);
                float num3 = 0.5f / num2;
                result.x = (matrix.m01 + matrix.m10) * num3;
                result.y = 0.5f * num2;
                result.z = (matrix.m12 + matrix.m21) * num3;
                result.w = (matrix.m02 - matrix.m20) * num3;
            }
            else
            {
                float num2 = (float)Math.Sqrt(1.0 + matrix.m22 - matrix.m00 - matrix.m11);
                float num3 = 0.5f / num2;
                result.x = (matrix.m02 + matrix.m20) * num3;
                result.y = (matrix.m12 + matrix.m21) * num3;
                result.z = 0.5f * num2;
                result.w = (matrix.m10 - matrix.m01) * num3;
            }
        }

        private Vector3 MatrixToEuler(Matrix4x4 m)
        {
            Vector3 v = new Vector3();
            if (m[1, 2] < 1)
            {
                if (m[1, 2] > -1)
                {
                    v.x = Mathf.Asin(-m[1, 2]);
                    v.y = Mathf.Atan2(m[0, 2], m[2, 2]);
                    v.z = Mathf.Atan2(m[1, 0], m[1, 1]);
                }
                else
                {
                    v.x = Mathf.PI * 0.5f;
                    v.y = Mathf.Atan2(m[0, 1], m[0, 0]);
                    v.z = 0;
                }
            }
            else
            {
                v.x = -Mathf.PI * 0.5f;
                v.y = Mathf.Atan2(-m[0, 1], m[0, 0]);
                v.z = 0;
            }

            for (int i = 0; i < 3; i++)
            {
                if (v[i] < 0)
                {
                    v[i] += 2 * Mathf.PI;
                }
                else if (v[i] > 2 * Mathf.PI)
                {
                    v[i] -= 2 * Mathf.PI;
                }
            }

            return v;
        }

        public static Matrix4x4 QuaternionToMatrix(Quaternion quat)
        {
            Matrix4x4 m = new Matrix4x4();

            float x = quat.x * 2;
            float y = quat.y * 2;
            float z = quat.z * 2;
            float xx = quat.x * x;
            float yy = quat.y * y;
            float zz = quat.z * z;
            float xy = quat.x * y;
            float xz = quat.x * z;
            float yz = quat.y * z;
            float wx = quat.w * x;
            float wy = quat.w * y;
            float wz = quat.w * z;

            m[0] = 1.0f - (yy + zz);
            m[1] = xy + wz;
            m[2] = xz - wy;
            m[3] = 0.0F;

            m[4] = xy - wz;
            m[5] = 1.0f - (xx + zz);
            m[6] = yz + wx;
            m[7] = 0.0F;

            m[8] = xz + wy;
            m[9] = yz - wx;
            m[10] = 1.0f - (xx + yy);
            m[11] = 0.0F;

            m[12] = 0.0F;
            m[13] = 0.0F;
            m[14] = 0.0F;
            m[15] = 1.0F;

            return m;
        }

        private static Quaternion MatrixToQuaternion(Matrix4x4 m)
        {
            Quaternion quat = new Quaternion();

            float fTrace = m[0, 0] + m[1, 1] + m[2, 2];
            float root;

            if (fTrace > 0)
            {
                root = Mathf.Sqrt(fTrace + 1);
                quat.w = 0.5f * root;
                root = 0.5f / root;
                quat.x = (m[2, 1] - m[1, 2]) * root;
                quat.y = (m[0, 2] - m[2, 0]) * root;
                quat.z = (m[1, 0] - m[0, 1]) * root;
            }
            else
            {
                int[] s_iNext = new int[] { 1, 2, 0 };
                int i = 0;
                if (m[1, 1] > m[0, 0])
                {
                    i = 1;
                }
                if (m[2, 2] > m[i, i])
                {
                    i = 2;
                }
                int j = s_iNext[i];
                int k = s_iNext[j];

                root = Mathf.Sqrt(m[i, i] - m[j, j] - m[k, k] + 1);
                if (root < 0)
                {
                    throw new IndexOutOfRangeException("error!");
                }
                quat[i] = 0.5f * root;
                root = 0.5f / root;
                quat.w = (m[k, j] - m[j, k]) * root;
                quat[j] = (m[j, i] + m[i, j]) * root;
                quat[k] = (m[k, i] + m[i, k]) * root;
            }
            float nor = Mathf.Sqrt(Dot(quat, quat));
            quat = new Quaternion(quat.x / nor, quat.y / nor, quat.z / nor, quat.w / nor);

            return quat;
        }

        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            result = (float)(quaternion1.x * (double)quaternion2.x + quaternion1.y * (double)quaternion2.y +
                quaternion1.z * (double)quaternion2.z + quaternion1.w * (double)quaternion2.w);
        }

        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = (float)(quaternion1.x * (double)quaternion2.x + quaternion1.y * (double)quaternion2.y +
                quaternion1.z * (double)quaternion2.z + quaternion1.w * (double)quaternion2.w);
            bool flag = false;
            if (num2 < 0.0)
            {
                flag = true;
                num2 = -num2;
            }

            float num3;
            float num4;
            if (num2 > 0.999998986721039)
            {
                num3 = 1f - num1;
                num4 = flag ? -num1 : num1;
            }
            else
            {
                float num5 = (float)Math.Acos(num2);
                float num6 = (float)(1.0 / Math.Sin(num5));
                num3 = (float)Math.Sin((1.0 - num1) * num5) * num6;
                num4 = flag ? (float)-Math.Sin(num1 * (double)num5) * num6 : (float)Math.Sin(num1 * (double)num5) * num6;
            }

            Quaternion quaternion;
            quaternion.x = (float)(num3 * (double)quaternion1.x + num4 * (double)quaternion2.x);
            quaternion.y = (float)(num3 * (double)quaternion1.y + num4 * (double)quaternion2.y);
            quaternion.z = (float)(num3 * (double)quaternion1.z + num4 * (double)quaternion2.z);
            quaternion.w = (float)(num3 * (double)quaternion1.w + num4 * (double)quaternion2.w);
            return quaternion;
        }

        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = (float)(quaternion1.x * (double)quaternion2.x + quaternion1.y * (double)quaternion2.y +
                quaternion1.z * (double)quaternion2.z + quaternion1.w * (double)quaternion2.w);
            bool flag = false;
            if (num2 < 0.0)
            {
                flag = true;
                num2 = -num2;
            }

            float num3;
            float num4;
            if (num2 > 0.999998986721039)
            {
                num3 = 1f - num1;
                num4 = flag ? -num1 : num1;
            }
            else
            {
                float num5 = (float)Math.Acos(num2);
                float num6 = (float)(1.0 / Math.Sin(num5));
                num3 = (float)Math.Sin((1.0 - num1) * num5) * num6;
                num4 = flag ? (float)-Math.Sin(num1 * (double)num5) * num6 : (float)Math.Sin(num1 * (double)num5) * num6;
            }

            result.x = (float)(num3 * (double)quaternion1.x + num4 * (double)quaternion2.x);
            result.y = (float)(num3 * (double)quaternion1.y + num4 * (double)quaternion2.y);
            result.z = (float)(num3 * (double)quaternion1.z + num4 * (double)quaternion2.z);
            result.w = (float)(num3 * (double)quaternion1.w + num4 * (double)quaternion2.w);
        }

        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            Quaternion quaternion = new Quaternion();
            if (quaternion1.x * (double)quaternion2.x + quaternion1.y * (double)quaternion2.y +
                quaternion1.z * (double)quaternion2.z + quaternion1.w * (double)quaternion2.w >= 0.0)
            {
                quaternion.x = (float)(num2 * (double)quaternion1.x + num1 * (double)quaternion2.x);
                quaternion.y = (float)(num2 * (double)quaternion1.y + num1 * (double)quaternion2.y);
                quaternion.z = (float)(num2 * (double)quaternion1.z + num1 * (double)quaternion2.z);
                quaternion.w = (float)(num2 * (double)quaternion1.w + num1 * (double)quaternion2.w);
            }
            else
            {
                quaternion.x = (float)(num2 * (double)quaternion1.x - num1 * (double)quaternion2.x);
                quaternion.y = (float)(num2 * (double)quaternion1.y - num1 * (double)quaternion2.y);
                quaternion.z = (float)(num2 * (double)quaternion1.z - num1 * (double)quaternion2.z);
                quaternion.w = (float)(num2 * (double)quaternion1.w - num1 * (double)quaternion2.w);
            }

            float num3 = 1f / (float)Math.Sqrt(quaternion.x * (double)quaternion.x + quaternion.y * (double)quaternion.y +
                                                quaternion.z * (double)quaternion.z + quaternion.w * (double)quaternion.w);
            quaternion.x *= num3;
            quaternion.y *= num3;
            quaternion.z *= num3;
            quaternion.w *= num3;
            if (float.IsNaN(quaternion.x) | float.IsNaN(quaternion.y) | float.IsNaN(quaternion.z) | float.IsNaN(quaternion.w))
                quaternion = new Quaternion();
            return quaternion;
        }

        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            if (quaternion1.x * (double)quaternion2.x + quaternion1.y * (double)quaternion2.y +
                quaternion1.z * (double)quaternion2.z + quaternion1.w * (double)quaternion2.w >= 0.0)
            {
                result.x = (float)(num2 * (double)quaternion1.x + num1 * (double)quaternion2.x);
                result.y = (float)(num2 * (double)quaternion1.y + num1 * (double)quaternion2.y);
                result.z = (float)(num2 * (double)quaternion1.z + num1 * (double)quaternion2.z);
                result.w = (float)(num2 * (double)quaternion1.w + num1 * (double)quaternion2.w);
            }
            else
            {
                result.x = (float)(num2 * (double)quaternion1.x - num1 * (double)quaternion2.x);
                result.y = (float)(num2 * (double)quaternion1.y - num1 * (double)quaternion2.y);
                result.z = (float)(num2 * (double)quaternion1.z - num1 * (double)quaternion2.z);
                result.w = (float)(num2 * (double)quaternion1.w - num1 * (double)quaternion2.w);
            }

            float num3 = 1f / (float)Math.Sqrt(result.x * (double)result.x + result.y * (double)result.y +
                                                result.z * (double)result.z + result.w * (double)result.w);
            if (float.IsNaN(num3) | float.IsInfinity(num3))
                num3 = 0;
            result.x *= num3;
            result.y *= num3;
            result.z *= num3;
            result.w *= num3;
        }

        public void Conjugate()
        {
            x = -x;
            y = -y;
            z = -z;
        }

        public static Quaternion Conjugate(Quaternion value)
        {
            Quaternion quaternion;
            quaternion.x = -value.x;
            quaternion.y = -value.y;
            quaternion.z = -value.z;
            quaternion.w = value.w;
            return quaternion;
        }

        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
            result.w = value.w;
        }

        private static void Angle(ref Quaternion a, ref Quaternion b, out float result)
        {
            result = (float)(Math.Acos(Math.Min(Math.Abs(Dot(a, b)), 1f)) * 2.0 * 57.2957801818848);
        }

        public static Quaternion Negate(Quaternion quaternion)
        {
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x;
            quaternion1.y = -quaternion.y;
            quaternion1.z = -quaternion.z;
            quaternion1.w = -quaternion.w;
            return quaternion1;
        }

        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.x = -quaternion.x;
            result.y = -quaternion.y;
            result.z = -quaternion.z;
            result.w = -quaternion.w;
        }

        public static Quaternion Sub(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.x = quaternion1.x - quaternion2.x;
            quaternion.y = quaternion1.y - quaternion2.y;
            quaternion.z = quaternion1.z - quaternion2.z;
            quaternion.w = quaternion1.w - quaternion2.w;
            return quaternion;
        }

        public static void Sub(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }

        public static Vector3 Rotate(Quaternion rotation, Vector3 vector3)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num1;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 vector3_1;
            vector3_1.x = (float)((1.0 - (num5 + (double)num6)) * vector3.x +
                (num7 - (double)num12) * vector3.y + (num8 + (double)num11) * vector3.z);
            vector3_1.y = (float)((num7 + (double)num12) * vector3.x +
                (1.0 - (num4 + (double)num6)) * vector3.y + (num9 - (double)num10) * vector3.z);
            vector3_1.z = (float)((num8 - (double)num11) * vector3.x + (num9 + (double)num10) * vector3.y +
                (1.0 - (num4 + (double)num5)) * vector3.z);
            return vector3_1;
        }

        public static void Rotate(ref Quaternion rotation, ref Vector3 vector3, out Vector3 result)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num1;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            result.x = (float)((1.0 - (num5 + (double)num6)) * vector3.x + (num7 - (double)num12) * vector3.y +
                (num8 + (double)num11) * vector3.z);
            result.y = (float)((num7 + (double)num12) * vector3.x + (1.0 - (num4 + (double)num6)) * vector3.y +
                (num9 - (double)num10) * vector3.z);
            result.z = (float)((num8 - (double)num11) * vector3.x + (num9 + (double)num10) * vector3.y +
                (1.0 - (num4 + (double)num5)) * vector3.z);
        }

        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x1 = quaternion1.x;
            float y1 = quaternion1.y;
            float z1 = quaternion1.z;
            float w1 = quaternion1.w;
            float x2 = quaternion2.x;
            float y2 = quaternion2.y;
            float z2 = quaternion2.z;
            float w2 = quaternion2.w;
            float num1 = (float)(y1 * (double)z2 - z1 * (double)y2);
            float num2 = (float)(z1 * (double)x2 - x1 * (double)z2);
            float num3 = (float)(x1 * (double)y2 - y1 * (double)x2);
            float num4 = (float)(x1 * (double)x2 + y1 * (double)y2 + z1 * (double)z2);
            Quaternion quaternion;
            quaternion.x = (float)(x1 * (double)w2 + x2 * (double)w1) + num1;
            quaternion.y = (float)(y1 * (double)w2 + y2 * (double)w1) + num2;
            quaternion.z = (float)(z1 * (double)w2 + z2 * (double)w1) + num3;
            quaternion.w = w1 * w2 - num4;
            return quaternion;
        }

        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x1 = quaternion1.x;
            float y1 = quaternion1.y;
            float z1 = quaternion1.z;
            float w1 = quaternion1.w;
            float x2 = quaternion2.x;
            float y2 = quaternion2.y;
            float z2 = quaternion2.z;
            float w2 = quaternion2.w;
            float num1 = (float)(y1 * (double)z2 - z1 * (double)y2);
            float num2 = (float)(z1 * (double)x2 - x1 * (double)z2);
            float num3 = (float)(x1 * (double)y2 - y1 * (double)x2);
            float num4 = (float)(x1 * (double)x2 + y1 * (double)y2 + z1 * (double)z2);
            result.x = (float)(x1 * (double)w2 + x2 * (double)w1) + num1;
            result.y = (float)(y1 * (double)w2 + y2 * (double)w1) + num2;
            result.z = (float)(z1 * (double)w2 + z2 * (double)w1) + num3;
            result.w = w1 * w2 - num4;
        }

        public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x;
            quaternion1.y = -quaternion.y;
            quaternion1.z = -quaternion.z;
            quaternion1.w = -quaternion.w;
            return quaternion1;
        }


        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.x = quaternion1.x - quaternion2.x;
            quaternion.y = quaternion1.y - quaternion2.y;
            quaternion.z = quaternion1.z - quaternion2.z;
            quaternion.w = quaternion1.w - quaternion2.w;
            return quaternion;
        }
        #endregion


        public static bool operator ==(Quaternion lhs, UnityEngine.Quaternion rhs)
        {
            return IsEqualUsingDot(Dot(lhs, rhs));
        }

        public static bool operator !=(Quaternion lhs, UnityEngine.Quaternion rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(UnityEngine.Quaternion lhs, Quaternion rhs)
        {
            return IsEqualUsingDot(Dot(lhs, rhs));
        }

        public static bool operator !=(UnityEngine.Quaternion lhs, Quaternion rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator UnityEngine.Quaternion(Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static implicit operator Quaternion(UnityEngine.Quaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static implicit operator UnityEngine.Vector4(Quaternion q)
        {
            return new UnityEngine.Vector4(q.x, q.y, q.z, q.w);
        }

        public static implicit operator Quaternion(UnityEngine.Vector4 q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }
}