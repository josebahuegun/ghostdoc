using System;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace SUPERMERKATUA
{
    /// <summary>
    /// EMAILA KUDEATZEKO KLASEA
    /// </summary>
    public class KudeatuEmail
    {
        /// <summary>
        /// Bidalis the specified adjunto path.
        /// </summary>
        /// <param name="adjuntoPath">The adjunto path.</param>
        public static void Bidali(string adjuntoPath)
        {
            Console.WriteLine("GMAIL BIDALTZEN...");

            try
            {
                string nireEmail = "djhuegun@gmail.com";
                string pasahitza = "n d s p c n l i r t j o v l j t"; //APLIKAZIO PASAHITZA

                string noraBidali = "djhuegun@gmail.com";

                using (MailMessage correua = new MailMessage())
                {
                    correua.From = new MailAddress(nireEmail);
                    correua.To.Add(noraBidali);
                    correua.Subject = "Supermerkatua - Backup XML - " + DateTime.Now;
                    correua.Body = "Hemen duzu XML fitxategia atxikita.";

                    if (!string.IsNullOrEmpty(adjuntoPath) && File.Exists(adjuntoPath))
                    {
                        correua.Attachments.Add(new Attachment(adjuntoPath));
                    }

                    using (SmtpClient server = new SmtpClient())
                    {
                        server.Host = "smtp.gmail.com";
                        server.Port = 587;
                        server.EnableSsl = true;
                        server.DeliveryMethod = SmtpDeliveryMethod.Network;
                        server.UseDefaultCredentials = false;
                        server.Credentials = new NetworkCredential(nireEmail, pasahitza);

                        server.Send(correua);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✔ EMAIL BIDALITA ONDO!");
                Console.ResetColor();
            }
            catch (SmtpException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SMTP ERROREA:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}
