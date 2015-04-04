using System;
using System.Security.Cryptography.X509Certificates;

namespace Telligent.Evolution.Extensions.Lync.Utils
{
    public class CertificateUtil
    {
        public static X509Certificate2 GetCertificate(StoreName name, StoreLocation location, string thumbprint)
        {
            var store = new X509Store(name, location);

            X509Certificate2Collection certificates = null;
            store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2 result = null;
                certificates = store.Certificates;

                foreach (var cert in certificates)
                {
                    var certThumbprint = cert.Thumbprint ?? string.Empty;
                    if (certThumbprint.Equals(thumbprint, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (result != null)
                        {
                            throw new ApplicationException(string.Format("Multiple certificates found: {0}", thumbprint));
                        }

                        result = new X509Certificate2(cert);
                    }
                }
                if (result == null)
                {
                    throw new ApplicationException(string.Format("No certificate was found: {0}", thumbprint));
                }

                return result;
            }
            finally
            {
                if (certificates != null)
                {
                    foreach (var cert in certificates)
                    {
                        cert.Reset();
                    }
                }

                store.Close();
            }
        }
    }
}
