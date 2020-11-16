using Amqp;
using System;
using Amqp.Sasl;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

namespace Sender
{
    class Sender
    {
       static void Main(string[] args)
        {
            Task<int> task = SslConnectionTestAsync();
            task.Wait();
        }
        static async Task<int> SslConnectionTestAsync()
        {
            try{
                ConnectionFactory factory = new ConnectionFactory();

                String certFile = "C:\\location\\certificate.cer";
                factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
                factory.SSL.LocalCertificateSelectionCallback = (a, b, c, d, e) => X509Certificate.CreateFromCertFile(certFile);
                factory.SSL.ClientCertificates.Add(X509Certificate.CreateFromCertFile(certFile));

                factory.SASL.Profile = SaslProfile.External;
                Connection.DisableServerCertValidation = false; 

                Address address = new Address("amqps://username:password@host:5671");
                Connection connection = await factory.CreateAsync(address);
                Session session = new Session(connection);
                SenderLink sender = new SenderLink(session, "sender-link", "amqp");
                Message message = null;

                DateTime d1 = DateTime.Now;   
                Console.WriteLine(d1); 
                Console.WriteLine("Send Start time : {0}!",d1); 

                for (int i = 0 ; i <5 ; i++){
                    string msg = "num --"+i +"-->this is a testing message for sender, to test sending proformance";
                    message = new Message(msg);
                    sender.SendAsync(message);
                    Console.WriteLine("Sent messaging {0}!", msg);
                }
               
                DateTime d2 = DateTime.Now;
                Console.WriteLine("Send End time : {0}!",d2); 
                Console.WriteLine("Press enter key to exit...");
                Console.ReadLine();

                await sender.CloseAsync();
                await session.CloseAsync();
                await connection.CloseAsync();
                return 0;
            }
         catch (Exception e)
            {
                Console.WriteLine("Exception {0}.", e);
                return 1;
            }
        }
       public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            return false;
        }
    }
}
