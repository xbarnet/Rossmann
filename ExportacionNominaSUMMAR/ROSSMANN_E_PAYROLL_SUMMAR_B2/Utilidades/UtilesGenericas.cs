using CaptioB2it.Ficheros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CaptioB2it.Utilidades
{

    public class UtilesGenericas
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static String Extraer_Campo(string texto, string separador, int posicion)
        {
            try
            {
                return (texto.Split(separador)[posicion-1]);
            }
            catch
            {
                return texto;
            }
        }


        //
        // Valida si un string de formato DDMMAAAA es una fecha correcta
        public static Boolean Valida_FECHA_DDMMAA(string fecha)
        {
            if (String.IsNullOrEmpty(fecha) || fecha.Length < 6)
                return false;

            try
            {
                var dia = fecha.Substring(0, 2);
                var mes = fecha.Substring(2, 2);
                var anyo = fecha.Substring(4, 2);
                DateTime.Parse(dia+"/"+mes+"/"+anyo);
                return true;
            }
            catch
            {
                return false;
            }
        }


        //
        // Valida si un string de formato AAAAMMDD es una fecha correcta
        public static Boolean Valida_FECHA_AAAAMMDD(string fecha)
        {
            if (String.IsNullOrEmpty(fecha) || fecha.Length < 8)
                return false;

            try
            {
                var dia = fecha.Substring(6, 2);
                var mes = fecha.Substring(4, 2);
                var anyo = fecha.Substring(0, 4);
                DateTime.Parse(dia + "/" + mes + "/" + anyo);
                return true;
            }
            catch
            {
                return false;
            }
        }


        //
        // Valida si una Fecha se encuentra entre el rango de fechas indicado
        public static Boolean Fecha_dentro_rango(string fechaCaptio, string fechaInicial, string fechaFinal)
        {
            try
            {
                DateTime fecha = Convert.ToDateTime((fechaCaptio.Contains("T")) ? (fechaCaptio.Split("T")[0]) : fechaCaptio);
                DateTime fecha1 = Convert.ToDateTime(fechaInicial);
                DateTime fecha2 = Convert.ToDateTime(fechaFinal);

                if ((fecha >= fecha1) && (fecha <= fecha2))
                {
                    //esta en el rango
                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Log.Error("---> FORMATO DE FECHA INCORRECTA");
                return false;
            }
        }


        //
        // Devuelve la fecha del último dia del mes de una fecha
        public static DateTime GetFechaUltimoDiaMes(DateTime fecha)
        {
            DateTime oPrimerDiaDelMes = new DateTime(fecha.Year, fecha.Month, 1);
            DateTime oUltimoDiaDelMes = oPrimerDiaDelMes.AddMonths(1).AddDays(-1);
            return (oUltimoDiaDelMes);
        }


        //
        // Valida si el valor data corresponde a un NIF/CIF/NIE válido
        public static Boolean Valida_NIFCIFNIE(string data)
        {
            if (String.IsNullOrEmpty(data) || data.Length < 8)
                return false;

            var initialLetter = data.Substring(0, 1).ToUpper();
            if (Char.IsLetter(data, 0))
            {
                switch (initialLetter)
                {
                    case "X":
                        data = "0" + data.Substring(1, data.Length - 1);
                        return ValidarNIF(data);
                    case "Y":
                        data = "1" + data.Substring(1, data.Length - 1);
                        return ValidarNIF(data);
                    case "Z":
                        data = "2" + data.Substring(1, data.Length - 1);
                        return ValidarNIF(data);
                    default:
                        if (new Regex("[A-Za-z][0-9]{7}[A-Za-z0-9]{1}$").Match(data).Success)
                            return ValidadCIF(data);
                        break;
                }
            }
            else if (Char.IsLetter(data, data.Length - 1))
            {
                if (new Regex("[0-9]{8}[A-Za-z]").Match(data).Success || new Regex("[0-9]{7}[A-Za-z]").Match(data).Success)
                    return ValidarNIF(data);
            }
            return false;
        }

        private static string GetLetra(int id)
        {
            Dictionary<int, String> letras = new Dictionary<int, string>
            {
                { 0, "T" },
                { 1, "R" },
                { 2, "W" },
                { 3, "A" },
                { 4, "G" },
                { 5, "M" },
                { 6, "Y" },
                { 7, "F" },
                { 8, "P" },
                { 9, "D" },
                { 10, "X" },
                { 11, "B" },
                { 12, "N" },
                { 13, "J" },
                { 14, "Z" },
                { 15, "S" },
                { 16, "Q" },
                { 17, "V" },
                { 18, "H" },
                { 19, "L" },
                { 20, "C" },
                { 21, "K" },
                { 22, "E" }
            };
            return letras[id];
        }

        private static bool ValidarNIF(string data)
        {
            if (data == String.Empty)
                return false;
            try
            {
                String letra;
                letra = data.Substring(data.Length - 1, 1);
                data = data.Substring(0, data.Length - 1);
                int nifNum = int.Parse(data);
                int resto = nifNum % 23;
                string tmp = GetLetra(resto);
                if (tmp.ToLower() != letra.ToLower())
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private static bool ValidadCIF(string data)
        {
            try
            {
                int pares = 0;
                int impares = 0;
                int suma;
                string ultima;
                int unumero;
                string[] uletra = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "0" };
                string[] fletra = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
                int[] fletra1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                string xxx;

                /*
                * T      P      P      N  N  N  N  N  C
                Siendo:
                T: Letra de tipo de Organización, una de las siguientes: A,B,C,D,E,F,G,H,K,L,M,N,P,Q,S.
                P: Código provincial.
                N: Númeración secuenial dentro de la provincia.
                C: Dígito de control, un número ó letra: Aó1,Bó2,Có3,Dó4,Eó5,Fó6,Gó7,Hó8,Ió9,Jó0.
                *
                *
                A.    Sociedades anónimas.
                B.    Sociedades de responsabilidad limitada.
                C.    Sociedades colectivas.
                D.    Sociedades comanditarias.
                E.    Comunidades de bienes y herencias yacentes.
                F.    Sociedades cooperativas.
                G.    Asociaciones.
                H.    Comunidades de propietarios en régimen de propiedad horizontal.
                I.    Sociedades civiles, con o sin personalidad jurídica.
                J.    Corporaciones Locales.
                K.    Organismos públicos.
                L.    Congregaciones e instituciones religiosas.
                M.    Órganos de la Administración del Estado y de las Comunidades Autónomas.
                N.    Uniones Temporales de Empresas.
                O.    Otros tipos no definidos en el resto de claves.

                */
                data = data.ToUpper();

                ultima = data.Substring(8, 1);

                int cont = 1;
                for (cont = 1; cont < 7; cont++)
                {
                    xxx = (2 * int.Parse(data.Substring(cont++, 1))) + "0";
                    impares += int.Parse(xxx.ToString().Substring(0, 1)) + int.Parse(xxx.ToString().Substring(1, 1));
                    pares += int.Parse(data.Substring(cont, 1));
                }

                xxx = (2 * int.Parse(data.Substring(cont, 1))) + "0";
                impares += int.Parse(xxx.Substring(0, 1)) + int.Parse(xxx.Substring(1, 1));

                suma = pares + impares;
                unumero = int.Parse(suma.ToString().Substring(suma.ToString().Length - 1, 1));
                unumero = 10 - unumero;
                if (unumero == 10) unumero = 0;

                if ((ultima == unumero.ToString()) || (ultima == uletra[unumero - 1]))
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        public static bool IsValidEmail(string email)
        {
            String sFormato;
            //sFormato = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            sFormato = "^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$";
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




        // <summary>
        // Convierte una imagen en una cadena de texto
        // </summary>
        // <param name="Path">Ruta la la imagen</param>
        // <returns>cadena de texto</returns>
        public static string ConvertirImatgeToString(string Path)
        {
            byte[] imageArray = System.IO.File.ReadAllBytes(Path);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            return base64ImageRepresentation;
        }


        // <summary>
        // Convierte string en Base64 a texto
        // </summary>
        // <param name="valor">Valor a reemplazar</param>
        // <returns></returns>
        public static string DecodeBase64ToString(string valor)
        {
            byte[] myBase64ret = Convert.FromBase64String(valor);
            string myStr = System.Text.Encoding.UTF8.GetString(myBase64ret);
            return myStr;
        }


        // <summary>
        // Convierte texto string en Base64
        // </summary>
        // <param name="valor">Valor a reemplazar</param>
        // <returns></returns>
        public static string EncodeStringToBase64(string valor)
        {
            byte[] myByte = System.Text.Encoding.UTF8.GetBytes(valor);
            string myBase64 = Convert.ToBase64String(myByte);
            return myBase64;
        }



        public string CalcularSumaDecimalStrings(List<string> valores)
        {
            string result = "";
            decimal suma = 0;

            foreach (string valor in valores)
            {
                suma = suma + decimal.Parse(valor, System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
            }

            result = suma.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
            return (result);
        }



        public string[] ConvertirEstructuraContableDTO_To_String(List<FicheroExportacionDTO> exportFile, string separador)
        {
            List<string> result = new List<string>();
            string linea = "";
            try
            {
                int num_linea = 0;
                foreach (FicheroExportacionDTO line in exportFile)
                {
                    linea = NetejarCaractersCSV(line.Campo_1) + separador +
                            NetejarCaractersCSV(line.Campo_2) + separador +
                            NetejarCaractersCSV(line.Campo_3) + separador +
                            NetejarCaractersCSV(line.Campo_4) + separador +
                            NetejarCaractersCSV(line.Campo_5) + separador +
                            NetejarCaractersCSV(line.Campo_6) + separador +
                            NetejarCaractersCSV(line.Campo_7) + separador +
                            NetejarCaractersCSV(line.Campo_8) + separador +
                            NetejarCaractersCSV(line.Campo_9) + separador +
                            NetejarCaractersCSV(line.Campo_10) + separador +
                            NetejarCaractersCSV(line.Campo_11) + separador +
                            NetejarCaractersCSV(line.Campo_12) + separador +
                            NetejarCaractersCSV(line.Campo_13) + separador +
                            NetejarCaractersCSV(line.Campo_14) + separador +
                            NetejarCaractersCSV(line.Campo_15) + separador +
                            NetejarCaractersCSV(line.Campo_16) + separador +
                            NetejarCaractersCSV(line.Campo_17) + separador +
                            NetejarCaractersCSV(line.Campo_18) + separador +
                            NetejarCaractersCSV(line.Campo_19) + separador +
                            NetejarCaractersCSV(line.Campo_20) + separador +
                            NetejarCaractersCSV(line.Campo_21) + separador +
                            NetejarCaractersCSV(line.Campo_22) + separador +
                            NetejarCaractersCSV(line.Campo_23) + separador +
                            NetejarCaractersCSV(line.Campo_24) + separador +
                            NetejarCaractersCSV(line.Campo_25) + separador +
                            NetejarCaractersCSV(line.Campo_26) + separador +
                            NetejarCaractersCSV(line.Campo_27) + separador +
                            NetejarCaractersCSV(line.Campo_28) + separador +
                            NetejarCaractersCSV(line.Campo_29) + separador +
                            NetejarCaractersCSV(line.Campo_30) + separador +
                            NetejarCaractersCSV(line.Campo_31) + separador +
                            NetejarCaractersCSV(line.Campo_32) + separador +
                            NetejarCaractersCSV(line.Campo_33) + separador +
                            NetejarCaractersCSV(line.Campo_34) + separador +
                            NetejarCaractersCSV(line.Campo_35) + separador +
                            NetejarCaractersCSV(line.Campo_36) + separador +
                            NetejarCaractersCSV(line.Campo_37) + separador +
                            NetejarCaractersCSV(line.Campo_38) + separador +
                            NetejarCaractersCSV(line.Campo_39) + separador +
                            NetejarCaractersCSV(line.Campo_40) + separador +
                            NetejarCaractersCSV(line.Campo_41) + separador +
                            NetejarCaractersCSV(line.Campo_42) + separador +
                            NetejarCaractersCSV(line.Campo_43) + separador +
                            NetejarCaractersCSV(line.Campo_44) + separador +
                            NetejarCaractersCSV(line.Campo_45) + separador +
                            NetejarCaractersCSV(line.Campo_46) + separador +
                            NetejarCaractersCSV(line.Campo_47) + separador +
                            NetejarCaractersCSV(line.Campo_48) + separador +
                            NetejarCaractersCSV(line.Campo_49) + separador +
                            NetejarCaractersCSV(line.Campo_50) + separador +
                            NetejarCaractersCSV(line.Campo_51) + separador +
                            NetejarCaractersCSV(line.Campo_52) + separador +
                            NetejarCaractersCSV(line.Campo_53) + separador +
                            NetejarCaractersCSV(line.Campo_54) + separador +
                            NetejarCaractersCSV(line.Campo_55) + separador +
                            NetejarCaractersCSV(line.Campo_56) + separador +
                            NetejarCaractersCSV(line.Campo_57) + separador +
                            NetejarCaractersCSV(line.Campo_58) + separador +
                            NetejarCaractersCSV(line.Campo_59) + separador +
                            NetejarCaractersCSV(line.Campo_60) + separador +
                            NetejarCaractersCSV(line.Campo_61) + separador +
                            NetejarCaractersCSV(line.Campo_62) + separador +
                            NetejarCaractersCSV(line.Campo_63) + separador +
                            NetejarCaractersCSV(line.Campo_64) + separador +
                            NetejarCaractersCSV(line.Campo_65) + separador +
                            NetejarCaractersCSV(line.Campo_66);

                    if (!String.IsNullOrEmpty(linea))
                    {
                        result.Add(linea.Replace("\r\n", "").Replace("\n", "").Replace("\r", ""));
                        num_linea++;
                    }
                }
                return (result.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("---> PROBLEMA AL CONVERTIR ESTRUCTURA A STRING");
                Log.Error(ex.Message, ex);
                return (null);
            }
        }



        public string[] ConvertirEstructuraContableDTO_To_String(List<FicheroExportacionDTO> exportFile, string separador, int num_campos)
        {
            List<string> result = new List<string>();
            string linea = "";
            try
            {
                int num_linea = 0;
                foreach (FicheroExportacionDTO line in exportFile)
                {
                    if (num_campos > 0)
                        linea = NetejarCaractersCSV(line.Campo_1);
                    if (num_campos > 1)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_2);
                    if (num_campos > 2)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_3);
                    if (num_campos > 3)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_4);
                    if (num_campos > 4)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_5);
                    if (num_campos > 5)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_6);
                    if (num_campos > 6)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_7);
                    if (num_campos > 7)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_8);
                    if (num_campos > 8)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_9);
                    if (num_campos > 9)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_10);
                    if (num_campos > 10)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_11);
                    if (num_campos > 11)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_12);
                    if (num_campos > 12)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_13);
                    if (num_campos > 13)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_14);
                    if (num_campos > 14)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_15);
                    if (num_campos > 15)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_16);
                    if (num_campos > 16)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_17);
                    if (num_campos > 17)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_18);
                    if (num_campos > 18)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_19);
                    if (num_campos > 19)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_20);
                    if (num_campos > 20)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_21);
                    if (num_campos > 21)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_22);
                    if (num_campos > 22)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_23);
                    if (num_campos > 23)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_24);
                    if (num_campos > 24)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_25);
                    if (num_campos > 25)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_26);
                    if (num_campos > 26)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_27);
                    if (num_campos > 27)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_28);
                    if (num_campos > 28)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_29);
                    if (num_campos > 29)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_30);
                    if (num_campos > 30)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_31);
                    if (num_campos > 31)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_32);
                    if (num_campos > 32)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_33);
                    if (num_campos > 33)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_34);
                    if (num_campos > 34)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_35);
                    if (num_campos > 35)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_36);
                    if (num_campos > 36)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_37);
                    if (num_campos > 37)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_38);
                    if (num_campos > 38)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_39);
                    if (num_campos > 39)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_40);
                    if (num_campos > 40)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_41);
                    if (num_campos > 41)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_42);
                    if (num_campos > 42)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_43);
                    if (num_campos > 43)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_44);
                    if (num_campos > 44)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_45);
                    if (num_campos > 45)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_46);
                    if (num_campos > 46)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_47);
                    if (num_campos > 47)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_48);
                    if (num_campos > 48)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_49);
                    if (num_campos > 49)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_50);
                    if (num_campos > 50)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_51);
                    if (num_campos > 51)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_52);
                    if (num_campos > 52)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_53);
                    if (num_campos > 53)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_54);
                    if (num_campos > 54)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_55);
                    if (num_campos > 55)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_56);
                    if (num_campos > 56)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_57);
                    if (num_campos > 57)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_58);
                    if (num_campos > 58)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_59);
                    if (num_campos > 59)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_60);
                    if (num_campos > 60)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_61);
                    if (num_campos > 61)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_62);
                    if (num_campos > 62)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_63);
                    if (num_campos > 63)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_64);
                    if (num_campos > 64)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_65);
                    if (num_campos > 65)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_66);
                    if (num_campos > 66)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_67);
                    if (num_campos > 67)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_68);
                    if (num_campos > 68)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_69);
                    if (num_campos > 69)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_70);
                    if (num_campos > 70)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_71);
                    if (num_campos > 71)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_72);
                    if (num_campos > 72)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_73);
                    if (num_campos > 73)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_74);
                    if (num_campos > 74)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_75);
                    if (num_campos > 75)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_76);
                    if (num_campos > 76)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_77);
                    if (num_campos > 77)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_78);
                    if (num_campos > 78)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_79);
                    if (num_campos > 79)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_80);
                    if (num_campos > 80)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_81);
                    if (num_campos > 81)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_82);
                    if (num_campos > 82)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_83);
                    if (num_campos > 83)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_84);
                    if (num_campos > 84)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_85);
                    if (num_campos > 85)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_86);
                    if (num_campos > 86)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_87);
                    if (num_campos > 87)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_88);
                    if (num_campos > 88)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_89);
                    if (num_campos > 89)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_90);
                    if (num_campos > 90)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_91);
                    if (num_campos > 91)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_92);
                    if (num_campos > 92)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_93);
                    if (num_campos > 93)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_94);
                    if (num_campos > 94)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_95);
                    if (num_campos > 95)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_96);
                    if (num_campos > 96)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_97);
                    if (num_campos > 97)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_98);
                    if (num_campos > 98)
                        linea = linea + separador + NetejarCaractersCSV(line.Campo_99);

                    if (!String.IsNullOrEmpty(linea))
                    {
                        result.Add(linea.Replace("\r\n", "").Replace("\n", "").Replace("\r", ""));
                        num_linea++;
                    }
                }
                return (result.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("---> PROBLEMA AL CONVERTIR ESTRUCTURA A STRING");
                Log.Error(ex.Message, ex);
                return (null);
            }
        }



        public string NetejarCaractersCSV(string s)
        {
            string res = s;

            if (s.Contains(';') && s.Contains('&'))
            {
                res = System.Web.HttpUtility.HtmlDecode(s);
            }
            if (res.Contains(";"))
            {
                //res = res.Replace(";", " ");
                res = res.Replace("\"", "\"\"");
                res = "\"" + res + "\"";
            }
            return (res);
        }


        public static string NetejarSaltsDeLinea(string s)
        {
            return (s.Replace("\r\n", "").Replace("\n", "").Replace("\r", ""));
        }


        // 
        // Funció per saber si una cadena es igual a alguna d'una llista de cadenes
        public static bool CompararCadenes(string cadena, string[] cadenesAComparar, bool caseSensitive)
        {
            bool res = false;

            foreach (string str in cadenesAComparar)
            {
                if ((caseSensitive && cadena.Equals(str)) || (!caseSensitive && cadena.ToLower().Equals(str.ToLower())))
                {
                    res = true;
                    break;
                }
            }

            return (res);
        }


        // 
        // Funció per retallar una cadena a un tamany màxim
        public static string RetallarCadena(int num_caracters, string cadena)
        {
            return (new string(cadena.Take(num_caracters).ToArray()));
        }



        public static int CalcularNumeroColumnes(List<FicheroExportacionDTO> ficheroExportacion)
        {
            List<int> valors = new List<int>();
            foreach (FicheroExportacionDTO registre in ficheroExportacion)
            {
                //if (int.TryParse(registre.NumeroColumnes, out int valor))
                if (registre.NumeroColumnes > 0)
                {
                    valors.Add(registre.NumeroColumnes);
                }
            }
            if (valors.Count() > 0)
            {
                return (valors.Max());
            }
            else
            {
                return (89);
            }
        }


    }
}
