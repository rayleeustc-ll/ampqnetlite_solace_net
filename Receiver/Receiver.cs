using Amqp;
using System;
using Amqp.Sasl;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;


namespace Receiver
{
     
    class Receiver
    {
       static void Main(string[] args)
        {
            Task<int> task = SslReConnectionTestAsync();
            task.Wait();
        }
        static async Task<int> SslReConnectionTestAsync()
        {
            try{
                ConnectionFactory factory = new ConnectionFactory();

                String certFile = "C:\\location\\certification.cer";
                factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
                factory.SSL.LocalCertificateSelectionCallback = (a, b, c, d, e) => X509Certificate.CreateFromCertFile(certFile);
                factory.SSL.ClientCertificates.Add(X509Certificate.CreateFromCertFile(certFile));

                factory.SASL.Profile = SaslProfile.External;
                Connection.DisableServerCertValidation = false; 

                Address address = new Address("amqps://username:password@host:5671");
                Connection connection = await factory.CreateAsync(address);
                Session session = new Session(connection);
                ReceiverLink receiver = new ReceiverLink(session, "receiver-link", "amqp");
                Message message = receiver.Receive();;

                while (message != null)
                {
                    Console.WriteLine("Received " + message.Body);
                    receiver.Accept(message);
                    message = receiver.Receive();
                }

                await receiver.CloseAsync();
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
