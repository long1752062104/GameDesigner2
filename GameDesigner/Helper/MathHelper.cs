using System;

namespace Net.Helper
{
    public static class MathHelper
    {
        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref byte self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked((byte)(self + value)),
                    '-' => checked((byte)(self - value)),
                    '*' => checked((byte)(self * value)),
                    '/' => checked((byte)(self / value)),
                    '%' => checked((byte)(self % value)),
                    '^' => checked((byte)(self ^ value)),
                    '&' => checked((byte)(self & value)),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref sbyte self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked((sbyte)(self + value)),
                    '-' => checked((sbyte)(self - value)),
                    '*' => checked((sbyte)(self * value)),
                    '/' => checked((sbyte)(self / value)),
                    '%' => checked((sbyte)(self % value)),
                    '^' => checked((sbyte)(self ^ value)),
                    '&' => checked((sbyte)(self & value)),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref short self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked((short)(self + value)),
                    '-' => checked((short)(self - value)),
                    '*' => checked((short)(self * value)),
                    '/' => checked((short)(self / value)),
                    '%' => checked((short)(self % value)),
                    '^' => checked((short)(self ^ value)),
                    '&' => checked((short)(self & value)),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref ushort self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked((ushort)(self + value)),
                    '-' => checked((ushort)(self - value)),
                    '*' => checked((ushort)(self * value)),
                    '/' => checked((ushort)(self / value)),
                    '%' => checked((ushort)(self % value)),
                    '^' => checked((ushort)(self ^ value)),
                    '&' => checked((ushort)(self & value)),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref int self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    '^' => checked(self ^ value),
                    '&' => checked(self & value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref uint self, uint value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    '^' => checked(self ^ value),
                    '&' => checked(self & value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref float self, float value, char oper)
        {
            try
            {
                checked
                {
                    self = oper switch
                    {
                        '+' => self + value,
                        '-' => self - value,
                        '*' => self * value,
                        '/' => self / value,
                        '%' => self % value,
                        _ => throw new Exception("没有这个操作数"),
                    };
                }
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref long self, int value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    '^' => checked(self ^ value),
                    '&' => checked(self & value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref ulong self, ulong value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    '^' => checked(self ^ value),
                    '&' => checked(self & value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref double self, double value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }

        /// <summary>
        /// 检查算术溢出，如果溢出了，啥都不能做
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="oper">计算方式</param>
        /// <returns></returns>
        public static bool Calc(this ref decimal self, decimal value, char oper)
        {
            try
            {
                self = oper switch
                {
                    '+' => checked(self + value),
                    '-' => checked(self - value),
                    '*' => checked(self * value),
                    '/' => checked(self / value),
                    '%' => checked(self % value),
                    _ => throw new Exception("没有这个操作数"),
                };
                return true;
            }
            catch (Exception ex)
            {
                Event.NDebug.LogError("算术溢出:" + ex);
            }
            return false;
        }
    }
}