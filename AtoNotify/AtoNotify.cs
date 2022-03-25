using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AtoNotify
{
    internal class AtoNotify
    {
        // Ovaj komad koda prima dva argumenta na ulaz: vremenski žig i tekst poruke
        // Od te dvije informacije sastavlja se poruka koja se šalje klijentu preko notify@ato.hr računa
        // Imati na umu da aplikacija nije definirana kao konzolaška - to jest nema niti jednog vidljivog prozora
        // To se može promijeniti u property sheetu aplikacije - odabrati "Console application"
        // Coding by kjurlina. Have a lot of fun.
        static void Main(string[] args)
        {
            // Deklaracija varijabli
            AtoNotifySupervisor Supervisor = null;
            string date = string.Empty;
            string time = string.Empty;
            string message = string.Empty;
            int i;

            // Instanca supervizora
            try
            {
                Supervisor = new AtoNotifySupervisor();
                AppDomain.CurrentDomain.ProcessExit += new EventHandler((sender, e) => CurrentDomain_ProcessExit(sender, e, Supervisor));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            // Kreiranje datoteke za logiranje
            try
            {
                if (!Supervisor.CheckLogFileExistence())
                {
                    // Create log file
                    Supervisor.CreateLogFile();
                }
                else if (Supervisor.ChecklLogFileSize() > 1048576)
                {
                    // If log file is too big archive it and create new one
                    Supervisor.ArchiveLogFile();
                    Supervisor.CreateLogFile();
                }
                else
                {
                    // Zasad ne zapisujemo kada se aplikacija pokrece i zaustavlja
                    // Supervisor.ToLogFile("Application started with existing log file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Something went wrong with log file. Exiting application");
                return;
            }

            // Procitaj ulazne argumente. Mora biti minimalno 3 argumenta: datum, vrijeme i tekst alarma
            try
            {
                date = args[0];
                time = args[1];
                for (i = 2; i < args.Length; i++)
                {
                    message = message + args[i] + " ";
                }
            }
            catch (Exception ex)
            {
                Supervisor.ToLogFile("Aplikacija nije pozvana s ispravnim parametrima (string datum + string vrijeme + string tekst odvojeni razmakom...)");
                Supervisor.ToLogFile(ex.Message);
            }

            // Ukoliko su ulazni parametri OK, kreiraj SMTP klijenta i posalji poruku
            try
            {
                // Definiraj SMPT klijenta
                SmtpClient AtoSMTP = new SmtpClient("smtp.office365.com");

                // Definiraj kredencijali
                AtoSMTP.UseDefaultCredentials = false;
                NetworkCredential basicAuthenticationInfo = new NetworkCredential("notifier@ato.hr", "Xax75901");
                AtoSMTP.Credentials = basicAuthenticationInfo;
                AtoSMTP.EnableSsl = true;

                // Kreiraj poruku
                MailAddress from = new MailAddress("notifier@ato.hr", "ATO Notifikator");
                MailAddress to = new MailAddress("alarm.odvodnja@dj-vodovod.hr ", "Dežurna služba održavanja ADJ CNUS");
                MailMessage msg = new MailMessage(from, to);

                msg.Subject = "ATO Notifikator :: CNUS šalje poruku :: " + message + " :: " + date + " " + time;
                msg.SubjectEncoding = msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Body = "Tekst alarma :: " + message + " <br />" + "Vremenski žig alarma :: " + date + " " + time;

                // Pošalji poruku
                AtoSMTP.Send(msg);

                // Napravi zapis
                Supervisor.ToLogFile("Poslana poruka :: " + message);
            }

            // Handlanje grešaka - samo ispiši u konzolu i izađi van
            catch (SmtpException ex)
            {
                Supervisor.ToLogFile("SMPT exception :: " + ex.Message);
            }
            catch (Exception ex)
            {
                Supervisor.ToLogFile("General exception :: " + ex.Message);
            }
        }

        // Izlazna strategija...
        static void CurrentDomain_ProcessExit(object sender, EventArgs e, AtoNotifySupervisor supervisor)
        {
            // Zasad ne zapisujemo kada se aplikacija pokrece i zaustavlja
            // Samo greske i uspjesno poslane mailove (gore u kodu)
            // supervisor.ToLogFile("Application closed");
            // supervisor.ToLogFile("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        }
    }
}
