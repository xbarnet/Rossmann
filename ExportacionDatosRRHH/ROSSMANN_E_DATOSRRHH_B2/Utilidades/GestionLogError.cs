using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptioB2it.Utilidades
{
    public class GestionLogError
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class LogDTO
        {
            public DateTime FechaHora { get; set; }
            public string TipoError { get; set; }
            public string DescripcionError { get; set; }
            public InfoLogDTO InfoLog { get; set; }
        }

        public class InfoLogDTO
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public string Data { get; set; }
            public string Id { get; set; }
            public string FileName { get; set; }
            public string Line { get; set; }
            public string AdminLogin { get; set; }
            public string ExtraInfo { get; set; }
        }


        public string Entorno;
        public List<LogDTO> Logs;


        public GestionLogError(string entorno)
        {
            this.Entorno = entorno;
            this.Logs = new List<LogDTO>();
        }


        public void AddLog(DateTime fechaHora, string tipo, string descripcion, InfoLogDTO infoLog)
        {
            this.Logs.Add(new LogDTO()
            {
                FechaHora = fechaHora,
                TipoError = String.IsNullOrEmpty(tipo) ? "" : tipo,
                DescripcionError = String.IsNullOrEmpty(descripcion) ? "" : descripcion,
                InfoLog = new InfoLogDTO()
                {
                    Type = String.IsNullOrEmpty(infoLog.Type) ? "" : infoLog.Type,
                    Description = String.IsNullOrEmpty(infoLog.Description) ? "" : infoLog.Description,
                    Data = String.IsNullOrEmpty(infoLog.Data) ? "" : infoLog.Data,
                    Id = String.IsNullOrEmpty(infoLog.Id) ? "" : infoLog.Id,
                    FileName = String.IsNullOrEmpty(infoLog.FileName) ? "" : infoLog.FileName,
                    Line = String.IsNullOrEmpty(infoLog.Line) ? "" : infoLog.Line,
                    AdminLogin = String.IsNullOrEmpty(infoLog.AdminLogin) ? "" : infoLog.AdminLogin,
                    ExtraInfo = String.IsNullOrEmpty(infoLog.ExtraInfo) ? "" : infoLog.ExtraInfo,
                }  
            });
        }


        public void AddError(DateTime fechaHora, string tipoError, string descripcionError)
        {
            this.Logs.Add(new LogDTO()
            {
                FechaHora = fechaHora,
                TipoError = String.IsNullOrEmpty(tipoError) ? "" : tipoError,
                DescripcionError = String.IsNullOrEmpty(descripcionError) ? "" : descripcionError,
                InfoLog = new InfoLogDTO()
                {
                    Type = String.IsNullOrEmpty(tipoError) ? "" : tipoError,
                    Description = String.IsNullOrEmpty(descripcionError) ? "" : descripcionError,
                    Data = "",
                    Id = "",
                    FileName = "",
                    Line = "",
                    AdminLogin = "",
                    ExtraInfo = "",
                }
            });
        }


        public void AddListLogs(List<LogDTO> listaLogs)
        {
            foreach (LogDTO log in listaLogs)
            {
                this.Logs.Add(new LogDTO()
                {
                    FechaHora = log.FechaHora,
                    TipoError = String.IsNullOrEmpty(log.TipoError) ? "" : log.TipoError,
                    DescripcionError = String.IsNullOrEmpty(log.DescripcionError) ? "" : log.DescripcionError,
                    InfoLog = new InfoLogDTO()
                    {
                        Type = String.IsNullOrEmpty(log.InfoLog.Type) ? "" : log.InfoLog.Type,
                        Description = String.IsNullOrEmpty(log.InfoLog.Description) ? "" : log.InfoLog.Description,
                        Data = String.IsNullOrEmpty(log.InfoLog.Data) ? "" : log.InfoLog.Data,
                        Id = String.IsNullOrEmpty(log.InfoLog.Id) ? "" : log.InfoLog.Id,
                        FileName = String.IsNullOrEmpty(log.InfoLog.FileName) ? "" : log.InfoLog.FileName,
                        Line = String.IsNullOrEmpty(log.InfoLog.Line) ? "" : log.InfoLog.Line,
                        AdminLogin = String.IsNullOrEmpty(log.InfoLog.AdminLogin) ? "" : log.InfoLog.AdminLogin,
                        ExtraInfo = String.IsNullOrEmpty(log.InfoLog.ExtraInfo) ? "" : log.InfoLog.ExtraInfo,
                    }
                });
            }
        }


        public void Clear()
        {
            this.Logs.Clear();
        }

        public int Count()
        {
            return (this.Logs.Count);
        }


        public List<LogDTO> GetLogs(string tipoError = null)
        {
            if (tipoError != null)
            {
                switch (tipoError.ToUpper())
                {
                    case "OK":
                        return (this.Logs.FindAll(x => x.TipoError.ToUpper().Equals("OK")));
                    case "NOK":
                        return (this.Logs.FindAll(x => x.TipoError.ToUpper().Equals("NOK")));
                    case "WARNING":
                        return (this.Logs.FindAll(x => x.TipoError.ToUpper().Equals("WARNING")));
                    case "ERROR":
                        return (this.Logs.FindAll(x => (!x.TipoError.ToUpper().Equals("WARNING")) && (!x.TipoError.ToUpper().Equals("NOK")) && (!x.TipoError.ToUpper().Equals("OK"))));
                    default:
                        return (this.Logs);
                }
            }
            return (this.Logs);
        }


        public void AfegirErrorsToLog(List<Ficheros.LogErroresDTO> errors)
        {
            foreach(Ficheros.LogErroresDTO error in errors)
            {
                this.AddLog(error.FechaHora, error.TipoError, error.DescripcionError, new InfoLogDTO
                {
                    Type = String.IsNullOrEmpty(error.TipoError) ? "" : error.TipoError,
                    Description = String.IsNullOrEmpty(error.DescripcionError) ? "" : error.DescripcionError,
                    Data = "",
                    Id = "",
                    FileName = "",
                    Line = "",
                    AdminLogin = "",
                    ExtraInfo = "",
                });
            }
        }


        //// Convertir estructura de logs a string
        //// type = 1 : log con todos los campos
        //// type = 2 : log solo con los dos primeros campos
        //// cabecera = true : Se incluye la cabezera
        //public string ConvertirLogToString_antic (List<LogDTO> logs, int type, bool cabecera)
        //{
        //    int l = 20;
        //    string result = "";

        //    if (cabecera)
        //    {
        //        switch (type)
        //        {
        //            case 1:
        //                // new string(report.ExternalId.Take(25).ToArray()),
        //                result = result + " ".PadLeft("9999-99-99 99:99:99 LOG  ".Length) + "Type".PadRight(l) + "- " + "Description".PadRight(l) + "- " + "Data".PadRight(l) + "- " + "Id".PadRight(l) + "- " + "FileName".PadRight(l) + "- " + "Line".PadRight(l) + "- " + "AdminLogin".PadRight(l) + "- " + "ExtraInfo" + Environment.NewLine;
        //                break;
        //            case 2:
        //                result = result + " ".PadLeft("9999-99-99 99:99:99 LOG  ".Length) + "Subject".PadRight(l) + "- " + "Error Description" + Environment.NewLine;
        //                break;
        //            default:
        //                break;
        //        }

        //    }

        //    foreach (LogDTO log in logs)
        //    {
        //        switch (type)
        //        {
        //            case 1:
        //                // new string(report.ExternalId.Take(25).ToArray()),
        //                result = result + log.FechaHora.ToString("yyyy-MM-dd HH:mm:ss LOG  ") + log.InfoLog.Type.PadRight(l) + "- " + log.InfoLog.Description.PadRight(l) + "- " + log.InfoLog.Data.PadRight(l) + "- " + log.InfoLog.Id.PadRight(l) + "- " + log.InfoLog.FileName.PadRight(l) + "- " + log.InfoLog.Line.PadRight(l) + "- " + log.InfoLog.AdminLogin.PadRight(l) + "- " + log.InfoLog.ExtraInfo + Environment.NewLine;
        //                break;
        //            case 2:
        //                result = result + log.FechaHora.ToString("yyyy-MM-dd HH:mm:ss LOG  ") + log.InfoLog.Type.PadRight(l) + "- " + log.InfoLog.Description + Environment.NewLine;
        //                break;
        //            default:
        //                break;
        //        }
            
        //    }
        //    return (result);
        //}




        // Convertir estructura de logs a string
        // type = 1 : log con todos los campos
        // type = 2 : log solo con los dos primeros campos
        // cabecera = true : Se incluye la cabezera
        public static string ConvertirLogToString(List<LogDTO> logs, int type, bool cabecera)
        {
            int[] l = new int[8];
            string[] txt_cabecera = new string[8];

            /*
            public string Type { get; set; }
            public string Description { get; set; }
            public string Data { get; set; }
            public string Id { get; set; }
            public string FileName { get; set; }
            public string Line { get; set; }
            public string AdminLogin { get; set; }
            public string ExtraInfo { get; set; }
             */

            // Texto de las cabeceras
            switch (type)
            {
                case 1:
                    txt_cabecera[0] = "Información";  // Type
                    txt_cabecera[1] = "Descripción";  // Description
                    txt_cabecera[2] = "Usuario";  // Data
                    txt_cabecera[3] = "Nombre informe";  // Id
                    txt_cabecera[4] = "Id informe";  // FileName
                    txt_cabecera[5] = "";  // Line
                    txt_cabecera[6] = "";  // AdminLogin
                    txt_cabecera[7] = "";  // ExtraInfo
                    break;
                case 2:
                    txt_cabecera[0] = "Información";  // Type
                    txt_cabecera[1] = "Descripción Error";  // Description
                    break;
                default:
                    break;
            }

            // Calcular la longitud dels camps
            switch (type)
            {
                case 1:
                    try
                    {
                        l[0] = Math.Max(logs.Select(x => x.InfoLog.Type.Length).Max(), txt_cabecera[0].Length);
                        l[1] = Math.Max(logs.Select(x => x.InfoLog.Description.Length).Max(), txt_cabecera[1].Length);
                        l[2] = Math.Max(logs.Select(x => x.InfoLog.Data.Length).Max(), txt_cabecera[2].Length);
                        l[3] = Math.Max(logs.Select(x => x.InfoLog.Id.Length).Max(), txt_cabecera[3].Length);
                        l[4] = Math.Max(logs.Select(x => x.InfoLog.FileName.Length).Max(), txt_cabecera[4].Length);
                        l[5] = Math.Max(logs.Select(x => x.InfoLog.Line.Length).Max(), txt_cabecera[5].Length);
                        l[6] = Math.Max(logs.Select(x => x.InfoLog.AdminLogin.Length).Max(), txt_cabecera[6].Length);
                        l[7] = Math.Max(logs.Select(x => x.InfoLog.ExtraInfo.Length).Max(), txt_cabecera[7].Length);
                    }
                    catch
                    {
                        l[0] = txt_cabecera[0].Length;
                        l[1] = txt_cabecera[1].Length;
                        l[2] = txt_cabecera[2].Length;
                        l[3] = txt_cabecera[3].Length;
                        l[4] = txt_cabecera[4].Length;
                        l[5] = txt_cabecera[5].Length;
                        l[6] = txt_cabecera[6].Length;
                        l[7] = txt_cabecera[7].Length;
                    }
                    break;
                case 2:
                    try
                    {
                        l[0] = Math.Max(logs.Select(x => x.InfoLog.Type.Length).Max(), txt_cabecera[0].Length);
                        l[1] = Math.Max(logs.Select(x => x.InfoLog.Description.Length).Max(), txt_cabecera[1].Length);
                    }
                    catch
                    {
                        l[0] = txt_cabecera[0].Length;
                        l[1] = txt_cabecera[1].Length;
                    }
                    break;
                default:
                    break;
            }

            string result = "";

            if (cabecera)
            {
                switch (type)
                {
                    case 1:
                        //result = result + " ".PadLeft("9999-99-99 99:99:99 LOG  ".Length) + ((l[0] > 0) ? "Type".PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? "Description".PadRight(l[1]) + " | " : "") + ((l[2] > 0) ? "Data".PadRight(l[2]) + " | " : "") + ((l[3] > 0) ? "Id".PadRight(l[3]) + " | " : "") + ((l[4] > 0) ? "FileName".PadRight(l[4]) + " | " : "") + ((l[5] > 0) ? "Line".PadRight(l[5]) + " | " : "") + ((l[6] > 0) ? "AdminLogin".PadRight(l[6]) + " | " : "") + ((l[7] > 0) ? "ExtraInfo" : "") + Environment.NewLine;
                        result = result + ((l[0] > 0) ? txt_cabecera[0].PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? txt_cabecera[1].PadRight(l[1]) + " | " : "") + ((l[2] > 0) ? txt_cabecera[2].PadRight(l[2]) + " | " : "") + ((l[3] > 0) ? txt_cabecera[3].PadRight(l[3]) + " | " : "") + ((l[4] > 0) ? txt_cabecera[4].PadRight(l[4]) + " | " : "") + ((l[5] > 0) ? txt_cabecera[5].PadRight(l[5]) + " | " : "") + ((l[6] > 0) ? txt_cabecera[6].PadRight(l[6]) + " | " : "") + ((l[7] > 0) ? txt_cabecera[7] : "") + Environment.NewLine;
                        break;
                    case 2:
                        //result = result + " ".PadLeft("9999-99-99 99:99:99 LOG  ".Length) + ((l[0] > 0) ? "Subject".PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? "Error Description" : "") + Environment.NewLine;
                        result = result + ((l[0] > 0) ? txt_cabecera[0].PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? txt_cabecera[1] : "") + Environment.NewLine;
                        break;
                    default:
                        break;
                }
            }

            foreach (LogDTO log in logs)
            {
                switch (type)
                {
                    case 1:
                        //result = result + log.FechaHora.ToString("yyyy-MM-dd HH:mm:ss LOG  ") + ((l[0] > 0) ? log.InfoLog.Type.PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? log.InfoLog.Description.PadRight(l[1]) + " | " : "") + ((l[2] > 0) ? log.InfoLog.Data.PadRight(l[2]) + " | " : "") + ((l[3] > 0) ? log.InfoLog.Id.PadRight(l[3]) + " | " : "") + ((l[4] > 0) ? log.InfoLog.FileName.PadRight(l[4]) + " | " : "") + ((l[5] > 0) ? log.InfoLog.Line.PadRight(l[5]) + " | " : "") + ((l[6] > 0) ? log.InfoLog.AdminLogin.PadRight(l[6]) + " | " : "") + ((l[7] > 0) ? log.InfoLog.ExtraInfo : "") + Environment.NewLine;
                        result = result + ((l[0] > 0) ? log.InfoLog.Type.PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? log.InfoLog.Description.PadRight(l[1]) + " | " : "") + ((l[2] > 0) ? log.InfoLog.Data.PadRight(l[2]) + " | " : "") + ((l[3] > 0) ? log.InfoLog.Id.PadRight(l[3]) + " | " : "") + ((l[4] > 0) ? log.InfoLog.FileName.PadRight(l[4]) + " | " : "") + ((l[5] > 0) ? log.InfoLog.Line.PadRight(l[5]) + " | " : "") + ((l[6] > 0) ? log.InfoLog.AdminLogin.PadRight(l[6]) + " | " : "") + ((l[7] > 0) ? log.InfoLog.ExtraInfo : "") + Environment.NewLine;
                        break;
                    case 2:
                        //result = result + log.FechaHora.ToString("yyyy-MM-dd HH:mm:ss LOG  ") + ((l[0] > 0) ? log.InfoLog.Type.PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? log.InfoLog.Description : "") + Environment.NewLine;
                        result = result + ((l[0] > 0) ? log.InfoLog.Type.PadRight(l[0]) + " | " : "") + ((l[1] > 0) ? log.InfoLog.Description : "") + Environment.NewLine;
                        break;
                    default:
                        break;
                }

            }
            return (result);
        }




        public string Eliminar_Textos_API_Captio_de_Log(string log)
        {
            string result = "";

            foreach (string line in log.Split(Environment.NewLine))
            {
                if (!line.Contains("@@@"))
                {
                    result = result + line + Environment.NewLine;
                }
            }
            return (result);
        }

    }
}
