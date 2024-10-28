using Renci.SshNet;
using System;
using System.Collections.Generic;
using Renci.SshNet.Sftp;
using CaptioB2it.Ficheros;
using System.IO;
using ExcelDataReader;
using System.Linq;

namespace CaptioB2it.Utilidades
{
    public class SftpUtils
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string Host;
        string Username;
        string Password;
        int Port;
        int Reintentos;
        List<LogErroresDTO> Errores;

        public SftpUtils(string host, string username, string password, string port, int reintentos)
        {
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Port = int.Parse(port);
            this.Reintentos = reintentos;
            this.Errores = new List<LogErroresDTO>();
        }

        public List<LogErroresDTO> ObtenerErrores()
        {
            return (this.Errores);
        }


        public List<string> ObtenerListaFicherosDirectorioSFTP(string directorio, string extension)
        {
            bool error = false;

            List<string> resultat = new List<string>();

            int i = 0;
            // Realiza hasta (this.reintentos) intentos en caso de error a los 10 segundos
            do
            {
                i++;
                error = false;
                try
                {
                    using (var sftp = new SftpClient(Host, Port, Username, Password))
                    {
                        sftp.Connect();
                        foreach (var entry in sftp.ListDirectory(directorio))
                        {
                            if (!(entry.IsDirectory) && (entry.FullName.Substring(entry.FullName.Length - extension.Length, extension.Length).Equals(extension)))
                            {
                                resultat.Add(entry.FullName.Substring(directorio.Length));
                            }
                        }
                        sftp.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("---> PROBLEMA AL ACCEDER AL DIRECTORIO " + directorio + " (" + i.ToString() + ") DEL SERVIDOR SFTP");
                    Log.Error(ex.Message, ex);
                    error = true;
                }
                if (error)
                {
                    // Espera 10 segundos antes de hacer un nuevo intento
                    System.Threading.Thread.Sleep(10000);
                }
            } while ((i < this.Reintentos) && (error));
            if (error)
            {
                this.Errores.Add(new LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "Sftp_Error",
                    DescripcionError = "No se ha podido conectar al sftp",
                });
            }

            return (resultat);

        }


        public void EnviarTextoToFile_x_SFTP(string Texto, string Filename, string FinalDirectoryName)
        {
            bool error = false;
            int i = 0;
            // Realiza hasta 5 intentos en caso de error a los 10 segundos
            do
            {
                i++;
                error = false;
                try
                {
                    using (var sftp = new SftpClient(Host, Port, Username, Password))
                    {
                        sftp.Connect();
                        if (sftp.Exists(FinalDirectoryName))
                        {
                            if (sftp.Exists(FinalDirectoryName + "/" + Filename))
                            {
                                sftp.Delete(FinalDirectoryName + "/" + Filename);
                            }
                            sftp.WriteAllText(FinalDirectoryName + "/" + Filename, Texto, System.Text.Encoding.UTF8);

                        }
                        sftp.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("---> PROBLEMA AL ACCEDER AL DIRECTORIO " + FinalDirectoryName + " (" + i.ToString() + ") DEL SERVIDOR SFTP");
                    Log.Error(ex.Message, ex);
                    error = true;
                }
                if (error)
                {
                    // Espera 10 segundos antes de hacer un nuevo intento
                    System.Threading.Thread.Sleep(10000);
                }
            } while ((i < this.Reintentos) && (error));
            if (error)
            {
                this.Errores.Add(new LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "Sftp_Error",
                    DescripcionError = "No se ha podido conectar al sftp",
                });
            }
            // Log
            Log.Info("FICHERO ENVIADO CORRECTAMENTE(" + i.ToString() + ") AL SERVIDOR SFTP [" + FinalDirectoryName + "/" + Filename + "]");
        }


        public void MoverFicheroGenerado_x_SFTP(string Filename, string InicialDirectoryName, string FinalDirectoryName)
        {
            int i = 0;
            bool error = false;
            // Realiza hasta 5 intentos en caso de error a los 10 segundos
            do
            {
                i++;
                error = false;
                try
                {
                    if (Filename.StartsWith("/"))
                    {
                        Filename = Filename.Substring(1);
                    }
                    using (var sftp = new SftpClient(Host, Port, Username, Password))
                    {
                        sftp.Connect();
                        if (sftp.Exists(InicialDirectoryName + "/" + Filename) && sftp.Exists(FinalDirectoryName))
                        {
                            SftpFile fitxer = sftp.Get(InicialDirectoryName + "/" + Filename);
                            if (sftp.Exists(FinalDirectoryName + "/" + Filename))
                            {
                                sftp.Delete(FinalDirectoryName + "/" + Filename);
                            }
                            fitxer.MoveTo(FinalDirectoryName + "/" + Filename);
                        }
                        sftp.Disconnect();
                    }
                    // Log
                    Log.Info("MOVIDO EL FICHERO " + InicialDirectoryName + "/" + Filename + " A " + FinalDirectoryName + "/" + Filename + " CORRECTAMENTE EN EL SERVIDOR SFTP");
                }
                catch (Exception ex)
                {
                    Log.Error("---> PROBLEMA AL MOVER EL FICHERO " + InicialDirectoryName + "/" + Filename + " A " + FinalDirectoryName + "/" + Filename + " (" + i.ToString() + ") EN EL SERVIDOR SFTP");
                    Log.Error(ex.Message, ex);
                    error = true;
                }
                if (error)
                {
                    // Espera 10 segundos antes de hacer un nuevo intento
                    System.Threading.Thread.Sleep(10000);
                }
            } while ((i < this.Reintentos) && (error));
            if (error)
            {
                this.Errores.Add(new LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "Sftp_Error",
                    DescripcionError = "No se ha podido conectar al sftp",
                });
            }

        }


        public bool GuardarFicheroTexto_x_SFTP(string[] exportFile, string Filename, string DirectoryName)
        {
            int i = 0;
            bool error = false;
            // Realiza hasta 5 intentos en caso de error a los 10 segundos
            do
            {
                i++;
                error = false;
                try
                {
                    using (var sftp = new SftpClient(Host, Port, Username, Password))
                    {
                        sftp.Connect();
                        if (sftp.Exists(DirectoryName))
                        {
                            if (sftp.Exists(DirectoryName + "/" + Filename))
                            {
                                sftp.Delete(DirectoryName + "/" + Filename);
                            }
                            sftp.AppendAllLines(DirectoryName + "/" + Filename, exportFile, System.Text.Encoding.UTF8);
                        }
                        sftp.Disconnect();
                    }
                    // Log
                    Log.Info("GENERADO EL FICHERO " + DirectoryName + "/" + Filename + " CORRECTAMENTE EN EL SERVIDOR SFTP");
                }
                catch (Exception ex)
                {
                    Log.Error("---> PROBLEMA AL GENERAR EL FICHERO " + DirectoryName + "/" + Filename + " (" + i.ToString() + ") EN EL SERVIDOR SFTP");
                    Log.Error(ex.Message, ex);
                    error = true;
                }
                if (error)
                {
                    // Espera 10 segundos antes de hacer un nuevo intento
                    System.Threading.Thread.Sleep(10000);
                }
            } while ((i < this.Reintentos) && (error));
            if (error)
            {
                this.Errores.Add(new LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "Sftp_Error",
                    DescripcionError = "No se ha podido conectar al sftp",
                });
            }
            return (error);
        }



        // Rutina per llegir un fitxer CSV del SFTP
        // System.Text.Encoding.GetEncoding("iso-8859-1")
        // System.Text.Encoding.UTF8
        public string[] Llegir_FitxerImportacioCSV(string fitxer, string separadorCSV, System.Text.Encoding codificacion)
        {
            string[] result;
            string Separador = separadorCSV;

            try
            {
                bool fitxerExixteix = false;
                List<string> linees_fitxer = new List<string>();
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //open file and returns as Stream
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect();
                    if (sftp.Exists(fitxer))
                    {
                        linees_fitxer.AddRange(sftp.ReadAllLines(fitxer, codificacion).ToList());
                        fitxerExixteix = true;
                    }
                    sftp.Disconnect();
                }

                if (fitxerExixteix)
                {
                    result = linees_fitxer.ToArray();

                    // Log
                    Log.Info("LEIDO CORRECTAMENTE EL FICHERO A IMPORTAR [" + fitxer + "]");
                    return (result);
                }
                else
                {
                    // Log
                    Log.Error("FICHERO A IMPORTAR INEXISTENTE [" + fitxer + "]");
                }
                
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);

            }
            return (null);
        }


    }
}
