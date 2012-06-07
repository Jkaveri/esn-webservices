using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace JK.Core.Utilities
{
    /// <summary>
    /// Cryptographic by RSA
    /// </summary>
    public class CryptoRsa
    {
        /// <summary>
        /// Initialize for Parameters
        /// </summary>
        /// <returns></returns>
        public static CspParameters AssignParameters()
        {
            const int PROV_RSA_FULL = 1;
            const string CONTAINER = "JKCore";
            var parameter = new CspParameters(PROV_RSA_FULL);
            parameter.KeyContainerName = CONTAINER;
            parameter.Flags = CspProviderFlags.UseMachineKeyStore;
            parameter.ProviderName = "Microsoft Strong Cryptographic Provider";
            return parameter;
        }
        /// <summary>
        /// Generate new key
        /// </summary>
        public static void AssignNewKey()
        {
            var param = AssignParameters();
            var rsa = new RSACryptoServiceProvider(1024, param);
            var writer = new StreamWriter("");
            var publicPrivateKeyXml = rsa.ToXmlString(true);
            writer.Write(publicPrivateKeyXml);
            writer.Close();

            writer = new StreamWriter("");
            var publicOnlyKeyXml = rsa.ToXmlString(false);
            writer.Write(publicOnlyKeyXml);
            writer.Close();
        }
        /// <summary>
        /// Encrypt input
        /// </summary>
        /// <param name="input">string input</param>
        /// <returns>output encoded</returns>
        public static string Encrypt(string input)
        {
            var param = AssignParameters();
            var rsa = new RSACryptoServiceProvider(1024, param);

            string publicKeyFile = System.Web.HttpContext.Current.Server.MapPath("/") + "/" +
                                   SiteSettings.GetInstance().GetValue("JPay::PublicKey");


            var reader = new StreamReader(publicKeyFile);
            var pubKey = reader.ReadToEnd();
            rsa.FromXmlString(pubKey);
            reader.Close();

            var endcyptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(input), false);

            return Convert.ToBase64String(endcyptedBytes);
        }
        /// <summary>
        /// Descrypt rsa
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>decoded</returns>
        public static string Decrypt(string input)
        {
            var param = AssignParameters();
            var rsa = new RSACryptoServiceProvider(1024, param);
            var base64Decrypted = Convert.FromBase64String(input);

            var reader = new StreamReader("privatekey.xml");
            var privKey = reader.ReadToEnd();
            rsa.FromXmlString(privKey);
            reader.Close();

            var decryptBytes = rsa.Decrypt(base64Decrypted,false);
            return Encoding.UTF8.GetString(decryptBytes);
        }
    }
}
