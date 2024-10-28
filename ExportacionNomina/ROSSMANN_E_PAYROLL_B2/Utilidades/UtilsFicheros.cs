using ExcelDataReader;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using CaptioB2it.Ficheros;

namespace CaptioB2it.Utilidades
{
    public class UtilsFicheros
    {

        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // Llegir fitxer moviments tarjes del Bankinter
        public List<string> LlegirFitxerTxt_Banks(string RemoteFileName)
        {
            try
            {
                List<string> lineasFitxer = new List<string>();
                // Llegir totes les línies del fitxer
                lineasFitxer.AddRange(File.ReadLines(RemoteFileName, System.Text.Encoding.Default));
                // Log
                Log.Info("FICHERO TXT LEIDO CORRECTAMENTE [" + RemoteFileName + "]");
                return lineasFitxer;
            }
            catch (Exception ex)
            {
                // Log
                Log.Error("---> PROBLEMA AL LEER FICHERO TXT [" + RemoteFileName + "]");
                Log.Error(ex.Message);
                return null;
            }
        }


        public string CalcularIban(string ccc)
        {
            // Calculamos el IBAN
            ccc = ccc.Trim();
            if (ccc.Length != 20)
            {
                return "La CCC debe tener 20 dígitos";
            }
            // Le añadimos el codigo del pais al ccc
            ccc = ccc + "142800";

            // Troceamos el ccc en partes (26 digitos)
            string[] partesCCC = new string[5];
            partesCCC[0] = ccc.Substring(0, 5);
            partesCCC[1] = ccc.Substring(5, 5);
            partesCCC[2] = ccc.Substring(10, 5);
            partesCCC[3] = ccc.Substring(15, 5);
            partesCCC[4] = ccc.Substring(20, 6);

            int iResultado = int.Parse(partesCCC[0]) % 97;
            string resultado = iResultado.ToString();
            for (int i = 0; i < partesCCC.Length - 1; i++)
            {
                iResultado = int.Parse(resultado + partesCCC[i + 1]) % 97;
                resultado = iResultado.ToString();
            }
            // Le restamos el resultado a 98
            int iRestoIban = 98 - int.Parse(resultado);
            string restoIban = iRestoIban.ToString();
            if (restoIban.Length == 1)
                restoIban = "0" + restoIban;

            return "ES" + restoIban + ccc;
        }


        public string ObtenerDigitoControl_1(string valor)
        {
            int[] valores = new int[] { 4, 8, 5, 10, 9, 7, 3, 6 };
            int control = 0;
            int i;
            for (i = 0; i <= 7; i++)
            {
                control += Convert.ToInt16(valor.Substring(i, 1)) * valores[i];
            }
            control = 11 - (control % 11);
            if (control == 11)
            {
                control = 0;
            }
            else if (control == 10)
            {
                control = 1;
            }
            return (control.ToString());
        }


        public string ObtenerDigitoControl_2(string valor)
        {
            int[] valores = new int[] { 1, 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            int control = 0;
            int i;
            for (i = 0; i <= 9; i++)
            {
                control += Convert.ToInt16(valor.Substring(i, 1)) * valores[i];
            }
            control = 11 - (control % 11);
            if (control == 11)
            {
                control = 0;
            }
            else if (control == 10)
            {
                control = 1;
            }
            return (control.ToString());
        }


        // Rutina para convertir ficheros CSV a XLSX
        public void ConvertCSV_to_XLSX(string NomFitxerSenseExtensio, string Extensio, string Directori, bool BorrarFitxerCSV)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                string fitxer = Directori + "\\" + NomFitxerSenseExtensio + Extensio;
                string fitxer_sense_extensio = Directori + "\\" + NomFitxerSenseExtensio;

                using (ExcelPackage package = new ExcelPackage())
                {
                    //Create the Worksheet
                    var sheet = package.Workbook.Worksheets.Add("Captio");
                    //Create the format object to describe the text file
                    var format = new ExcelTextFormat
                    {
                        Delimiter = ';'
                    };
                    //format.SkipLinesBeginning = 0;
                    CultureInfo ci = new CultureInfo("es-ES");          //Use your choice of Culture
                    ci.NumberFormat.NumberDecimalSeparator = ".";       //Decimal is comma
                    format.Culture = ci;
                    format.Encoding = System.Text.Encoding.Default;
                    format.DataTypes = new eDataTypes[] { eDataTypes.Number, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.Number,
                                                        eDataTypes.String, eDataTypes.Number, eDataTypes.Number,
                                                        eDataTypes.Number, eDataTypes.Number, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,
                                                        eDataTypes.String, eDataTypes.String, eDataTypes.String,};
                    //Now read the file into the sheet.
                    var range = sheet.Cells["A1"].LoadFromText(new FileInfo(fitxer), format);
                    package.SaveAs(new FileInfo(fitxer_sense_extensio + ".xlsx"));
                }
                // Log
                Log.Info("FICHERO XLSX GENERADO CORRECTAMENTE [" + fitxer_sense_extensio + ".xlsx" + "]");

                // Borrar fichero .csv
                if (BorrarFitxerCSV)
                {
                    File.Delete(fitxer);

                    // Log
                    //Log.Info("FICHERO CSV ELIMINADO CORRECTAMENTE [" + fitxer + "]");
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }
        }


        // Rutina para convertir ficheros XLS a TXT separando las columnas con el caracter especificado
        // Si senseEspais = true no es consideren les columnes que només tinguin espais en blanc
        public int ConvertXLSX_to_TXT(string NomFitxerSenseExtensio, string Extensio, string Directori, string Separador, bool senseEspais)
        {
            try
            {
                string fitxer = Directori + "\\" + NomFitxerSenseExtensio + Extensio;
                string fitxer_sense_extensio = Directori + "\\" + NomFitxerSenseExtensio;
                string fila = "";

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //open file and returns as Stream
                using (var stream2 = File.Open(fitxer, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream2))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(fitxer_sense_extensio + ".txt"))
                        {
                            do
                            {
                                while (reader.Read())
                                {
                                    fila = "";
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (senseEspais)
                                        {
                                            if ((reader.GetString(i) != null) && (reader.GetString(i).Trim() != ""))
                                            {
                                                fila = fila + ((fila != "") ? Separador : "") + reader.GetString(i);
                                            }
                                        }
                                        else
                                        {
                                            if ((reader.GetString(i) != null) && (reader.GetString(i) != ""))
                                            {
                                                fila = fila + ((fila != "") ? Separador : "") + reader.GetString(i);
                                            }
                                        }
                                    }
                                    file.WriteLine(fila);
                                }
                            } while (reader.NextResult());
                        }
                    }
                }
                // Log
                Log.Info("FICHERO TXT GENERADO CORRECTAMENTE [" + fitxer_sense_extensio + ".txt" + "]");
                return (0);
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }
            return (-1);
        }

        // Rutina per llegir els workflows del fitxer plantilla
        public List<WorkflowsCargaDTO> Llegir_Workflows_de_XLSX(string fitxer, string nom_pestanya, string type)
        {
            List<WorkflowsCargaDTO> result = new List<WorkflowsCargaDTO>();
            string Separador = "|#|#|";

            try
            {
                List<string> linees_fitxer = new List<string>();
                string fila = "";
                string[] valors_fitxer;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //open file and returns as Stream
                using (var stream2 = File.Open(fitxer, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream2))
                    {
                        do
                        {
                            if (reader.Name.Equals(nom_pestanya))
                            {
                                while (reader.Read())
                                {
                                    fila = "";
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        var valor = reader.GetValue(i);
                                        if ((valor != null) && !(String.IsNullOrEmpty(valor.ToString())))
                                        //if ((reader.GetString(i) != null) && (reader.GetString(i) != ""))
                                        {
                                            fila = fila + ((fila != "") ? Separador : "") + valor.ToString();
                                        }
                                        else
                                        {
                                            fila = fila + Separador;
                                        }
                                    }
                                    //Console.WriteLine(fila);
                                    if ((!fila.Contains("nombre|#|#|nombre_etapa|#|#|login_supervisor")) && !String.IsNullOrEmpty(fila.Replace(Separador, "")))
                                    {
                                        linees_fitxer.Add(new string(fila));
                                    }
                                }
                            }
                        } while (reader.NextResult());
                        
                    } 
                }

                foreach(string linea in linees_fitxer)
                {
                    valors_fitxer = linea.Split(Separador);
                    List<WorkflowsCargaDTO_etapas> etapas = new List<WorkflowsCargaDTO_etapas>();

                    foreach (string columna in Extreure_etapes_linea_fitxer(valors_fitxer, Separador))
                    {
                        etapas.Add(new WorkflowsCargaDTO_etapas
                        {
                            Nombre_etapa = columna.Split(Separador)[0],
                            Login_supervisor = columna.Split(Separador)[1]
                        });
                    }
                    result.Add(new WorkflowsCargaDTO
                    {
                        Nombre = valors_fitxer[0],
                        Type = type,
                        Etapas = etapas
                    });
                }

                // Log
                Log.Info("LLEGIDA CORRECTAMENT LA PESTANYA [" + nom_pestanya + "] DEL FITXER [" + fitxer + "]");
                return (result);
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }
            result.Clear();
            return (result);
        }


        private string[] Extreure_etapes_linea_fitxer (string[] columnes, string separador)
        {
            List<string> result = new List<string>();

            for (int columna = 1; columna < columnes.Length; columna = columna + 2)
            {
                if ((!String.IsNullOrEmpty(columnes[columna])) && (!String.IsNullOrEmpty(columnes[columna])))
                {
                    result.Add(new string(columnes[columna]) + separador + (columnes[columna+1]));
                }
            }
            return (result.ToArray());
        }


        // Rutina per llegir els usuaris del fitxer plantilla
        public List<UsuariosCargaDTO> Llegir_Usurais_de_XLSX(string fitxer, string nom_pestanya)
        {
            List<UsuariosCargaDTO> result = new List<UsuariosCargaDTO>();
            string Separador = "|#|#|";

            try
            {
                List<string> linees_fitxer = new List<string>();
                string fila = "";
                string[] valors_fitxer;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //open file and returns as Stream
                using (var stream2 = File.Open(fitxer, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream2))
                    {
                        do
                        {
                            if (reader.Name.Equals(nom_pestanya))
                            {
                                while (reader.Read())
                                {
                                    fila = "";
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        var valor = reader.GetValue(i);
                                        if ((valor != null) && !(String.IsNullOrEmpty(valor.ToString())))
                                        //if ((reader.GetString(i) != null) && (reader.GetString(i) != ""))
                                        {
                                            fila = fila + ((fila != "") ? Separador : "") + valor.ToString();
                                        }
                                        else
                                        {
                                            fila = fila + Separador;
                                        }
                                    }
                                    //Console.WriteLine(fila);
                                    if (!String.IsNullOrEmpty(fila) && (!fila.Contains("login|#|#|email|#|#|nombre")) && !String.IsNullOrEmpty(fila.Replace(Separador,"")))
                                    {
                                        linees_fitxer.Add(new string(fila));
                                    }
                                }
                            }
                        } while (reader.NextResult());

                    }
                }

                foreach (string linea in linees_fitxer)
                {
                    valors_fitxer = linea.Split(Separador);
                    result.Add(new UsuariosCargaDTO
                    {
                        Login = (valors_fitxer.Length > 0) ? valors_fitxer[0] : "",
                        Email = (valors_fitxer.Length > 1) ? valors_fitxer[1] : "",
                        Nombre = (valors_fitxer.Length > 2) ? valors_fitxer[2] : "",
                        Codigo_empresa = (valors_fitxer.Length > 3) ? valors_fitxer[3] : "",
                        Codigo_empleado = (valors_fitxer.Length > 4) ? valors_fitxer[4] : "",
                        Centro_coste = (valors_fitxer.Length > 5) ? valors_fitxer[5] : "",
                        Grupo_km = (valors_fitxer.Length > 6) ? valors_fitxer[6] : "",
                        Grupo_usuarios = (valors_fitxer.Length > 7) ? valors_fitxer[7] : "",
                        Workflow_informes = (valors_fitxer.Length > 8) ? valors_fitxer[8] : "",
                        Workflow_anticipos = (valors_fitxer.Length > 9) ? valors_fitxer[9] : "",
                    });
                }

                // Log
                Log.Info("LLEGIDA CORRECTAMENT LA PESTANYA [" + nom_pestanya + "] DEL FITXER [" + fitxer + "]");
                return (result);
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }
            result.Clear();
            return (result);
        }


        // Rutina per llegir els workflows del fitxer plantilla
        public List<ListaValoresCargaDTO> Llegir_Taules_de_XLSX(string fitxer, string nom_pestanya)
        {
            List<ListaValoresCargaDTO> result = new List<ListaValoresCargaDTO>();
            string Separador = "|#|#|";

            try
            {
                List<string> linees_fitxer = new List<string>();
                string fila = "";
                string[] valors_fitxer;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //open file and returns as Stream
                using (var stream2 = File.Open(fitxer, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream2))
                    {
                        do
                        {
                            if (reader.Name.Equals(nom_pestanya))
                            {
                                while (reader.Read())
                                {
                                    fila = "";
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        var valor = reader.GetValue(i);
                                        if ((valor != null) && !(String.IsNullOrEmpty(valor.ToString())))
                                        //if ((reader.GetString(i) != null) && (reader.GetString(i) != ""))
                                        {
                                            fila = fila + ((fila != "") ? Separador : "") + valor.ToString();
                                        }
                                        else
                                        {
                                            fila = fila + Separador;
                                        }
                                    }
                                    //Console.WriteLine(fila);
                                    if ((!fila.Contains("Nombre|#|#|Codigo")) && !String.IsNullOrEmpty(fila.Replace(Separador, "")))
                                    {
                                        linees_fitxer.Add(new string(fila));
                                    }
                                }
                            }
                        } while (reader.NextResult());

                    }
                }

                foreach (string linea in linees_fitxer)
                {
                    valors_fitxer = linea.Split(Separador);
                    result.Add(new ListaValoresCargaDTO
                    {
                        Codigo = valors_fitxer[1],
                        Descripcion = valors_fitxer[0]
                    });
                }

                // Log
                Log.Info("LLEGIDA CORRECTAMENT LA PESTANYA [" + nom_pestanya + "] DEL FITXER [" + fitxer + "]");
                return (result);
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }
            result.Clear();
            return (result);
        }



        // Rutina per moure un fitxer de directori
        // Retorna 0 si s'ha mogut correctament
        public void MoureFitxerDeDirectori(string fullName_Origen, string fullName_Desti)
        {

            // Moure fitxer al directori de Processats
            //string fullName_fitxersPendents = Directory.GetCurrentDirectory() + "\\" + program.appSettings.directorioFicherosPendientes + "\\" + fitxer;
            //string fullName_fitxersProcessats = Directory.GetCurrentDirectory() + "\\" + program.appSettings.directorioFicherosProcesados + "\\" + fitxer;
            try
            {
                if (!File.Exists(fullName_Origen))
                {
                    // This statement ensures that the file is created,
                    // but the handle is not kept.
                    using (FileStream fs = File.Create(fullName_Origen)) { }
                }

                // Ensure that the target does not exist.
                if (File.Exists(fullName_Desti))
                    File.Delete(fullName_Desti);

                // Move the file.
                File.Move(fullName_Origen, fullName_Desti);
                // See if the original exists now.
                //if (File.Exists(fullName_Origen))
                //{
                //    Console.WriteLine("The original file still exists, which is unexpected.");
                //}
                //else
                //{
                //    Console.WriteLine("The original file no longer exists, which is expected.");
                //}
            }
            catch (Exception ex)
            {
                // Log
                Log.Error("---> PROBLEMA AL MOVER EL FICHERO DE DIRECTORIO : " + "[" + fullName_Origen + "] - [" + fullName_Desti + "]");
                Log.Error(ex.Message);
            }
        }


        public bool IsValidEmail(string email)
        {
            String sFormato;
            sFormato = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (System.Text.RegularExpressions.Regex.IsMatch(email, sFormato))
            {
                if (System.Text.RegularExpressions.Regex.Replace(email, sFormato, String.Empty).Length == 0)
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

    }
}

