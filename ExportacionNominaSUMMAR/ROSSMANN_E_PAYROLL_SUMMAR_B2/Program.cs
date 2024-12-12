using System;
using System.Collections.Generic;
using System.Globalization;
using CaptioB2it.Entidades;
using CaptioB2it.Utilidades;
using CaptioB2it.Ficheros;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using GestionAppConfig = CaptioB2it.Utilidades.GestionAppConfig;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;


namespace E_ACCOUNT_B2
{
    public class Program
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Declaración variables
        CultureInfo spanishCulture;
        AccesoCaptio_v3 captio_v3;
        UtilsCaptio utils_captio;
        //SftpUtils utils_sftp;
        //SmtpUtils utils_smtp;
        GestionLogError utils_logs;
        AppSettings appSettings = new AppSettings();
        UtilesGenericas utils_camps;
        UtilsFicheros utils_fitxers = new UtilsFicheros();
        GestionAppConfig GestionAppConfig = new GestionAppConfig();

        DateTime fechaGeneracion = DateTime.Now;
        DateTime fechaGeneracionUTC;
        DateTime fechaInicioExport;
        DateTime fechaFinExport;
        string fechaInicioExport_txt;
        string fechaFinExport_txt;
        List<LogErroresDTO> logErrores = new List<LogErroresDTO>();

        //int info_error = 0;  // 1-OK. 2-WARNING. 3-NOK. 4-ErrorCaptio
        bool info_error_Warning = false;
        bool info_error_Error = false;

        // Main
        public static void Main(string[] args)
        {

            Program program = new Program();

            // Configuració del AppSettings
            program.appSettings = program.GestionAppConfig.ReadAppSettings();

            // Comprovar que existeixen els directoris
            if (!Directory.Exists(Path.GetDirectoryName(program.appSettings.LogEjecucion)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(program.appSettings.LogEjecucion));
            }
            if (!Path.Exists(program.appSettings.DirectorioFicherosEntrada))
            {
                Directory.CreateDirectory(program.appSettings.DirectorioFicherosEntrada);
            }
            if (!Path.Exists(program.appSettings.DirectorioFicherosSalida))
            {
                Directory.CreateDirectory(program.appSettings.DirectorioFicherosSalida);
            }

            // Configuració fitxer de logs
            if ((program.appSettings.LogEjecucion.Contains("\\")) && (!(Directory.Exists(program.appSettings.LogEjecucion.Substring(0, program.appSettings.LogEjecucion.LastIndexOf("\\"))))))
            {
                program.appSettings.LogEjecucion = program.appSettings.LogEjecucion.Remove(0, program.appSettings.LogEjecucion.LastIndexOf("\\") + 1);
            }
            if ((program.appSettings.LogEjecucion.Contains(".txt")) && (program.appSettings.LogEjecucion.Substring(program.appSettings.LogEjecucion.Length - 4, 4).Equals(".txt")))
            {
                program.appSettings.LogEjecucion = program.appSettings.LogEjecucion.Insert(program.appSettings.LogEjecucion.LastIndexOf(".txt"), "_" + program.fechaGeneracion.ToString("yyyyMMdd-HHmmss"));
            }
            else
            {
                program.appSettings.LogEjecucion = program.appSettings.LogEjecucion + "_" + program.fechaGeneracion.ToString("yyyyMMdd-HHmmss") + ".txt";
            }
            log4net.GlobalContext.Properties["LogName"] = program.appSettings.LogEjecucion;
            XmlConfigurator.Configure(LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()), new FileInfo("log4net.config"));

            program.spanishCulture = new CultureInfo("es-ES");

            // Datos de acceso a las API's v3
            string UrlToken_v3 = "https://login.captio.net/identity/connect/token";          //URL para recoger el token *PRO* Api v3 de Captio
            string CaptioUrl_v3 = "https://api-integrations.captio.net";                      //URL de la API *PRO* de Captio
            string GrantType_v3 = "client_credentials";
            string Scope_v3 = "integrations_api";
            string ClientId_v3 = "B2IT.Captio";
            string ClientSecret_v3 = "a079ff4c-0e6b-4ba5-8d78-a3f88aed3c07";                 //Credenciales de acceso a *PRO*
            string CaptioCustomerKey_v3 = program.appSettings.CustomerKey;
            int CaptioPageSize_v3 = 500;
            int CaptioReintentos = 5;
            bool CaptioLogApi = false;

            switch (program.appSettings.EntornoCaptio.ToUpper())
            {
                case "PRE":
                    UrlToken_v3 = "https://identity-pre.captio.net/identity/connect/token";      //URL para recoger el token *PRE* Api v3 de Captio
                    CaptioUrl_v3 = "https://api-integrations-pre.captio.net";                    //URL de la API *PRE* de Captio
                    ClientSecret_v3 = "fcb74cea-94c6-4757-bffc-e27668809b99";             //Credenciales de acceso a *PRE*
                    CaptioCustomerKey_v3 = program.appSettings.CustomerKey;
                    CaptioLogApi = true;
                    break;
                case "API":
                    CaptioLogApi = true;
                    break;
                default:
                    break;
            }

            // BORRAR XAVIER
            //CaptioLogApi = true;

            bool error = false;
            try
            {
                // Obtener acceso a la API
                program.captio_v3 = new AccesoCaptio_v3(UrlToken_v3, GrantType_v3, Scope_v3, ClientId_v3, ClientSecret_v3, CaptioUrl_v3, CaptioCustomerKey_v3, CaptioPageSize_v3, CaptioLogApi, CaptioReintentos);

                // Obtener acceso a SFTP (servidor SFTP)
                //program.utils_sftp = new SftpUtils(program.appSettings.SftpServidor, program.appSettings.SftpUsuario, program.appSettings.SftpPassword, program.appSettings.SftpPuerto, CaptioReintentos);

                // Obtener acceso a las utilidades de Captio
                program.utils_captio = new UtilsCaptio(program.captio_v3);

                // Obtener acceso a SMTP para los Logs (envio de mails con los logs)
                //program.utils_smtp_logs = new SmtpUtils(program.appSettings.LogMailUserCredentials, program.appSettings.LogMailUserFromName, program.appSettings.LogMailServerSmtp, program.appSettings.LogMailPortSmtp, program.appSettings.LogMailUserCredentials, program.appSettings.LogMailPasswordCredentials, CaptioReintentos);

                // Obtener acceso a la clase de utilidades
                program.utils_camps = new UtilesGenericas();

                // Obtener acceso a la clase de utilidades de ficheros
                //program.utils_fitxers = new UtilsFicheros();

                // Iniciar Logs
                program.utils_logs = new GestionLogError(CaptioCustomerKey_v3);

                //// Obtener acceso a la clase de utilidades de ficheros
                //program.utils_fitxers = new UtilsFicheros();

                // Log
                Log.Info("[" + program.fechaGeneracion.ToString() + "]");
                Log.Info("***********************************************************************************");
                Log.Info("***********************************************************************************");
                Log.Info("*********** INICIANDO EL PROCESO DE EXPORTACION DE GASTOS ***********");
                Log.Info("******** Inicio del proceso  : " + program.fechaGeneracion.ToString() + " ********");
                Log.Info("***********************************************************************************");


                List<string> reportsProcessats = new List<string>();
                // Exportació del fitxer de comptabilitat
                error = program.MainIntegration_Contabilidad(CaptioCustomerKey_v3, ref reportsProcessats);


                if (!error)
                {
                    // Guardamos la FechaUltimaExportacion para la siguiente exportación
                    program.GestionAppConfig.UpdateAppSettings("FechaUltimaEjecucion", program.fechaGeneracion.ToString(program.spanishCulture));
                }

                if (!error)
                {
                    // Log
                    Log.Info("***********************************************************************************************");
                    Log.Info("******** Final del proceso : " + DateTime.Now.ToString() + "********");
                    Log.Info("***** FINALIZADO CORRECTAMENTE EL PROCESO DE EXPORTACION DE CONTABILIDAD *****");
                    Log.Info("***********************************************************************************************");
                    Log.Info("************************************************************************************************\r\n\n");
                }
                else
                {
                    // Log
                    Log.Info("********************************************************************************************");
                    Log.Error("******** Final del proceso : " + DateTime.Now.ToString() + " ********");
                    Log.Error("***** FINALIZADO CON ERRORES EL PROCESO DE EXPORTACION DE GASTOS *****");
                    Log.Info("********************************************************************************************");
                    Log.Info("********************************************************************************************\r\n\n");
                }
            }
            catch (Exception ex)
            {
                // 
                // Log
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Info("********************************************************************************************");
                Log.Error("******** Final del proceso : " + DateTime.Now.ToString() + "********");
                Log.Error("***** FINALIZADO CON ERROR EL PROCESO DE EXPORTACION DE GASTOS *****");
                Log.Info("********************************************************************************************");
                Log.Info("********************************************************************************************\r\n\n");
                // Log Mail
                program.utils_logs.AddLog(DateTime.Now, "System_Error", "Captio Access Error", new GestionLogError.InfoLogDTO
                {
                    Type = "System Error",
                    Description = "Captio Access Error",
                    Data = "",
                    Id = "",
                    FileName = "",
                    Line = "",
                    AdminLogin = "",
                    ExtraInfo = "",
                });
                program.info_error_Error = true;
            }

            ////
            //// Gestió LOGS
            //string subject = "";
            //string fileName = "";
            ////if (program.info_error != 4)
            //if (!program.info_error_Error)
            //{
            //    //if ((program.utils_captio.ObtenerErrores().Count() > 0) || (program.utils_sftp.ObtenerErrores().Count() > 0))
            //    if (program.utils_captio.ObtenerErrores().Count() > 0)
            //    {
            //        program.info_error_Error = true;
            //    }
            //}
            //bool error_aux = false;
            //if (program.info_error_Error)
            //{
            //    // GASTOS CAPTIO Error
            //    program.utils_logs.AfegirErrorsToLog(program.utils_captio.ObtenerErrores());
            //    //program.utils_logs.AfegirErrorsToLog(program.utils_sftp.ObtenerErrores());
            //    // GASTOS CAPTIO ERROR CAPTIO
            //    subject = "NOK " + CaptioCustomerKey_v3 + " GASTOS CAPTIO";
            //    fileName = Path.Combine(Path.GetDirectoryName(program.appSettings.LogEjecucion), "Log_ERROR_" + program.fechaGeneracion.ToString("yyyyMMddTHHmmss") + ".txt");
            //    // enviar logs per correu
            //    if (program.utils_logs.GetLogs("ERROR").Count() > 0)
            //    {
            //        error_aux = program.utils_captio.EnviarLog_x_Mail(program.utils_logs.GetLogs("ERROR"), fileName, program.appSettings.LogMailTo_ErrorCAPTIO.Trim() + ";" + program.appSettings.LogMailCC_ErrorCAPTIO.Trim(), subject, "es", "0", false, 2);
            //    }
            //}
            //else
            //{
            //    if (program.info_error_Warning)
            //    {
            //        // GASTOS CAPTIO Warning
            //        subject = "WARNING " + CaptioCustomerKey_v3 + " GASTOS CAPTIO";
            //        fileName = Path.Combine(Path.GetDirectoryName(program.appSettings.LogEjecucion), "Log_Warning_" + program.fechaGeneracion.ToString("yyyyMMddTHHmmss") + ".txt");
            //        // enviar logs per correu
            //        error_aux = program.utils_captio.EnviarLog_x_Mail(program.utils_logs.GetLogs("Warning"), fileName, program.appSettings.LogMailTo, subject, "es", "1", false, 1);
            //    }

            //    // GASTOS CAPTIO OK
            //    List<GestionLogError.LogDTO> logs = new List<GestionLogError.LogDTO>();
            //    List<GestionLogError.LogDTO> logs_reports = new List<GestionLogError.LogDTO>();
            //    logs_reports.AddRange(program.utils_logs.GetLogs("OK"));
            //    if (logs_reports.Count() > 0)
            //    {
            //        // Reports
            //        logs.Add(new GestionLogError.LogDTO
            //        {
            //            FechaHora = DateTime.Now,
            //            TipoError = "OK",
            //            DescripcionError = "Importe Total",
            //            InfoLog = new GestionLogError.InfoLogDTO
            //            {
            //                Type = "Importe Total",
            //                Description = program.utils_camps.CalcularSumaDecimalStrings(logs_reports.FindAll(x => x.DescripcionError.Equals("Report")).Select(v => v.InfoLog.Description).ToList()),
            //                Data = logs_reports.FindAll(x => x.DescripcionError.Equals("Report")).Select(v => v.InfoLog.Description).Count().ToString() + " informes",
            //                Id = "-",
            //                FileName = program.appSettings.NombreFicheroExportacion + program.fechaGeneracion.ToString("yyyyMMddHHmmss"),
            //                Line = "-",
            //                AdminLogin = "",
            //                ExtraInfo = ""
            //            }
            //        });
            //        logs.AddRange(logs_reports.FindAll(x => x.DescripcionError.Equals("Report")));
            //    }
            //    subject = "OK " + CaptioCustomerKey_v3 + " GASTOS CAPTIO";
            //    fileName = Path.Combine(Path.GetDirectoryName(program.appSettings.LogEjecucion), "Log_OK_" + program.fechaGeneracion.ToString("yyyyMMddTHHmmss") + ".txt");
            //    // enviar logs per correu
            //    error_aux = program.utils_captio.EnviarLog_x_Mail(logs, fileName, program.appSettings.LogMailTo, subject, "es", "2", true, 1);
            //}

        }



        //
        // Contabilidad
        // **********************************
        // Main de Exportación Contabilidad
        // **********************************
        bool MainIntegration_Contabilidad(string CaptioCustomerKey_v3, ref List<string> reportsProcessats)
        {
            bool error = false;

            // Log
            Log.Info("Inicio del proceso de Generación del fichero Contabilidad de " + CaptioCustomerKey_v3 + " : " + DateTime.Now.ToString());

            // Calculamos la fecha de inicio y final de los informes a seleccionar
            CalcularFechasInicioFin();

            // Log
            Log.Info("Fecha incicio de los Reports a procesar aprobados : " + fechaInicioExport.ToString() + "  [" + fechaInicioExport_txt + "]");
            Log.Info("Fecha final de los Reports a procesar aprobados : " + fechaFinExport.ToString() + "  [" + fechaFinExport_txt + "]");

            //// Otindre els Custom Fields de Captio
            //List<CustomFieldsDTO_v3_1> customFieldsCaptio = new List<CustomFieldsDTO_v3_1>();
            //customFieldsCaptio.AddRange(captio_v3.GetCustomFields_v3_1(""));

            // Otindre les Categories de Captio
            List<CompaniesCategoriesDTO_v3_1> categoriesCaptio = new List<CompaniesCategoriesDTO_v3_1>();
            categoriesCaptio.AddRange(captio_v3.GetCompaniesCategories_v3_1(""));

            // Otindre els usuaris de Captio
            List<UsersDTO_v3_1> usersCaptio = new List<UsersDTO_v3_1>();
            usersCaptio.AddRange(captio_v3.GetUsers_v3_1(""));

            //// Obtener el id del nombre campo "NombreCampoPersonalizado_Estacion"
            //string id_CF_Estacion = "";
            //if (customFieldsCaptio.Exists(x => x.Type.Equals("1") && x.ExternalName.Equals(appSettings.NombreCampoPersonalizado_Estacion)))
            //{
            //    id_CF_Estacion = customFieldsCaptio.Find(x => x.Type.Equals("1") && x.ExternalName.Equals(appSettings.NombreCampoPersonalizado_Estacion)).Id;
            //}
            //else
            //{
            //    // Log
            //    Log.Error("FALTA CAMPO PERSONALIZADO " + appSettings.NombreCampoPersonalizado_Estacion);
            //    // Log Mail
            //    utils_logs.AddLog(DateTime.Now, "ERROR", "Mandatory Fields Empty", new GestionLogError.InfoLogDTO
            //    {
            //        Type = "Configuration Error",
            //        Description = "Mandatory Fields Empty",
            //        Data = appSettings.NombreCampoPersonalizado_Estacion + " Empty.  This custom fields are mandatory",
            //        Id = "",
            //        FileName = "",
            //        Line = "",
            //        AdminLogin = "",
            //        ExtraInfo = "",
            //    });
            //    error = true;
            //    return (error);
            //}

            // Lectura y proceso de los informes
            List<FicheroExportacionDTO> exportDATA = new List<FicheroExportacionDTO>
            {
                AddFirstLine()
            };
            exportDATA.AddRange(ProcessReports(fechaInicioExport_txt, fechaFinExport_txt, ref usersCaptio, categoriesCaptio, ref reportsProcessats));

            string nombreFichero = "";
            //System.Text.Encoding codificacion = System.Text.Encoding.GetEncoding("iso-8859-1");
            // System.Text.Encoding.GetEncoding(1252)
            System.Text.Encoding codificacion = System.Text.Encoding.UTF8;

            // Generamos fichero de contabilidad
            if (exportDATA.Count > 0)
            {
                try
                {
                    // Guardem Fitxer generat
                    nombreFichero = appSettings.NombreFicheroExportacion_Contabilidad + fechaInicioExport.ToString("yyyyMMddTHHmmss") + "-" + fechaFinExport.ToString("yyyyMMddTHHmmss") + ".csv";

                    //// 
                    //File.WriteAllLines(Path.Combine(appSettings.DirectorioFicherosGenerados, nombreFichero), utils_camps.ConvertirEstructuraContableDTO_To_String(exportDATA, appSettings.SeparadorCSV, UtilesGenericas.CalcularNumeroColumnes(exportDATA)), codificacion);
                    //utils_fitxers.ConvertCSV_to_XLSX(Path.GetFileNameWithoutExtension(nombreFichero), Path.GetExtension(nombreFichero), appSettings.DirectorioFicherosGenerados);

                    // Generem el fitxer sense la última línea
                    string[] aaa = utils_camps.ConvertirEstructuraContableDTO_To_String(exportDATA, appSettings.SeparadorCSV, UtilesGenericas.CalcularNumeroColumnes(exportDATA));
                    string bbb = "";
                    for (int i = 0; i < aaa.Length;  i++)
                    {
                        // "\r\n"
                        bbb = bbb + aaa[i];
                        if ((i + 1) < aaa.Length)
                        {
                            bbb = bbb + "\r\n";
                        }
                    }
                    File.WriteAllText(Path.Combine(appSettings.DirectorioFicherosSalida, nombreFichero), bbb, codificacion);
                    utils_fitxers.ConvertCSV_to_XLSX(Path.GetFileNameWithoutExtension(nombreFichero), Path.GetExtension(nombreFichero), appSettings.DirectorioFicherosSalida, true);
                    

                    // Log
                    Log.Info("GENERADO FICHERO DE Contabilidad [" + Path.Combine(appSettings.DirectorioFicherosSalida, nombreFichero) + "]");
                    if (exportDATA.Count == 1)
                    {
                        Log.Info("FICHERO SIN DATOS");
                    }
                }
                catch (Exception ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    error = true;
                }
            }
            else
            {
                Log.Info("Fichero de Contabilidad NO Generado.  Fichero sin datos.");
            }


            return (error);
        }


        //
        // Contabilidad
        // Rutina per realitzar l'exportació de dades
        //private List<ExportacionContabilidadDTO> ProcessReports(string FechaInicio, string FechaFin, List<UsersDTO_v3_1> users, List<WorkflowsDTO_v3_1> workflows, string CF_ComentarioJustificacion)
        private List<FicheroExportacionDTO> ProcessReports(string FechaInicio, string FechaFin, ref List<UsersDTO_v3_1> usersCaptio, List<CompaniesCategoriesDTO_v3_1> categoriesCaptio, ref List<string> reportsProcessatsTotals)
        {
            try
            {
                ReportsDTO_v3_1[] reports;

                string startDate = FechaInicio;  // Data inicial
                string endDate = FechaFin;    // Data final
                string reportStatus = "4";  // informes aprobados

                List<ExpensesDTO_v3_1> gastos_del_informe = new List<ExpensesDTO_v3_1>();
                List<ExpensesDTO_v3_1> todos_los_gastos = new List<ExpensesDTO_v3_1>();
                List<string> reportsProcessats = new List<string>();

                UsersDTO_v3_1 user;

                List<FicheroExportacionDTO> exportData = new List<FicheroExportacionDTO>();

                // Comptador de número de report
                int num_report = 0;

                // Comptador del número de línea
                int num_asiento = 0;

                // Llegir tots els Reports entre dos dates
                reports = captio_v3.GetReports_v3_1(reportStatus, startDate, endDate);

                //// BORRAR XAVIER
                //List<ReportsDTO_v3_1> reports_AUX_XAVIER = new List<ReportsDTO_v3_1>();
                //reports_AUX_XAVIER.AddRange(captio_v3.GetReports_v3_1("\"" + "Id" + "\"" + ":" + "1161237"));
                //reports_AUX_XAVIER.AddRange(captio_v3.GetReports_v3_1("\"" + "Id" + "\"" + ":" + "1228487"));
                //reports_AUX_XAVIER.AddRange(captio_v3.GetReports_v3_1("\"" + "Id" + "\"" + ":" + "1229738"));
                //reports = reports_AUX_XAVIER.ToArray();
                //// FI BORRAR XAVIER

                if ((reports != null) && (reports.Length > 0))
                {
                    // Procesamos cada usuario
                    foreach (string id_user in reports.Select(x => x.User.Id).Distinct())
                    {
                        todos_los_gastos.Clear();
                        reportsProcessats.Clear();

                        // Otindre l'usuari
                        user = utils_captio.GetUsuario(id_user, ref usersCaptio);

                        // Log
                        Log.Info("PROCESANDO REPORTS DEL USUARIO : [" + user.Login.ToString() + "] - [" + user.Name + "]");

                        // Procesamos cada report
                        foreach (ReportsDTO_v3_1 informe in Array.FindAll(reports, r => r.User.Id.Equals(id_user)))
                        {
                            // Processar cada report
                            num_report++;

                            // Log
                            Log.Info("--> PROCESANDO REPORT : [" + num_report.ToString() + "/" + reports.Count() + "] : " + informe.Name + "  [" + informe.Id + "]" + "  [" + informe.ExternalId.Trim() + "]");


                            // Obtener los tickets de un report
                            gastos_del_informe.Clear();
                            gastos_del_informe.AddRange(captio_v3.GetExpenses_v3_1("\"" + "Report_id" + "\"" + ":" + informe.Id));

                            // Seleccionar los gastos de KM, de DIETAS y GASTOS DE VIAJE
                            foreach (ExpensesDTO_v3_1 expense in gastos_del_informe)
                            {
                                if ((expense.IsMileage.ToLower().Equals("true")) || (utils_captio.GetCategoryCode_Expense(expense, categoriesCaptio).Trim().Contains("DIETA")) || (utils_captio.GetCategoryCode_Expense(expense, categoriesCaptio).Trim().Contains("GASTOS DE VIAJE")))
                                {
                                    //// Excluir los gastos de DIETA y MEDIA DIETA que sean menores del valor máximo del ticket configurado en Captio.
                                    //if (!EsTicketDieta_Inferior_al_Limite_Rossmann(expense, categoriesCaptio))
                                    //{
                                    //    // Afegir el ticket a processar
                                    //    todos_los_gastos.Add(expense);
                                    //}
                                    todos_los_gastos.Add(expense);
                                }

                            }

                            // Afegir id a reports processats
                            reportsProcessats.Add(new string(informe.Id));

                        }

                        // Generar las líneas dels fitxer de cada usuario
                        bool usuario_ok = FicheroExportacion_Gastos(ref exportData, user, todos_los_gastos, ref num_asiento, categoriesCaptio);
                        if (usuario_ok)
                        {
                            // Log
                            Log.Info(" --> Reports del usuario [" + user.Login + "] añadidos a los ficheros de exportación");

                            // Afegir id a reports processats
                            reportsProcessatsTotals.AddRange(reportsProcessats);
                        }
                        else
                        {
                            // Log
                            Log.Error(" --> Reports del usuario [" + user.Login + "] NO añadidos a los ficheros de exportación");

                            //// Log Mail
                            //utils_logs.AddLog(DateTime.Now, "Warning", "Violation Report", new GestionLogError.InfoLogDTO
                            //{
                            //    Type = "Violtation: report no exported",
                            //    Description = "ExternalId Report : " + informe.ExternalId,
                            //    Data = user.Login,
                            //    Id = informe.ExternalId.Trim(),
                            //    FileName = "-",
                            //    Line = "-",
                            //    AdminLogin = "-",
                            //    ExtraInfo = "-",
                            //});
                            info_error_Warning = true;

                            // Afegir report a reportsNoExportats
                            //iDsInformesNoExportados = (String.IsNullOrEmpty(iDsInformesNoExportados)) ? informe.Id : (iDsInformesNoExportados + "|" + informe.Id);
                        }

                    }
                }
                return (exportData);
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);

                return null;
            }
        }


        //
        // Contabilidad
        private FicheroExportacionDTO AddFirstLine()
        {
            FicheroExportacionDTO result = (new FicheroExportacionDTO()
            {
                Campo_1 = "Email Usuario",
                Campo_2 = "Nombre Usuario",
                Campo_3 = "Código Usuario SUMMAR",
                Campo_4 = "Nombre Categoría",
                Campo_5 = "Código de Categoría",
                Campo_6 = "Tasa KM",
                Campo_7 = "Total KM",
                Campo_8 = "Total Dietas",
                Campo_9 = "Amount",
                Campo_10 = "Fecha generación",
                Campo_11 = "Código SUMMAR",

                NumeroColumnes = 11
            });
            return (result);
        }


        //
        // Fichero exportación
        // Retorna true si s'ha inclós el report correctament i false si no s'ha inclós
        private bool FicheroExportacion_Gastos(ref List<FicheroExportacionDTO> exportData, UsersDTO_v3_1 user, List<ExpensesDTO_v3_1> expenses, ref int num_asiento, List<CompaniesCategoriesDTO_v3_1> categoriesCaptio)
        {
            List<FicheroExportacionDTO> result = new List<FicheroExportacionDTO>();
            List<ExpensesDTO_v3_1> expenses_KM = new List<ExpensesDTO_v3_1>();
            List<ExpensesDTO_v3_1> expenses_Dietas = new List<ExpensesDTO_v3_1>();

            int num_linea = 1;

            //bool report_amb_Violations = Informe_amb_Violations_STEGroup(report, expenses, user, ref workflowsCaptio);
            bool report_amb_Violations = false;

            if (!report_amb_Violations)
            {
                decimal suma_total_todos_los_gastos = utils_captio.SumaTotalFinalAmountExpenses(expenses);

                try
                {
                    expenses_KM.Clear();
                    expenses_Dietas.Clear();
                    expenses_KM.AddRange(expenses.FindAll(x => x.IsMileage.ToLower().Equals("true")));
                    expenses_Dietas.AddRange(expenses.FindAll(x => (!x.IsMileage.ToLower().Equals("true"))));

                    foreach (string tasaKM in (expenses_KM.Select(x => utils_captio.GetTasaKM_Expense(x)).Distinct()))
                    {
                        List<ExpensesDTO_v3_1> expensesKM_x_TasaKM = new List<ExpensesDTO_v3_1>();
                        expensesKM_x_TasaKM.AddRange(expenses_KM.FindAll(x => utils_captio.GetTasaKM_Expense(x).Equals(tasaKM)));
                        decimal total_KM = 0;
                        foreach (ExpensesDTO_v3_1 expenseKM in expensesKM_x_TasaKM)
                        {
                            if (expenseKM.MileageInfo != null && (!String.IsNullOrEmpty(expenseKM.MileageInfo.Distance)))
                            {
                                //Console.WriteLine(expenseKM.MileageInfo.Distance + "  ---> " + expenseKM.Id);
                                total_KM = total_KM + decimal.Parse(expenseKM.MileageInfo.Distance, CultureInfo.GetCultureInfo("en-US"));
                            }
                        }
                        decimal total_Amount = utils_captio.SumaTotalFinalAmountExpenses(expensesKM_x_TasaKM);

                        string codigoCategoria = utils_captio.GetCategoryCode_Expense(expensesKM_x_TasaKM.Last(), categoriesCaptio);
                        string codigo_SUMMAR = (codigoCategoria.Contains("#")) ? codigoCategoria.Split("#").Last() : "";

                        result.Add(new FicheroExportacionDTO()
                        {
                            Campo_1 = user.Email,
                            Campo_2 = user.Name,
                            //Campo_3 = (String.IsNullOrEmpty(user.UserOptions.EmployeeCode)) ? "" : user.UserOptions.EmployeeCode,
                            Campo_3 = utils_captio.ObtindreValorCampPersonalitzatUsuari(user, appSettings.NombreCampoPersonalizadoUsuario_CodigoSUMMAR),
                            Campo_4 = "Kilometraje",
                            Campo_5 = "KM",
                            Campo_6 = decimal.Parse(tasaKM, CultureInfo.GetCultureInfo("en-US")).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                            Campo_7 = total_KM.ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                            Campo_8 = "",
                            Campo_9 = total_Amount.ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                            //Campo_10 = fechaGeneracion.ToString("dd/MM/yyyy"),
                            Campo_10 = (Obtener_Fecha_Generacion_SUMMAR(fechaGeneracion)).ToString("dd/MM/yyyy"),
                            Campo_11 = codigo_SUMMAR
                        });
                        num_linea++;
                    }

                    foreach (string categoria in (expenses_Dietas.Select(x => utils_captio.GetCategoryName_Expense(x)).Distinct()))
                    {
                        List<ExpensesDTO_v3_1> expensesDietas_x_Categoria = new List<ExpensesDTO_v3_1>();
                        expensesDietas_x_Categoria.AddRange(expenses_Dietas.FindAll(x => utils_captio.GetCategoryName_Expense(x).Equals(categoria)));

                        string codigoCategoria = utils_captio.GetCategoryCode_Expense(expensesDietas_x_Categoria.Last(), categoriesCaptio);
                        string codigo_DIETA = (codigoCategoria.Contains("#")) ? codigoCategoria.Split("#").First() : "DIETA";
                        string codigo_SUMMAR = (codigoCategoria.Contains("#")) ? codigoCategoria.Split("#").Last() : "";

                        result.Add(new FicheroExportacionDTO()
                        {
                            Campo_1 = user.Email,
                            Campo_2 = user.Name,
                            //Campo_3 = (String.IsNullOrEmpty(user.UserOptions.EmployeeCode)) ? "" : user.UserOptions.EmployeeCode,
                            Campo_3 = utils_captio.ObtindreValorCampPersonalitzatUsuari(user, appSettings.NombreCampoPersonalizadoUsuario_CodigoSUMMAR),
                            Campo_4 = categoria,
                            //Campo_5 = "DIETA",
                            Campo_5 = codigo_DIETA,
                            Campo_6 = "",
                            Campo_7 = "",
                            Campo_8 = expensesDietas_x_Categoria.Count.ToString(),
                            Campo_9 = utils_captio.SumaTotalFinalAmountExpenses(expensesDietas_x_Categoria).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                            //Campo_10 = fechaGeneracion.ToString("dd/MM/yyyy"),
                            Campo_10 = (Obtener_Fecha_Generacion_SUMMAR(fechaGeneracion)).ToString("dd/MM/yyyy"),
                            Campo_11 = codigo_SUMMAR
                        });
                        num_linea++;
                    }
                }
                catch (Exception ex)
                {
                    // Log
                    Log.Error("Reports del usuario [" + user.Login + "] no incluidos en el fichero");
                    Log.Error(ex.Message);
                    return (false);
                }

                // Afegir líneas als fitxers d'exportació
                exportData.AddRange(result);

                //// Log Mail
                //utils_logs.AddLog(DateTime.Now, "OK", "Report", new GestionLogError.InfoLogDTO
                //{
                //    Type = num_asiento.ToString(),
                //    Description = suma_total_todos_los_gastos.ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                //    Data = user.Login,
                //    Id = report.ExternalId.Trim(),
                //    FileName = "",
                //    Line = "-",
                //    AdminLogin = "-",
                //    ExtraInfo = "-",
                //});
            }
            else
            {
                return (false);
            }

            return (true);

        }



        private string ObtenerTaxRate(FicheroExportacionDTO registro, string tipo_iva)
        {
            string result = "";
            try
            {
                decimal tipo_iva_decimal = decimal.Parse(tipo_iva, CultureInfo.GetCultureInfo("es-ES"));
                try
                {
                    decimal tipo_iva_1_registro = decimal.Parse(registro.Campo_50, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_1_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_50.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_2_registro = decimal.Parse(registro.Campo_53, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_2_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_53.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_3_registro = decimal.Parse(registro.Campo_56, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_3_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_56.Trim());
                    }
                }
                catch
                { }
            }
            catch
            { }
            return (result);
        }


        private string ObtenerTaxableBase(FicheroExportacionDTO registro, string tipo_iva)
        {
            string result = "";
            try
            {
                decimal tipo_iva_decimal = decimal.Parse(tipo_iva, CultureInfo.GetCultureInfo("es-ES"));
                try
                {
                    decimal tipo_iva_1_registro = decimal.Parse(registro.Campo_50, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_1_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_51.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_2_registro = decimal.Parse(registro.Campo_53, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_2_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_54.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_3_registro = decimal.Parse(registro.Campo_56, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_3_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_57.Trim());
                    }
                }
                catch
                { }
            }
            catch
            { }
            //try
            //{
            //    decimal tipo_iva_decimal = decimal.Parse(tipo_iva, CultureInfo.GetCultureInfo("es-ES"));
            //    decimal tipo_iva_registro = decimal.Parse(registro.Campo_50, CultureInfo.GetCultureInfo("es-ES"));
            //    if (tipo_iva_registro == tipo_iva_decimal)
            //    {
            //        result = (registro.Campo_51.Trim());
            //    }
            //    else if (registro.Campo_53.Trim().Equals(tipo_iva.Trim()))
            //    {
            //        result = (registro.Campo_53.Trim());
            //    }
            //    else if (registro.Campo_56.Trim().Equals(tipo_iva.Trim()))
            //    {
            //        result = (registro.Campo_57.Trim());
            //    }
            //}
            //catch
            //{
            //}
            return (result);
        }

        private string ObtenerTaxPayable(FicheroExportacionDTO registro, string tipo_iva)
        {
            string result = "";
            try
            {
                decimal tipo_iva_decimal = decimal.Parse(tipo_iva, CultureInfo.GetCultureInfo("es-ES"));
                try
                {
                    decimal tipo_iva_1_registro = decimal.Parse(registro.Campo_50, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_1_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_52.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_2_registro = decimal.Parse(registro.Campo_53, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_2_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_55.Trim());
                    }
                }
                catch
                { }
                try
                {
                    decimal tipo_iva_3_registro = decimal.Parse(registro.Campo_56, CultureInfo.GetCultureInfo("es-ES"));
                    if (tipo_iva_3_registro == tipo_iva_decimal)
                    {
                        result = (registro.Campo_58.Trim());
                    }
                }
                catch
                { }
            }
            catch
            { }
            //try
            //{
            //    decimal tipo_iva_decimal = decimal.Parse(tipo_iva, CultureInfo.GetCultureInfo("es-ES"));
            //    decimal tipo_iva_registro = decimal.Parse(registro.Campo_50, CultureInfo.GetCultureInfo("es-ES"));
            //    if (tipo_iva_registro == tipo_iva_decimal)
            //    {
            //        result = (registro.Campo_52.Trim());
            //    }
            //    else if (registro.Campo_53.Trim().Equals(tipo_iva.Trim()))
            //    {
            //        result = (registro.Campo_55.Trim());
            //    }
            //    else if (registro.Campo_56.Trim().Equals(tipo_iva.Trim()))
            //    {
            //        result = (registro.Campo_58.Trim());
            //    }
            //}
            //catch
            //{
            //}
            return (result);
        }


        private string CalcularCuentaContable(ExpensesDTO_v3_1 expense, UsersDTO_v3_1 user)
        {
            string result = (expense.Category == null) ? "" : (String.IsNullOrEmpty(expense.Category.Code) ? "" : expense.Category.Code).Trim();

            if ((user != null) &&  (!String.IsNullOrEmpty(user.UserOptions.EmployeeCode)) && user.UserOptions.EmployeeCode.Contains("#") && !String.IsNullOrEmpty(user.UserOptions.EmployeeCode.Split("#")[1].Trim()))
            {
                result = (user == null) ? "" : ((String.IsNullOrEmpty(user.UserOptions.EmployeeCode)) ? "" : user.UserOptions.EmployeeCode.Split("#")[1].Trim());
            }
            //List<UsersDTO_v3_1_CustomFields> userCustomFields = new List<UsersDTO_v3_1_CustomFields>();
            //userCustomFields.AddRange(user.CustomFields);

            //if ((userCustomFields.Exists(x => x.Name.Equals(appSettings.NombreCampoPersonalizadoUsuario_Departamento))) && ((userCustomFields.Find(x => x.Name.Equals(appSettings.NombreCampoPersonalizadoUsuario_Departamento))).Name.Trim().Equals(appSettings.NombreRotadoras.Trim())))
            //{
            //    result = appSettings.CuentaContable_Rotadoras;
            //}
            //if (user.Login.ToUpper().Equals(appSettings.LoginUsuario_Cobertura.ToUpper()))
            //{
            //    result = appSettings.CuentaContable_Cobertura;
            //}
            //if (user.Login.ToUpper().Equals(appSettings.LoginUsuario_Formacion.ToUpper()))
            //{
            //    result = appSettings.CuentaContable_Formacion;
            //}

            return (result);
        }


        private void CalcularFechasInicioFin()
        {
            string FechaUltimaExportacion_AppConfig = appSettings.FechaUltimaEjecucion;
            fechaGeneracionUTC = fechaGeneracion.ToUniversalTime();

            // Fecha inicio
            if (FechaUltimaExportacion_AppConfig != "")
            {
                fechaInicioExport = Convert.ToDateTime(FechaUltimaExportacion_AppConfig, spanishCulture).AddMilliseconds(1).ToUniversalTime();
                if (fechaInicioExport.CompareTo(fechaGeneracion) >= 0)
                {
                    fechaInicioExport = fechaGeneracionUTC.AddDays(-1);
                }
            }
            else
            {
                fechaInicioExport = fechaGeneracionUTC.AddDays(-1);
            }

            // Fecha fin
            fechaFinExport = fechaGeneracionUTC.AddSeconds(-1);

            //// BORRAR XAVIER
            //fechaInicioExport = new DateTime(2024, 02, 06, 11, 58, 52);
            //fechaFinExport = new DateTime(2024, 02, 21, 08, 12, 15);
            //// FI BORRAR XAVIER

            fechaInicioExport_txt = fechaInicioExport.ToString("yyyy-MM-ddTHH:mm:ss" + ".00Z");
            fechaFinExport_txt = fechaFinExport.ToString("yyyy-MM-ddTHH:mm:ss" + ".99Z");
        }



        private bool MarcarReportsExportadosSII(List<ReportsDTO_v3_1> reports)
        {
            bool result = false;

            try
            {
                // Marcamos como exportados los Reports en el SII
                List<SIIReportExportedDTO_v3_1_PUT> reportsSII_exportados = new List<SIIReportExportedDTO_v3_1_PUT>();
                string id;
                string statusSII = "";
                foreach (ReportsDTO_v3_1 report in reports)
                {
                    id = report.Id;
                    try
                    {
                        statusSII = captio_v3.GetSII_v3_1("\"" + "Id" + "\"" + ":" + id).Last().SIIStatus;
                    }
                    catch
                    {
                        statusSII = "";
                    }
                    if (statusSII.Equals("2"))
                    {
                        reportsSII_exportados.Add(new SIIReportExportedDTO_v3_1_PUT
                        {
                            Id = new string(id)
                        });
                    }
                }
                if (reportsSII_exportados.Count() > 0)
                {
                    foreach (SIIReportExportedDTO_v3_1_PUT reportSII_PUT in reportsSII_exportados)
                    {
                        Log.Info("Report Id [" + reportSII_PUT.Id + "], Extenal Id [" + reports.First(x => x.Id.Equals(reportSII_PUT.Id)).ExternalId.Trim() + "] marcado como EXPORTADO SII");
                    }
                    // MARCAR reports como EXPORTADOS SII
                    captio_v3.PutSIIReportExported_v3_1(reportsSII_exportados.ToArray());
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }

            return (result);
        }


        private DateTime CalcularFechaContable()
        {
            DateTime fecha = new DateTime(fechaGeneracion.Year, fechaGeneracion.Month, 05, 0, 0, 0);

            return (fecha);
        }


        private string Obtener_CuentaContable_Social (ExpensesDTO_v3_1 expense)
        {
            string result = (expense.Category == null) ? "" : (String.IsNullOrEmpty(expense.Category.Code) ? "" : expense.Category.Code).Trim();
            if (result.Contains("#"))
            {
                result = result.Split("#")[0];
            }

            return (result);
        }


        private string Obtener_CuentaContable_Analitico(ExpensesDTO_v3_1 expense)
        {
            string result = (expense.Category == null) ? "" : (String.IsNullOrEmpty(expense.Category.Code) ? "" : expense.Category.Code).Trim();
            if (result.Contains("#"))
            {
                result = result.Split("#")[0];
                string prefijo = result.Split("#")[1];

                if (result.Length >= prefijo.Length)
                {
                    for (int i = prefijo.Length; i > 0; i--)
                    {
                        result = result.Remove(i, 1).Insert(i, prefijo[i - 1].ToString());
                    }
                }
                else
                {
                    result = prefijo;
                }
            }
            return (result);

        }


        private bool EsTicketDieta_Inferior_al_Limite_Rossmann (ExpensesDTO_v3_1 expense, List<CompaniesCategoriesDTO_v3_1> categoriasCaptio)
        {
            bool result = false;

            string codigoCategoria = utils_captio.GetCategoryCode_Expense(expense, categoriasCaptio);

            if (!codigoCategoria.Contains("DIETA"))
            {
                result = false;
            }
            else if (!categoriasCaptio.Find(x => x.Id.Equals(expense.Category.Id)).SelfLimited.ToLower().Equals("true"))
            {
                result = false;
            }
            else
            {
                // decimal.Parse(tasaKM, CultureInfo.GetCultureInfo("en-US")).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                decimal limite = decimal.Parse(categoriasCaptio.Find(x => x.Id.Equals(expense.Category.Id)).MaxAmount, CultureInfo.GetCultureInfo("en-US"));
                decimal valorTicket = decimal.Parse(expense.FinalAmount.Value, CultureInfo.GetCultureInfo("en-US"));
                if (valorTicket < limite) 
                {
                    result = true;
                }
            }
            
            return (result);
        }


        private bool EsTicketDieta_Superior_O_Igual_al_Limite_Rossmann(ExpensesDTO_v3_1 expense, List<CompaniesCategoriesDTO_v3_1> categoriasCaptio)
        {
            bool result = false;

            string codigoCategoria = utils_captio.GetCategoryCode_Expense(expense, categoriasCaptio);

            if (!codigoCategoria.Contains("DIETA"))
            {
                result = false;
            }
            else if (!categoriasCaptio.Find(x => x.Id.Equals(expense.Category.Id)).SelfLimited.ToLower().Equals("true"))
            {
                result = false;
            }
            else
            {
                // decimal.Parse(tasaKM, CultureInfo.GetCultureInfo("en-US")).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                decimal limite = decimal.Parse(categoriasCaptio.Find(x => x.Id.Equals(expense.Category.Id)).MaxAmount, CultureInfo.GetCultureInfo("en-US"));
                decimal valorTicket = decimal.Parse(expense.FinalAmount.Value, CultureInfo.GetCultureInfo("en-US"));
                if (valorTicket >= limite)
                {
                    result = true;
                }
            }

            return (result);
        }


        private DateTime Obtener_Fecha_Generacion_SUMMAR (DateTime Fechagenercion)
        {
            DateTime result = new DateTime(Fechagenercion.Year, Fechagenercion.Month, 20, 0, 0, 0);

            if (Fechagenercion.Day < 20)
            {
                result = result.AddMonths(-1);
            }

            return (result);
        }

    }
}

