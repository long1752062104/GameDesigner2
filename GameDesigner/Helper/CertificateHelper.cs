using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Net.Helper
{
    public class CertificateHelper
    {
        public static X509Certificate2 CreateX509Certificate2()
        {
            // 创建一个新的RSA密钥对，用于证书
            using (RSA rsa = RSA.Create())
            {
                var request = new CertificateRequest("CN=gdnet_websocket_certificate", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                // 设置证书的有效期
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                // 创建自签名证书
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
                // 将证书导出到文件（可选）
                var certData = certificate.Export(X509ContentType.Pkcs12, "gdnet_certificate");
                return new X509Certificate2(certData, "gdnet_certificate");
            }
        }
    }
}
