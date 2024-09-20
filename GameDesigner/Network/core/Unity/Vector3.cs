using System;
using UnityEngine;
using UnityEngine.Internal;

namespace Net
{
    [Serializable]
    public struct Vector3 : IEquatable<Vector3>
    {
        #region 源码
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0f;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            Vector3 a = target - current;
            float magnitude = a.magnitude;
            Vector3 result;
            if (magnitude <= maxDistanceDelta || magnitude < 1E-45f)
            {
                result = target;
            }
            else
            {
                result = current + a / magnitude * maxDistanceDelta;
            }
            return result;
        }


        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed)
        {
            float deltaTime = Time.deltaTime;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }


        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime)
        {
            float deltaTime = Time.deltaTime;
            float positiveInfinity = float.PositiveInfinity;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, positiveInfinity, deltaTime);
        }

        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            smoothTime = Mathf.Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float d = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            Vector3 vector = current - target;
            Vector3 vector2 = target;
            float maxLength = maxSpeed * smoothTime;
            vector = ClampMagnitude(vector, maxLength);
            target = current - vector;
            Vector3 vector3 = (currentVelocity + num * vector) * deltaTime;
            currentVelocity = (currentVelocity - num * vector3) * d;
            Vector3 vector4 = target + (vector + vector3) * d;
            if (Dot(vector2 - current, vector4 - vector2) > 0f)
            {
                vector4 = vector2;
                currentVelocity = (vector4 - vector2) / deltaTime;
            }
            return vector4;
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
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
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
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public void Set(float newX, float newY, float newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        public static Vector3 Scale(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Scale(Vector3 scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }

        public override bool Equals(object other)
        {
            return other is Vector3 && Equals((Vector3)other);
        }

        public bool Equals(Vector3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            return -2f * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float num = Magnitude(value);
            Vector3 result;
            if (num > 1E-05f)
            {
                result = value / num;
            }
            else
            {
                result = zero;
            }
            return result;
        }

        public void Normalize()
        {
            float num = Magnitude(this);
            if (num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = zero;
            }
        }

        [Newtonsoft_X.Json.JsonIgnore]
        public Vector3 normalized
        {
            get
            {
                return Normalize(this);
            }
        }

        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            float num = Dot(onNormal, onNormal);
            Vector3 result;
            if (num < Mathf.Epsilon)
            {
                result = zero;
            }
            else
            {
                result = onNormal * Dot(vector, onNormal) / num;
            }
            return result;
        }

        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            return vector - Project(vector, planeNormal);
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            float num = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            float result;
            if (num < 1E-15f)
            {
                result = 0f;
            }
            else
            {
                float f = Mathf.Clamp(Dot(from, to) / num, -1f, 1f);
                result = Mathf.Acos(f) * 57.29578f;
            }
            return result;
        }

        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float num = Angle(from, to);
            float num2 = Mathf.Sign(Dot(axis, Cross(from, to)));
            return num * num2;
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            Vector3 result;
            if (vector.sqrMagnitude > maxLength * maxLength)
            {
                result = vector.normalized * maxLength;
            }
            else
            {
                result = vector;
            }
            return result;
        }

        public static float Magnitude(Vector3 vector)
        {
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public float magnitude
        {
            get
            {
                return Mathf.Sqrt(x * x + y * y + z * z);
            }
        }

        public static float SqrMagnitude(Vector3 vector)
        {
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        public float sqrMagnitude
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
        }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
        }

        public static Vector3 zero
        {
            get
            {
                return zeroVector;
            }
        }

        public static Vector3 one
        {
            get
            {
                return oneVector;
            }
        }

        public static Vector3 forward
        {
            get
            {
                return forwardVector;
            }
        }

        public static Vector3 back
        {
            get
            {
                return backVector;
            }
        }

        public static Vector3 up
        {
            get
            {
                return upVector;
            }
        }

        public static Vector3 down
        {
            get
            {
                return downVector;
            }
        }

        public static Vector3 left
        {
            get
            {
                return leftVector;
            }
        }

        public static Vector3 right
        {
            get
            {
                return rightVector;
            }
        }

        public static Vector3 positiveInfinity
        {
            get
            {
                return positiveInfinityVector;
            }
        }

        public static Vector3 negativeInfinity
        {
            get
            {
                return negativeInfinityVector;
            }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        public static Vector3 operator *(float d, Vector3 a)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) < 9.9999994E-11f;
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", new object[]
            {
                x,
                y,
                z
            });
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", new object[]
            {
                x.ToString(format),
                y.ToString(format),
                z.ToString(format)
            });
        }

        [Obsolete("Use Vector3.forward instead.")]
        public static Vector3 fwd
        {
            get
            {
                return new Vector3(0f, 0f, 1f);
            }
        }

        [Obsolete("Use Vector3.Angle instead. AngleBetween uses radians instead of degrees and was deprecated for this reason")]
        public static float AngleBetween(Vector3 from, Vector3 to)
        {
            return Mathf.Acos(Mathf.Clamp(Dot(from.normalized, to.normalized), -1f, 1f));
        }

        [Obsolete("Use Vector3.ProjectOnPlane instead.")]
        public static Vector3 Exclude(Vector3 excludeThis, Vector3 fromThat)
        {
            return ProjectOnPlane(fromThat, excludeThis);
        }

        public const float kEpsilon = 1E-05f;
        public const float kEpsilonNormalSqrt = 1E-15f;
        public float x;
        public float y;
        public float z;
        private static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 upVector = new Vector3(0f, 1f, 0f);
        private static readonly Vector3 downVector = new Vector3(0f, -1f, 0f);
        private static readonly Vector3 leftVector = new Vector3(-1f, 0f, 0f);
        private static readonly Vector3 rightVector = new Vector3(1f, 0f, 0f);
        private static readonly Vector3 forwardVector = new Vector3(0f, 0f, 1f);
        private static readonly Vector3 backVector = new Vector3(0f, 0f, -1f);
        private static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        #endregion

        #region 网上代码
        private const float k1OverSqrt2 = 0.7071068f;

        public Vector3(float value)
        {
            x = y = z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            x = value.x;
            y = value.y;
            this.z = z;
        }

        public float Length()
        {
            return (float)Math.Sqrt(x * (double)x + y * (double)y + z * (double)z);
        }

        public float LengthSquared()
        {
            return (float)(x * (double)x + y * (double)y + z * (double)z);
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
            result = (float)Math.Sqrt(num4);
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            return (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            result = (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
        }


        public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
        {
            result = (float)(vector1.x * (double)vector2.x + vector1.y * (double)vector2.y +
                vector1.z * (double)vector2.z);
        }

        public static void Normalize(ref Vector3 value, out Vector3 result)
        {
            float num1 = (float)(value.x * (double)value.x + value.y * (double)value.y + value.z * (double)value.z);
            if (num1 < (double)Mathf.Epsilon)
            {
                result = value;
            }
            else
            {
                float num2 = 1f / (float)Math.Sqrt(num1);
                result.x = value.x * num2;
                result.y = value.y * num2;
                result.z = value.z * num2;
            }
        }

        public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
        {
            float num1 = (float)(vector1.y * (double)vector2.z - vector1.z * (double)vector2.y);
            float num2 = (float)(vector1.z * (double)vector2.x - vector1.x * (double)vector2.z);
            float num3 = (float)(vector1.x * (double)vector2.y - vector1.y * (double)vector2.x);
            result.x = num1;
            result.y = num2;
            result.z = num3;
        }

        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
        {
            float num =
                    (float)(vector.x * (double)normal.x + vector.y * (double)normal.y + vector.z * (double)normal.z);
            result.x = vector.x - 2f * num * normal.x;
            result.y = vector.y - 2f * num * normal.y;
            result.z = vector.z - 2f * num * normal.z;
        }

        public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x < (double)value2.x ? value1.x : value2.x;
            result.y = value1.y < (double)value2.y ? value1.y : value2.y;
            result.z = value1.z < (double)value2.z ? value1.z : value2.z;
        }

        public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x > (double)value2.x ? value1.x : value2.x;
            result.y = value1.y > (double)value2.y ? value1.y : value2.y;
            result.z = value1.z > (double)value2.z ? value1.z : value2.z;
        }

        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            float x = value1.x;
            float num1 = x > (double)max.x ? max.x : x;
            float num2 = num1 < (double)min.x ? min.x : num1;
            float y = value1.y;
            float num3 = y > (double)max.y ? max.y : y;
            float num4 = num3 < (double)min.y ? min.y : num3;
            float z = value1.z;
            float num5 = z > (double)max.z ? max.z : z;
            float num6 = num5 < (double)min.z ? min.z : num5;
            Vector3 vector3;
            vector3.x = num2;
            vector3.y = num4;
            vector3.z = num6;
            return vector3;
        }

        public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
        {
            float x = value1.x;
            float num1 = x > (double)max.x ? max.x : x;
            float num2 = num1 < (double)min.x ? min.x : num1;
            float y = value1.y;
            float num3 = y > (double)max.y ? max.y : y;
            float num4 = num3 < (double)min.y ? min.y : num3;
            float z = value1.z;
            float num5 = z > (double)max.z ? max.z : z;
            float num6 = num5 < (double)min.z ? min.z : num5;
            result.x = num2;
            result.y = num4;
            result.z = num6;
        }

        public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
        }

        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            amount = amount > 1.0 ? 1f : (amount < 0.0 ? 0.0f : amount);
            amount = (float)(amount * (double)amount * (3.0 - 2.0 * amount));
            Vector3 vector3;
            vector3.x = value1.x + (value2.x - value1.x) * amount;
            vector3.y = value1.y + (value2.y - value1.y) * amount;
            vector3.z = value1.z + (value2.z - value1.z) * amount;
            return vector3;
        }

        public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            amount = amount > 1.0 ? 1f : (amount < 0.0 ? 0.0f : amount);
            amount = (float)(amount * (double)amount * (3.0 - 2.0 * amount));
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
        }

        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float)(2.0 * num2 - 3.0 * num1 + 1.0);
            float num4 = (float)(-2.0 * num2 + 3.0 * num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            Vector3 vector3;
            vector3.x = (float)(value1.x * (double)num3 + value2.x * (double)num4 + tangent1.x * (double)num5 +
                tangent2.x * (double)num6);
            vector3.y = (float)(value1.y * (double)num3 + value2.y * (double)num4 + tangent1.y * (double)num5 +
                tangent2.y * (double)num6);
            vector3.z = (float)(value1.z * (double)num3 + value2.z * (double)num4 + tangent1.z * (double)num5 +
                tangent2.z * (double)num6);
            return vector3;
        }

        public static void Hermite(
            ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float)(2.0 * num2 - 3.0 * num1 + 1.0);
            float num4 = (float)(-2.0 * num2 + 3.0 * num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            result.x = (float)(value1.x * (double)num3 + value2.x * (double)num4 + tangent1.x * (double)num5 +
                tangent2.x * (double)num6);
            result.y = (float)(value1.y * (double)num3 + value2.y * (double)num4 + tangent1.y * (double)num5 +
                tangent2.y * (double)num6);
            result.z = (float)(value1.z * (double)num3 + value2.z * (double)num4 + tangent1.z * (double)num5 +
                tangent2.z * (double)num6);
        }

        public static Vector3 Negate(Vector3 value)
        {
            Vector3 vector3;
            vector3.x = -value.x;
            vector3.y = -value.y;
            vector3.z = -value.z;
            return vector3;
        }

        public static void Negate(ref Vector3 value, out Vector3 result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
        }

        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x + value2.x;
            vector3.y = value1.y + value2.y;
            vector3.z = value1.z + value2.z;
            return vector3;
        }

        public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
            result.z = value1.z + value2.z;
        }

        public static Vector3 Sub(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x - value2.x;
            vector3.y = value1.y - value2.y;
            vector3.z = value1.z - value2.z;
            return vector3;
        }

        public static void Sub(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
            result.z = value1.z - value2.z;
        }

        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x * value2.x;
            vector3.y = value1.y * value2.y;
            vector3.z = value1.z * value2.z;
            return vector3;
        }

        public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
            result.z = value1.z * value2.z;
        }

        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            Vector3 vector3;
            vector3.x = value1.x * scaleFactor;
            vector3.y = value1.y * scaleFactor;
            vector3.z = value1.z * scaleFactor;
            return vector3;
        }

        public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
        }

        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x / value2.x;
            vector3.y = value1.y / value2.y;
            vector3.z = value1.z / value2.z;
            return vector3;
        }

        public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
            result.z = value1.z / value2.z;
        }

        public static Vector3 Divide(Vector3 value1, float divider)
        {
            float num = 1f / divider;
            Vector3 vector3;
            vector3.x = value1.x * num;
            vector3.y = value1.y * num;
            vector3.z = value1.z * num;
            return vector3;
        }

        public static void Divide(ref Vector3 value1, float divider, out Vector3 result)
        {
            float num = 1f / divider;
            result.x = value1.x * num;
            result.y = value1.y * num;
            result.z = value1.z * num;
        }

        private static float magnitudeStatic(ref Vector3 inV)
        {
            return (float)Math.Sqrt(Dot(inV, inV));
        }

        private static Vector3 orthoNormalVectorFast(ref Vector3 n)
        {
            Vector3 vector3;
            if (Math.Abs(n.z) > (double)k1OverSqrt2)
            {
                float num = 1f / (float)Math.Sqrt(n.y * (double)n.y + n.z * (double)n.z);
                vector3.x = 0.0f;
                vector3.y = -n.z * num;
                vector3.z = n.y * num;
            }
            else
            {
                float num = 1f / (float)Math.Sqrt(n.x * (double)n.x + n.y * (double)n.y);
                vector3.x = -n.y * num;
                vector3.y = n.x * num;
                vector3.z = 0.0f;
            }

            return vector3;
        }

        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent)
        {
            float num1 = magnitudeStatic(ref normal);
            if (num1 > (double)Mathf.Epsilon)
                normal /= num1;
            else
                normal = new Vector3(1f, 0.0f, 0.0f);
            float num2 = Dot(normal, tangent);
            tangent -= num2 * normal;
            float num3 = magnitudeStatic(ref tangent);
            if (num3 < (double)Mathf.Epsilon)
                tangent = orthoNormalVectorFast(ref normal);
            else
                tangent /= num3;
        }

        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal)
        {
            float num1 = magnitudeStatic(ref normal);
            if (num1 > (double)Mathf.Epsilon)
                normal /= num1;
            else
                normal = new Vector3(1f, 0.0f, 0.0f);
            float num2 = Dot(normal, tangent);
            tangent -= num2 * normal;
            float num3 = magnitudeStatic(ref tangent);
            if (num3 > (double)Mathf.Epsilon)
                tangent /= num3;
            else
                tangent = orthoNormalVectorFast(ref normal);
            float num4 = Dot(tangent, binormal);
            float num5 = Dot(normal, binormal);
            binormal -= num5 * normal + num4 * tangent;
            float num6 = magnitudeStatic(ref binormal);
            if (num6 > (double)Mathf.Epsilon)
                binormal /= num6;
            else
                binormal = Cross(normal, tangent);
        }

        public static void Project(ref Vector3 vector, ref Vector3 onNormal, out Vector3 result)
        {
            result = onNormal * Dot(vector, onNormal) / Dot(onNormal, onNormal);
        }

        public static void Angle(ref Vector3 from, ref Vector3 to, out float result)
        {
            from.Normalize();
            to.Normalize();
            Dot(ref from, ref to, out float result1);
            result = Mathf.Cos(Mathf.Clamp(result1, -1f, 1f)) * 57.29578f;
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x * value2.x;
            vector3.y = value1.y * value2.y;
            vector3.z = value1.z * value2.z;
            return vector3;
        }

        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x / value2.x;
            vector3.y = value1.y / value2.y;
            vector3.z = value1.z / value2.z;
            return vector3;
        }
        #endregion

        public static UnityEngine.Vector3 operator +(UnityEngine.Vector3 a, Vector3 b)
        {
            return new UnityEngine.Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static UnityEngine.Vector3 operator -(UnityEngine.Vector3 a, Vector3 b)
        {
            return new UnityEngine.Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static UnityEngine.Vector3 operator *(UnityEngine.Vector3 a, Vector3 b)
        {
            return new UnityEngine.Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static UnityEngine.Vector3 operator /(UnityEngine.Vector3 a, Vector3 b)
        {
            return new UnityEngine.Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 operator +(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 operator /(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static bool operator ==(UnityEngine.Vector3 lhs, Vector3 rhs)
        {
            return (lhs - rhs).sqrMagnitude < 9.9999994E-11f;
        }

        public static bool operator !=(UnityEngine.Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}