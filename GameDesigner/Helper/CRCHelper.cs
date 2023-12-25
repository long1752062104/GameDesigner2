﻿namespace Net.Helper
{
    public static class CRCHelper
    {
        /// <summary>
        /// CRC校验代码表, 用户可自行改变CRC校验码, 直接改源代码, 客户端和服务器检验码必须一致, 否则识别失败
        /// </summary>
        public static readonly byte[] CRCCode = new byte[]
        {
            0x2d, 0x9e, 0x2e, 0xbe, 0x29, 0x5e, 0x0e, 0x64, 0x30, 0xcb, 0xe5, 0xce, 0x0c, 0x4e,
            0xe8, 0x4d, 0x87, 0xf0, 0x14, 0xcd, 0x24, 0x3a, 0x4a, 0xe7, 0x73, 0x75, 0x3d, 0x85,
            0xa7, 0xde, 0x95, 0x23, 0x25, 0x07, 0x11, 0x1d, 0x82, 0x28, 0x33, 0x2c, 0xeb, 0xa5,
            0x31, 0xf3, 0x91, 0xf6, 0x5c, 0x69, 0xf5, 0xa3, 0x32, 0x26, 0xd7, 0x84, 0x3e, 0x49,
            0x77, 0xbb, 0x3b, 0xfc, 0x9b, 0xfd, 0xc0, 0xb0, 0x08, 0xb4, 0x62, 0xe4, 0x8e, 0xa6,
            0xb9, 0x18, 0xef, 0xc6, 0x46, 0xe0, 0x90, 0x20, 0x27, 0x1b, 0x72, 0xc7, 0xf2, 0xdb,
            0x71, 0x03, 0x7e, 0x00, 0x35, 0x53, 0x4c, 0xe2, 0x63, 0x55, 0x61, 0x4b, 0x9a, 0x93,
            0x02, 0xab, 0xd9, 0x3c, 0xbd, 0xf9, 0x47, 0x42, 0x09, 0xad, 0x70, 0x1a, 0xc5, 0x2a,
            0xb8, 0x34, 0xd0, 0x81, 0xe9, 0xae, 0x60, 0x10, 0x4f, 0x74, 0xb7, 0x76, 0xe3, 0xfb,
            0xe6, 0xc9, 0x6b, 0xdf, 0x3f, 0x12, 0xa8, 0xec, 0xcf, 0x05, 0x1c, 0xc8, 0x98, 0x51,
            0x21, 0x5d, 0x41, 0x45, 0x94, 0xd1, 0xe1, 0x52, 0x67, 0xea, 0x8b, 0xd5, 0x0d, 0x01,
            0x97, 0x83, 0xbf, 0x17, 0xbc, 0x40, 0xb1, 0x89, 0x79, 0x7a, 0x16, 0xfe, 0xff, 0x54,
            0x80, 0x5b, 0x43, 0x13, 0xf1, 0xfa, 0x5f, 0x57, 0x50, 0xee, 0x44, 0x92, 0xca, 0x15,
            0x9f, 0xf7, 0x56, 0x65, 0x9c, 0xdd, 0x5a, 0xc2, 0x86, 0xd3, 0xf8, 0x06, 0xa0, 0x58,
            0xa1, 0x6a, 0x39, 0x59, 0xd2, 0xf4, 0x0f, 0x6c, 0x6f, 0x1f, 0xd8, 0x68, 0x19, 0xb2,
            0x0a, 0x48, 0x6d, 0xa4, 0x8d, 0xa2, 0x37, 0x66, 0x04, 0x22, 0x0b, 0x9d, 0xb6, 0x78,
            0x36, 0x7d, 0xb3, 0xdc, 0x96, 0x8a, 0xda, 0x7c, 0xba, 0x8c, 0x8f, 0xac, 0x2f, 0x6e,
            0x7f, 0xcc, 0x38, 0x2b, 0x99, 0xaf, 0xc3, 0xd6, 0xc1, 0xd4, 0xc4, 0xaa, 0x7b, 0x88,
            0xed, 0x1e, 0xb5, 0xa9,
        };

        public static byte CRC8(byte[] buffer)
        {
            return CRC8(buffer, 0, buffer.Length);
        }

        public static byte CRC8(byte[] buffer, int off, int len)
        {
            byte crc = 0;
            for (int i = off; i < len; i++)
            {
                crc ^= CRCCode[crc ^ buffer[i]];
            }
            return crc;
        }

        public unsafe static byte CRC8(byte* buffer, int off, int len)
        {
            byte crc = 0;
            for (int i = off; i < len; i++)
            {
                var value = buffer[i];
                crc ^= CRCCode[crc ^ value];
            }
            return crc;
        }

        public unsafe static byte CRC8(byte* buffer, int off, int len, byte mask)
        {
            byte crc = 0;
            for (int i = off; i < len; i++)
            {
                var value = buffer[i];
                crc = (byte)(crc ^ CRCCode[crc ^ value] ^ mask);
            }
            return crc;
        }
    }
}
