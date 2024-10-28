using CaptioB2it.Ficheros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CaptioB2it.Utilidades
{
    class SmtpUtils
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int tiempo_espera_reintento = 30000;

        string Mail_User_From;
        string Mail_User_FromName;
        string Mail_Server_Smtp;
        int Mail_Port_Smtp;
        string Mail_User_Credentials;
        string Mail_Password_Credentials;
        int Reintentos;
        List<LogErroresDTO> Errores;


        public SmtpUtils(string mail_User_From, string mail_User_FromName, string mail_Server_Smtp, string mail_Port_Smtp, string mail_User_Credentials, string mail_Password_Credentials, int reintentos)
        {
            this.Mail_User_From = mail_User_From;
            this.Mail_User_FromName = mail_User_FromName;
            this.Mail_Server_Smtp = mail_Server_Smtp;
            this.Mail_Port_Smtp = int.Parse(mail_Port_Smtp);
            this.Mail_User_Credentials = mail_User_Credentials;
            this.Mail_Password_Credentials = mail_Password_Credentials;
            this.Reintentos = reintentos;
            this.Errores = new List<LogErroresDTO>();
        }


        public List<LogErroresDTO> ObtenerErrores()
        {
            return (this.Errores);
        }


        private Boolean Email_bien_escrito(String email)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                if (Regex.Replace(email, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Rutina que envia un mail adjuntant els fitxers passats com a paràmetre.
        // Retorna true si hi ha error i false si tot OK
        public bool SendEmail(string Mail_To, string Mail_CC, string Subject, string Body, List<string> filesAttached = null)
        {
            int intento = 0;
            bool error = true;
            while ((intento < this.Reintentos) && (error))
            {
                try
                {
                    error = false;
                    // Preparamos el mail
                    System.Net.Mail.MailMessage email = new MailMessage()
                    {
                        From = new MailAddress(Mail_User_From, Mail_User_FromName)
                    };
                    foreach (var address in Mail_To.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (Email_bien_escrito(address))
                        {
                            email.To.Add(new MailAddress(address));
                        }
                        else
                        {
                            this.Errores.Add(new LogErroresDTO
                            {
                                FechaHora = DateTime.Now,
                                TipoError = "Smtp_Error",
                                DescripcionError = "Dirección incorrecta : " + address,
                            });
                        }
                    }
                    foreach (var address in Mail_CC.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (Email_bien_escrito(address))
                        {
                            email.CC.Add(new MailAddress(address));
                        }
                        else
                        {
                            this.Errores.Add(new LogErroresDTO
                            {
                                FechaHora = DateTime.Now,
                                TipoError = "Smtp_Error",
                                DescripcionError = "Dirección incorrecta : " + address,
                            });
                        }
                    }
                    if ((email.To.Count == 0) && (email.CC.Count == 0))
                    {
                        error = true;
                        return (error);
                    }

                    email.Subject = Subject;
                    //email.Body = string.Concat("Fichero : ", this.GetFileNameWithoutExtension(), ".xlsx \r\n");
                    //string body = email.Body;
                    //DateTime now = DateTime.Now;
                    email.Body = Body;
                    email.IsBodyHtml = false;
                    if (filesAttached != null)
                    {
                        //adjuntamos archivos
                        foreach (string archivo in filesAttached)
                        {
                            //comprobamos si existe el archivo y lo agregamos a los adjuntos
                            if (File.Exists(@archivo))
                                email.Attachments.Add(new Attachment(@archivo));
                        }
                    }

                    // HOST, PORT
                    SmtpClient smtpEmail = new SmtpClient(Mail_Server_Smtp, Mail_Port_Smtp);
                    // User, Password
                    System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(Mail_User_Credentials, Mail_Password_Credentials);
                    smtpEmail.EnableSsl = true;
                    smtpEmail.UseDefaultCredentials = false;
                    smtpEmail.Credentials = credentials;
                    smtpEmail.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Envio del mail
                    smtpEmail.Send(email);
                    smtpEmail.Dispose();
                }
                catch (Exception ex)
                {
                    // Log
                    Log.Error("---> PROBLEMA AL ENVIAR POR MAIL EL FICHERO GENERADO");
                    Log.Error(ex.Message, ex);
                    intento++;
                    // Espera 30 segundos antes de hacer un nuevo intento
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);
                    error = true;
                }
            }
            if (error)
            {
                this.Errores.Add(new LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "Smtp_Error",
                    DescripcionError = "No se ha podido enviar el mail por smtp",
                });
            }
            return (error);
        }

    }
}


