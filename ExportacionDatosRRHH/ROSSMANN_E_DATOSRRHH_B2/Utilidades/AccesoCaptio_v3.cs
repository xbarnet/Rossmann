using CaptioB2it.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace CaptioB2it.Utilidades
{
    public class AccesoCaptio_v3
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int tiempo_espera_reintento = 60000;

        string captioUrl;
        string captioUrlToken;
        string grant_type;
        string scope;
        string client_id;
        string client_secret;
        string customerKey;
        int pageSize;
        int num_reintentos;
        AuthDTO auth;
        Boolean captioLogAPI = false;
        DateTime captioTokenDateTime;
        string resposta_auth = "";
        List<Ficheros.LogErroresDTO> Errores;
        int max_registres = 250;


        public AccesoCaptio_v3(string UrlToken, string GrantType, string Scope, string ClientId, string ClientSecret, string CaptioUrl, string CustomerKey, int PageSize, bool CaptioLogAPI, int Retries)
        {
            this.captioUrl = CaptioUrl;
            this.captioUrlToken = UrlToken;
            this.grant_type = GrantType;
            this.scope = Scope;
            this.client_id = ClientId;
            this.client_secret = ClientSecret;
            this.customerKey = CustomerKey;
            this.pageSize = PageSize;
            this.num_reintentos = Retries;
            this.captioLogAPI = CaptioLogAPI;
            this.auth = GetToken_v3(ref this.resposta_auth);
            this.captioTokenDateTime = DateTime.Now;
            this.Errores = new List<Ficheros.LogErroresDTO>();
        }


        public void CambiarAccesoCaptio_v3(string UrlToken, string GrantType, string Scope, string ClientId, string ClientSecret, string CaptioUrl, string CustomerKey, int PageSize, bool CaptioLogAPI, int Retries)
        {
            this.captioUrl = CaptioUrl;
            this.captioUrlToken = UrlToken;
            this.grant_type = GrantType;
            this.scope = Scope;
            this.client_id = ClientId;
            this.client_secret = ClientSecret;
            this.customerKey = CustomerKey;
            this.pageSize = PageSize;
            this.num_reintentos = Retries;
            this.captioLogAPI = CaptioLogAPI;
            this.auth = GetToken_v3(ref this.resposta_auth);
            this.captioTokenDateTime = DateTime.Now;
            //this.Errores = new List<GestionLogError.LogErroresDTO>();
        }


        public List<Ficheros.LogErroresDTO> ObtenerErrores()
        {
            return (this.Errores);
        }


        public void EliminarErrores()
        {
            this.Errores.Clear();
        }


        private void CompruebaToken()
        {
            TimeSpan duracion = DateTime.Now - this.captioTokenDateTime;
            if (duracion.TotalSeconds > 3000)
            {
                this.auth = GetToken_v3(ref this.resposta_auth);
                this.captioTokenDateTime = DateTime.Now;
            }
        }


        private bool CompruebaSiReintentoRespuesta(HttpWebResponse response, ref int reintento)
        {
            bool result = false;
            try
            {
                int resposta = Int32.Parse(((int)response.StatusCode).ToString().First() + "00");
                switch (resposta)
                {
                    case 100:
                        // repostes informatives
                        reintento = this.num_reintentos;
                        break;
                    case 200:
                        // repostes correctes
                        reintento = this.num_reintentos;
                        break;
                    case 300:
                        // repostes redireccions
                        reintento = this.num_reintentos;
                        break;
                    case 400:
                        // errors de client
                        reintento = this.num_reintentos;
                        break;
                    case 500:
                        // errors de servidor
                        reintento++;
                        // Log
                        Log.Error(" --> Intento : " + reintento.ToString());
                        result = true;
                        break;
                    default:
                        reintento++;
                        // Log
                        Log.Error(" --> Intento : " + reintento.ToString());
                        result = true;
                        break;
                }
            }
            catch
            {
                reintento++;
                Log.Error(" --> Intento : " + reintento.ToString());
                result = true;
            }
            return (result);
        }


        // 
        // GET /api/v3.1/Accountings/Banks
        // Obtindre el id del banc.  Retorna null si no existeix
        public string GetIdBank_v3_1(string nom_banc, string filters)
        {
            AccountingsBanksDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Accountings/Banks";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AccountingsBanksDTO_v3_1[]>(requestResult);
                    List<AccountingsBanksDTO_v3_1> llista_resultat = new List<AccountingsBanksDTO_v3_1>();
                    llista_resultat.AddRange(Result);
                    try
                    {
                        return (llista_resultat.Find(x => x.Name.TrimEnd(' ') == nom_banc.TrimEnd(' ')).Id);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //
        // GET /api/v3.1/Accountings/Banks
        // Obtiene las entidades bancarias
        public AccountingsBanksDTO_v3_1[] GetAccountingsBanks_v3_1(string filters)
        {
            AccountingsBanksDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Accountings/Banks";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AccountingsBanksDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AccountingsBanksDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Accountings/Banks
        // Incorpora un Accounting Bank
        public string PostAccountingsBanks_v3_1(string Name)
        {
            AccountingsBanksDTO_v3_1_POST registro = new AccountingsBanksDTO_v3_1_POST();
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Accountings/Banks";
                registro.Name = Name;
                var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                ResponseDTO_v3_1_POST[] respuestaPost = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                return (respuestaPost[0].Result.Id);
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Accountings/Payments
        // Obtiene los pagos registrados de la conciliación
        public AccountingsPaymentsDTO_v3_1[] GetAccountingsPayments_v3_1(string filters)
        {
            AccountingsPaymentsDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Accountings/Payments";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AccountingsPaymentsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AccountingsPaymentsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Accountings/Payments
        // Incorpora els Accountings Payments (moviments bancaris per la concil·liació)
        public string PostAccountingsPaymnents_v3_1(string payments)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Accountings/Payments";
                //var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(payments));
                var requestResult = PostURLWeb(webApiUrl.ToString(), payments.Substring(1, payments.Length - 2), ref Web_Response);
                return (requestResult);
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Accountings/Payments
        // Incorpora moviments bancaris a Captio
        // Retorna el "ID" del moviment insertat si només s'envia 1 moviment
        // Retorna "00" si s'envien varis moviment i s'insertant correctament tots, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap moviment
        // Retorna null si error
        public string PostAccountingsPaymnents_v3_1(AccountingsPaymentsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Accountings/Payments";
                List<AccountingsPaymentsDTO_v3_1_POST> registrosToProcess = new List<AccountingsPaymentsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (AccountingsPaymentsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }

        //
        // GET /api/v3.1/Advances
        // Obtiene los Advances de Captio
        public AdvancesDTO_v3_1[] GetAdvances_v3_1(string filters)
        {
            AdvancesDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Advances";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AdvancesDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AdvancesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Advances
        // Crea Anticipos en Captio
        // Retorna el "ID" del moviment insertat si només s'envia 1 moviment
        // Retorna "00" si s'envien varis moviment i s'insertant correctament tots, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap moviment
        // Retorna null si error
        public string PostAdvances_v3_1(AdvancesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Advances";
                List<AdvancesDTO_v3_1_POST> registrosToProcess = new List<AdvancesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (AdvancesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Advances/Approve
        // Solicita aprovació de Advances a Captio
        // Retorna "[]" (llista buida) si s'han aprovat tots els registres
        // Retorna la llista dels id's erronis
        // Retorna null en cas d'error
        public List<string> PutAdvancesApprove_v3_1(AdvancesApproveDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Advances/Approve";
                var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registros), ref Web_Response);
                if (Web_Response == "200")
                //if (requestResult == "null")
                {
                    return (JsonConvert.DeserializeObject<string[]>("[]").ToList());
                }
                else if (Web_Response.Split("-")[0] == "400")
                {
                    // Llegir els Id dels tickets que donen problemes
                    Error_400_DTO_v3_1[] estructuraError = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Substring(Web_Response.IndexOf('#') + 1));
                    foreach (Error_400_DTO_v3_1 error in estructuraError)
                    {
                        tickets_Errors.Add(error.Value);
                    }
                    return (tickets_Errors);
                }
                return (null);
            }
            else
            {
                return (null);
            }
        }


        //
        // GET /api/v3.1/AssignmentObjects
        // Obtiene los ejes analíticos definidos
        public AssignmentObjectsDTO_v3_1[] GetAssignmentObjects_v3_1(string filters)
        {
            AssignmentObjectsDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AssignmentObjectsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AssignmentObjectsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/AssignmentObjects
        // Incorpora un Eje Análitico
        // Retorna el id del AssignmentObject o null en cas d'error
        public string PostAssignmentObjects_v3_1(string Name, int RelationType, int Type)
        {
            AssignmentObjectsDTO_v3_1_POST registro = new AssignmentObjectsDTO_v3_1_POST();
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects";
                registro.Name = Name;
                registro.RelationType = RelationType.ToString();
                registro.Type = Type.ToString();
                var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                if (Web_Response == "200")
                {
                    ResponseDTO_v3_1_POST[] res;
                    try
                    {
                        res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                    }
                    catch
                    {
                        res = null;
                    }
                    if ((res != null) && (res.Count() == 1))
                    {
                        return (res[0].Result.Id);
                    }
                    else
                    {
                        return (null);
                    }

                }
            }
            return null;
        }


        // 
        // DELETE /api/v3.1/AssignmentObjects
        // Elimina Ejes analítos de Captio
        // Retorna "00" si s'esborren totes els moviments correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap moviment
        // Retorna null si error
        public string DeleteAssignmentObjects_v3_1(AssignmentObjectsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects";
                List<AssignmentObjectsDTO_v3_1_DELETE> registrosToProcess = new List<AssignmentObjectsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/AssignmentObjects/Values
        // Obtiene los ejes analíticos definidos
        public AssignmentObjectsValuesDTO_v3_1[] GetAssignmentObjectsValues_v3_1(string filters)
        {
            AssignmentObjectsValuesDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/Values";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AssignmentObjectsValuesDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AssignmentObjectsValuesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/AssignmentObjects/Values
        // Incorpora un valor a la tabla de un Eje Análitico y devuelve el Id asignado o null en caso de error
        public string PostAssignmentObjectsValues_v3_1(string Id_EjeAnalitico, string Code, string Value)
        {
            AssignmentObjectsValuesDTO_v3_1_POST registro = new AssignmentObjectsValuesDTO_v3_1_POST();
            AssignmentObjectsValuesDTO_v3_1_POST_Values registro_value = new AssignmentObjectsValuesDTO_v3_1_POST_Values();
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/Values";
                registro.Id = Id_EjeAnalitico;
                registro_value.Code = Code;
                registro_value.Value = Value;
                registro.Values = new AssignmentObjectsValuesDTO_v3_1_POST_Values[] { registro_value };
                var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                if (Web_Response == "200")
                {
                    try
                    {
                        ResponseDTO_v3_1_POST[] Result = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                        if (Result[0].Result != null)
                        {
                            return (Result[0].Result.Id.ToString());
                        }
                    }
                    catch
                    {
                        return (null);
                    }
                }
            }
            return null;
        }


        // 
        // POST /api/v3.1/AssignmentObjects/Values
        // Incorpora un valores a la tabla de un Eje Análitico
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostAssignmentObjectsValues_v3_1(AssignmentObjectsValuesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/Values";
                List<AssignmentObjectsValuesDTO_v3_1_POST> registrosToProcess = new List<AssignmentObjectsValuesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                                ResponseDTO_v3_1_POST[] res;
                                try
                                {
                                    res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                                }
                                catch
                                {
                                    res = null;
                                }
                                if ((res != null) && (res.Count() == 1) && (result == null))
                                {
                                    result = (res[0].Result.Id);
                                }
                                else
                                {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // DELETE /api/v3.1/AssignmentObjects/Values
        // Elimina el valor de la tabla de un Eje Analítico
        // Id_EjeAnalitico -> Id del eje analítico, Value_Id -> Id del AssignmentObjectValue,  Value_User_Id -> Id del usuario que se eliminará el AssignmentObjectValue
        // Retorna "00" si s'esborra correctament
        // Retorna null si error
        public string DeleteAssignmentObjectsValues_v3_1(string Id_EjeAnalitico, string Value_Id)
        {
            AssignmentObjectsValuesDTO_v3_1_DELETE registro = new AssignmentObjectsValuesDTO_v3_1_DELETE();
            AssignmentObjectsValuesDTO_v3_1_DELETE_Values registro_value = new AssignmentObjectsValuesDTO_v3_1_DELETE_Values();
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/Values";
                registro_value.Id = Value_Id;

                string json = "";
                json = "[" + JsonConvert.SerializeObject(registro_value) + "]";

                registro.Id = Id_EjeAnalitico;
                registro.Values = JsonConvert.DeserializeObject<AssignmentObjectsValuesDTO_v3_1_DELETE_Values[]>(json);
                var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                if (Web_Response == "200")
                //if (requestResult == "null")
                {
                    return ("00");
                }
            }
            return null;
        }


        // 
        // DELETE /api/v3.1/AssignmentObjects/Values
        // Elimina Ejes analítos de Captio
        // Retorna "00" si s'esborren totes els moviments correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap moviment
        // Retorna null si error
        public string DeleteAssignmentObjectsValues_v3_1(AssignmentObjectsValuesDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/Values";
                List<AssignmentObjectsValuesDTO_v3_1_DELETE> registrosToProcess = new List<AssignmentObjectsValuesDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        //
        // GET /api/v3.1/AssignmentObjects/ValuesUsers
        // Obtiene los ejes analíticos definidos asignados a cada usuario
        public AssignmentObjectsValuesUsersDTO_v3_1[] GetAssignmentObjectsValuesUsers_v3_1(string filters)
        {
            AssignmentObjectsValuesUsersDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/ValuesUsers";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<AssignmentObjectsValuesUsersDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<AssignmentObjectsValuesUsersDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/AssignmentObjects/ValuesUsers
        // Asigna a una lista de usuarios un valor de la tabla de Valores de un Eje Análitico
        // Id_EjeAnalitico -> Id del eje analítico, Value_Id -> Id del AssignmentObjectValue,  Value_User_Id -> Id del usuario que visualiza el AssignmentObjectValue
        // Retorna "00" si s'assignen tots els usuaris correctament, "0-" si només s'assignen a alguns usuaris correctament i "--" si no s'assigna a cap usuari
        // Retorna null si error
        public string PostAssignmentObjectsValuesUsers_v3_1(string Id_EjeAnalitico, string Value_Id, List<AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users> Value_Users)
        {
            AssignmentObjectsValuesUsersDTO_v3_1_POST registro = new AssignmentObjectsValuesUsersDTO_v3_1_POST();
            AssignmentObjectsValuesUsersDTO_v3_1_POST_Values registro_value = new AssignmentObjectsValuesUsersDTO_v3_1_POST_Values();
            AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users registro_value_user;
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/ValuesUsers";
                registro.Id = Id_EjeAnalitico;
                registro_value.Id = Value_Id;
                List<AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users> registrosToProcess = new List<AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users registro_Value_Users in Value_Users)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro_Value_Users);
                        i++;
                    }
                    if ((i == max_registres) || (registro_Value_Users.Equals(Value_Users.Last())))
                    {
                        string json = "";
                        foreach (AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users User_Id in registrosToProcess)
                        {
                            registro_value_user = new AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users
                            {
                                Id = User_Id.Id
                            };
                            json = json + "," + JsonConvert.SerializeObject(registro_value_user);
                        }
                        json = "[" + json.Substring(1) + "]";
                        registro_value.Users = JsonConvert.DeserializeObject<AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users[]>(json);
                        registro.Values = new AssignmentObjectsValuesUsersDTO_v3_1_POST_Values[] { registro_value };
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/AssignmentObjects/ValuesUsers
        // Asigna a una lista de usuarios un valor de la tabla de Valores de un Eje Análitico
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostAssignmentObjectsValuesUsers_v3_1(AssignmentObjectsValuesUsersDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/ValuesUsers";
                List<AssignmentObjectsValuesUsersDTO_v3_1_POST> registrosToProcess = new List<AssignmentObjectsValuesUsersDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesUsersDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }

            return (null);
        }



        // 
        // DELETE /api/v3.1/AssignmentObjects/ValuesUsers
        // Elimina una asignación de la tabla de un Eje Análitico
        // Id_EjeAnalitico -> Id del eje analítico, Value_Id -> Id del AssignmentObjectValue,  Value_User_Id -> Id del usuario que se eliminará el AssignmentObjectValue
        // Retorna "00" si s'esborren totes les assignacions a usuaris correctament, "0-" si només s'eliminem algunes correctament i "--" si no s'elimina cap assignació als usuaris
        // Retorna null si error
        public string DeleteAssignmentObjectsValuesUsers_v3_1(string Id_EjeAnalitico, string Value_Id, List<AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users> Value_Users)
        {
            AssignmentObjectsValuesUsersDTO_v3_1_DELETE registro = new AssignmentObjectsValuesUsersDTO_v3_1_DELETE();
            AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values registro_value = new AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values();
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/ValuesUsers";
                List<AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users> registrosToProcess = new List<AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users registro_Value_Users in Value_Users)
                {
                    if (i < max_registres)
                    {
                        //registrosToProcess.Add(registro_Value_Users);
                        registrosToProcess.Add(new AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users() { Id = registro_Value_Users.Id });
                        i++;
                    }
                    if ((i == max_registres) || (registro_Value_Users.Equals(Value_Users.Last())))
                    {
                        registro.Id = Id_EjeAnalitico;
                        registro_value.Id = Value_Id;
                        string json = "";
                        json = JsonConvert.SerializeObject(registrosToProcess);
                        registro_value.Users = JsonConvert.DeserializeObject<AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users[]>(json);
                        registro.Values = new AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values[] { registro_value };
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                        //var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // DELETE /api/v3.1/AssignmentObjects/ValuesUsers
        // Elimina Ejes analítos de Captio
        // Retorna "00" si s'esborren totes els moviments correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap moviment
        // Retorna null si error
        public string DeleteAssignmentObjectsValuesUsers_v3_1(AssignmentObjectsValuesUsersDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/AssignmentObjects/ValuesUsers";
                List<AssignmentObjectsValuesUsersDTO_v3_1_DELETE> registrosToProcess = new List<AssignmentObjectsValuesUsersDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (AssignmentObjectsValuesUsersDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }




        //
        // GET /api/v3.1/Companies/Vats
        // Obtiene los tipos de IVA definidas en la configuración de Captio
        public CompaniesVatsDTO_v3_1[] GetCompaniesVats_v3_1(string filters)
        {
            CompaniesVatsDTO_v3_1[] CompaniesVatsResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Vats";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    CompaniesVatsResult = JsonConvert.DeserializeObject<CompaniesVatsDTO_v3_1[]>(requestResult);
                    return (CompaniesVatsResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesVatsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/Rules
        // Obtiene las reglas creadas para el entorno definidos en la configuración de Captio
        public CompaniesRulesDTO_v3_1[] GetCompaniesRules_v3_1(string filters)
        {
            CompaniesRulesDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Rules";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesRulesDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesRulesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/Alerts
        // Obtiene los avisos configurados en el cliente en la configuración de Captio
        public CompaniesAlertsDTO_v3_1[] GetCompaniesAlerts_v3_1(string filters)
        {
            CompaniesAlertsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Groups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesAlertsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesAlertsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/Groups
        // Obtiene los Grupos definidos en la configuración de Captio
        public CompaniesGroupsDTO_v3_1[] GetCompaniesGroups_v3_1(string filters)
        {
            CompaniesGroupsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Groups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesGroupsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesGroupsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/Currency
        // Obtiene las monedas definidas en la configuración de Captio
        public CompaniesCurrencyDTO_v3_1[] GetCompaniesCurrency_v3_1(string filters)
        {
            CompaniesCurrencyDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Currency";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesCurrencyDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesCurrencyDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/KmGroups
        // Obtiene los Grupos de Kilometraje definidos en la configuración de Captio
        public CompaniesKmGroupsDTO_v3_1[] GetCompaniesKmGroups_v3_1(string filters)
        {
            CompaniesKmGroupsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/KmGroups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesKmGroupsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesKmGroupsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/Payments
        // Obtiene los Payments definidos como métodos de pago
        public CompaniesPaymentsDTO_v3_1[] GetCompaniesPayments_v3_1(string filters)
        {
            CompaniesPaymentsDTO_v3_1[] UserResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Payments";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    UserResult = JsonConvert.DeserializeObject<CompaniesPaymentsDTO_v3_1[]>(requestResult);
                    return (UserResult);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        //
        //GET /api/v3.1/Companies/Categories
        //Obtiene las categorías definidas en la configuración de Captio
        public CompaniesCategoriesDTO_v3_1[] GetCompaniesCategories_v3_1(string filters)
        {
            CompaniesCategoriesDTO_v3_1[] CompaniesCategoryResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Categories";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    CompaniesCategoryResult = JsonConvert.DeserializeObject<CompaniesCategoriesDTO_v3_1[]>(requestResult);
                    return (CompaniesCategoryResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesCategoriesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Companies/Categories
        // Elimina Categorías y sus subcategorías definidas en la configuración de Captio
        // Retorna "00" si s'esborren totes les categoríes correctament, "0-" si només s'eliminem algunes correctament i "--" si no s'elimina cap categoría
        // Retorna null si error
        public string DeleteCompaniesCategories_v3_1(CompaniesCategoriesDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Companies/Categories";
                List<CompaniesCategoriesDTO_v3_1_DELETE> registrosToProcess = new List<CompaniesCategoriesDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (CompaniesCategoriesDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Companies/Categories
        // Actualiza los campos de una categoría o subcategoría definida en la configuración de Captio
        // Retorna "00" si s'esborren totes les categoríes correctament, "0-" si només s'eliminem algunes correctament i "--" si no s'elimina cap categoría
        // Retorna null si error
        public string PatchCompaniesCategories_v3_1(CompaniesCategoriesDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Companies/Categories";
                List<CompaniesCategoriesDTO_v3_1_PATCH> registrosToProcess = new List<CompaniesCategoriesDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (CompaniesCategoriesDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                ResponseDTO_v3_1_POST[] res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Companies/Categories
        // Crea Categorías (incluyendo subcategorías si fuera necesario) en Captio
        // Retorna el "ID" de la categoría insertada si només s'envia 1 categoría
        // Retorna "00" si s'envien vàries categorías i s'insertant correctament totes, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap categoría
        // Retorna null si error
        public string PostCompaniesCategories_v3_1(CompaniesCategoriesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Companies/Categories";
                List<CompaniesCategoriesDTO_v3_1_POST> registrosToProcess = new List<CompaniesCategoriesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (CompaniesCategoriesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Companies/Groups/Users
        // Obtiene los usuarios asignados a los grupos de usuarios definidos en la configuración de Captio
        public CompaniesGroupsUsersDTO_v3_1[] GetCompaniesGroupsUsers_v3_1(string filters)
        {
            CompaniesGroupsUsersDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Groups/Users";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesGroupsUsersDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesGroupsUsersDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/UserCustomFields
        // Obtiene los Custom Fields Users definidos en la configuración de Captio
        public CompaniesUserCustomFieldsDTO_v3_1[] GetCompaniesUserCustomFields_v3_1(string filters)
        {
            CompaniesUserCustomFieldsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/UserCustomFields";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesUserCustomFieldsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesUserCustomFieldsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PUT /api/v3.1/Companies/UserCustomFields
        // Actualiza o crea los campos personalizados a todos los usuarios de una definidos en la configuración de Captio
        // Retorna "00" si se actulizan/crean todos los campos personalizados de usuario correctamente, "0-" si solo se crean/actualizan algunos correctamente y "--" si no se crean/actualiza ningún campo personalizado de usuario
        // Retorna null si error
        public string PutCompaniesUserCustomFields_v3_1(CompaniesUserCustomFieldsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Companies/UserCustomFields";
                List<CompaniesUserCustomFieldsDTO_v3_1_PUT> registrosToProcess = new List<CompaniesUserCustomFieldsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (CompaniesUserCustomFieldsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Companies/Categories/Groups
        // Obtiene los grupos de las categorías definidas en la configuración de Captio
        public CompaniesCategoriesGroupsDTO_v3_1[] GetCompaniesCategoriesGroups_v3_1(string filters)
        {
            CompaniesCategoriesGroupsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/Categories/Groups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesCategoriesGroupsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesCategoriesGroupsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Companies/VatIdentificationNumber
        // Obtiene las sociedades dada de alta en la configuración de Captio
        public CompaniesVatIdentificationNumberDTO_v3_1[] GetCompaniesVatIdentificationNumber_v3_1(string filters)
        {
            CompaniesVatIdentificationNumberDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Companies/VatIdentificationNumber";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CompaniesVatIdentificationNumberDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CompaniesVatIdentificationNumberDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Companies/SubCategories
        // Crea Subcategorías dentro de una categoria padre en Captio
        // Retorna el "ID" de la categoría insertada si només s'envia 1 categoría
        // Retorna "00" si s'envien vàries categorías i s'insertant correctament totes, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap categoría
        // Retorna null si error
        public string PostCompaniesSubcategories_v3_1(CompaniesSubCategoriesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Companies/SubCategories";
                List<CompaniesSubCategoriesDTO_v3_1_POST> registrosToProcess = new List<CompaniesSubCategoriesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (CompaniesSubCategoriesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/CustomFields
        // Obtiene los Custom Fields definidos en Captio
        public CustomFieldsDTO_v3_1[] GetCustomFields_v3_1(string filters)
        {
            CustomFieldsDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/CustomFields";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CustomFieldsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CustomFieldsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/CustomFields
        // Elimina Custom Fields de Captio
        // Retorna "00" si s'esborren tots el Custom Fields correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap camp personalitzat
        // Retorna null si error
        public string DeleteCustomFields_v3_1(CustomFieldsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields";
                List<CustomFieldsDTO_v3_1_DELETE> registrosToProcess = new List<CustomFieldsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (CustomFieldsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/CustomFields
        // Crea un Custom Field a la configuración de Captio
        // Retorna el "ID" del campo personalizado insertat si només s'envia 1 custom field
        // Retorna "00" si s'envien vàris camps personalitzats i s'inserten correctament tots, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap camp personalitzat
        // Retorna null si error
        public string PostCustomFields_v3_1(CustomFieldsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields";
                List<CustomFieldsDTO_v3_1_POST> registrosToProcess = new List<CustomFieldsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (CustomFieldsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/CustomFields
        // Actualiza los datos de los campos personalizados definidos en la configuración de Captio
        // Retorna "00" si se actualizan todos los campos personalizados correctamente, "0-" si solo se actualizan algunos correctamente y "--" si no se actualiza ningún campo personalizado
        // Retorna null si error
        public string PutCustomFields_v3_1(CustomFieldsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields";
                List<CustomFieldsDTO_v3_1_PUT> registrosToProcess = new List<CustomFieldsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (CustomFieldsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/CustomFields/Items
        // Obtiene los items de los Custom Field definidos en Captio
        public CustomFieldsItemsDTO_v3_1[] GetCustomFieldsItems_v3_1(string filters)
        {
            CustomFieldsItemsDTO_v3_1[] Result;

            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/CustomFields/Items";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<CustomFieldsItemsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<CustomFieldsItemsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/CustomFields/Items
        // Elimina Items de Custom Fields de Captio
        // Retorna "00" si s'esborren tots els Items dels Custom Fields correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap item dels camps personalitzats
        // Retorna null si error
        public string DeleteCustomFieldsItems_v3_1(CustomFieldsItemsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields/Items";
                List<CustomFieldsItemsDTO_v3_1_DELETE> registrosToProcess = new List<CustomFieldsItemsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (CustomFieldsItemsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/CustomFields/Items
        // Crea un Custom Field a la configuración de Captio
        // Retorna una tabla con los "ID" de los items del campo personalizado insertado si solo s'envia 1 custom field
        // Retorna "00" si s'envien vàris items de varis camps personalitzats i s'inserten correctament tots, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap items dels camps personalitzats
        // Retorna null si error
        public string[] PostCustomFieldsItems_v3_1(CustomFieldsItemsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields/Items";
                List<CustomFieldsItemsDTO_v3_1_POST> registrosToProcess = new List<CustomFieldsItemsDTO_v3_1_POST>();
                string[] result = null;
                int i = 0;
                foreach (CustomFieldsItemsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseCustomFieldItemsDTO_v3_1_POST[] res = JsonConvert.DeserializeObject<ResponseCustomFieldItemsDTO_v3_1_POST[]>(requestResult);
                            if ((res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Items);
                            }
                            else
                            {
                                result = (result == null || result[0].Equals("00")) ? new string[1] { "00" } : new string[1] { "0-" };
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result[0].Equals("--")) ? new string[1] { "--" } : new string[1] { "0-" };
                                }
                                else if (cont > 0)
                                {
                                    result = new string[1] { "0-" };
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/CustomFields/Items
        // Actualiza los datos de los campos items de los personalizados definidos en la configuración de Captio
        // Retorna "00" si se actualizan todos los campos personalizados correctamente, "0-" si solo se actualizan algunos correctamente y "--" si no se actualiza ningún campo personalizado
        // Retorna null si error
        public string PutCustomFieldsItems_v3_1(CustomFieldsItemsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/CustomFields/Items";
                List<CustomFieldsItemsDTO_v3_1_PUT> registrosToProcess = new List<CustomFieldsItemsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (CustomFieldsItemsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Expenses
        // Obtiene los Gastos de Captio
        public ExpensesDTO_v3_1[] GetExpenses_v3_1(string filters)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ExpensesDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ExpensesDTO_v3_1[]>(requestResult);
                    // Afegir capçalera url a les UrlKey
                    foreach (ExpensesDTO_v3_1 aux in Result)
                    {
                        aux.UrlKey = (String.IsNullOrEmpty(aux.UrlKey)) ? (aux.UrlKey) : (cabecera_url + aux.UrlKey);
                        foreach (ExpensesDTO_v3_1_Attachments aux_attachment in aux.Attachments)
                        {
                            aux_attachment.UrlKey = (String.IsNullOrEmpty(aux_attachment.UrlKey)) ? (aux_attachment.UrlKey) : (cabecera_url + aux_attachment.UrlKey);
                        }
                    }
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Expenses
        // Obtiene los datos de una Expense concreta con un Id
        public ExpensesDTO_v3_1 GetExpense_v3_1(string id)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ExpensesDTO_v3_1 ExpenseResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Id" + "\"" + ":" + id).Append("}");
                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Id" + "\"" + ":" + id).Append("}");
                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Id" + "\"" + ":" + id).Append("}");
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    ExpenseResult = JsonConvert.DeserializeObject<ExpensesDTO_v3_1[]>(requestResult)[0];
                    // Afegir capçalera url a les UrlKey
                    ExpenseResult.UrlKey = (String.IsNullOrEmpty(ExpenseResult.UrlKey)) ? (ExpenseResult.UrlKey) : (cabecera_url + ExpenseResult.UrlKey);
                    foreach (ExpensesDTO_v3_1_Attachments aux_attachment in ExpenseResult.Attachments)
                    {
                        aux_attachment.UrlKey = (String.IsNullOrEmpty(aux_attachment.UrlKey)) ? (aux_attachment.UrlKey) : (cabecera_url + aux_attachment.UrlKey);
                    }
                    return (ExpenseResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesDTO_v3_1>(""));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Expenses
        // Elimina Expenses de Captio
        // Retorna "00" si s'esborren tots els Items dels Custom Fields correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap item dels camps personalitzats
        // Retorna null si error
        public string DeleteExpenses_v3_1(ExpensesDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Expenses";
                List<ExpensesDTO_v3_1_DELETE> registrosToProcess = new List<ExpensesDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (ExpensesDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Expenses/Alerts
        // Obtiene las Alerts de los Expenses de Captio
        public ExpensesAlertsDTO_v3_1[] GetExpensesAlerts_v3_1(string filters)
        {
            ExpensesAlertsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses/Alerts";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ExpensesAlertsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesAlertsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Expenses/Payments
        // Obtiene los Payments de los Expenses de Captio
        public ExpensesPaymentsDTO_v3_1[] GetExpensesPayments_v3_1(string filters)
        {
            ExpensesPaymentsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses/Payments";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ExpensesPaymentsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesPaymentsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Expenses/Incomplete
        // Obtiene los Gastos Incompletos de Captio
        public ExpensesIncompleteDTO_v3_1[] GetExpensesIncomplete_v3_1(string filters)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ExpensesIncompleteDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses/Incomplete";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ExpensesIncompleteDTO_v3_1[]>(requestResult);
                    // Afegir capçalera url a les UrlKey
                    foreach (ExpensesIncompleteDTO_v3_1 aux in Result)
                    {
                        aux.UrlKey = (String.IsNullOrEmpty(aux.UrlKey)) ? (aux.UrlKey) : (cabecera_url + aux.UrlKey);
                        foreach (ExpensesIncompleteDTO_v3_1_Attachments aux_attachment in aux.Attachments)
                        {
                            aux_attachment.UrlKey = (String.IsNullOrEmpty(aux_attachment.UrlKey)) ? (aux_attachment.UrlKey) : (cabecera_url + aux_attachment.UrlKey);
                        }
                    }
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesIncompleteDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Expenses/CertifiedUrls
        // Obtiene las Alerts de los Expenses de Captio
        public ExpensesCertifiedUrlsDTO_v3_1[] GetExpensesCertifiedUrls_v3_1(string filters)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ExpensesCertifiedUrlsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Expenses/CertifiedUrls";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ExpensesCertifiedUrlsDTO_v3_1[]>(requestResult);
                    // Afegir capçalera url a les UrlKey
                    foreach (ExpensesCertifiedUrlsDTO_v3_1 aux in Result)
                    {
                        aux.UrlKey = (String.IsNullOrEmpty(aux.UrlKey)) ? (aux.UrlKey) : (cabecera_url + aux.UrlKey);
                        foreach (ExpensesCertifiedUrlsDTO_v3_1_Attachments aux_attachment in aux.Attachments)
                        {
                            aux_attachment.UrlKey = (String.IsNullOrEmpty(aux_attachment.UrlKey)) ? (aux_attachment.UrlKey) : (cabecera_url + aux_attachment.UrlKey);
                        }
                    }
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ExpensesCertifiedUrlsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // POST /api/v3.1/Expenses/Mileage
        // Crea Gastos de kilometraje en Captio
        // Retorna el "ID" del Expense creat si només s'envia 1 ticket
        // Retorna "00" si s'envien varis tickets i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap expense
        // Retorna null si error
        public string PostExpensesMileage_v3_1(ExpensesMileageDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Expenses/Mileage";
                List<ExpensesMileageDTO_v3_1_POST> registrosToProcess = new List<ExpensesMileageDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ExpensesMileageDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Expenses/Manually
        // Crea Gastos Manuales en Captio
        // Retorna el "ID" del Expense creat si només s'envia 1 ticket
        // Retorna "00" si s'envien varis tickets i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap expense
        // Retorna null si error
        public string PostExpensesManually_v3_1(ExpensesManuallyDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Expenses/Manually";
                List<ExpensesManuallyDTO_v3_1_POST> registrosToProcess = new List<ExpensesManuallyDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ExpensesManuallyDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (requestResult != null)
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Expenses/Update
        // Actualiza la categoria y los campos personalizados de una Expense a Captio
        // Retorna "00" si s'esborren totes les categoríes correctament, "0-" si només s'eliminem algunes correctament i "--" si no s'elimina cap categoría
        // Retorna null si error
        public string PatchExpensesUpdate_v3_1(ExpensesUpdateDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Expenses/Update";
                List<ExpensesUpdateDTO_v3_1_PATCH> registrosToProcess = new List<ExpensesUpdateDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (ExpensesUpdateDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                ResponseDTO_v3_1_POST[] res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Expenses/UpdateAmount
        // Actualiza el valor del ticket de una Expense a Captio
        // Retorna "00" si s'esborren totes les categoríes correctament, "0-" si només s'eliminem algunes correctament i "--" si no s'elimina cap categoría
        // Retorna null si error
        public string PatchExpensesUpdateAmount_v3_1(ExpensesUpdateAmountDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Expenses/UpdateAmount";
                List<ExpensesUpdateAmountDTO_v3_1_PATCH> registrosToProcess = new List<ExpensesUpdateAmountDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (ExpensesUpdateAmountDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                ResponseDTO_v3_1_POST[] res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Logs  (NO FUNCIONA).  ESBORRAR
        // Enviar un archive de Logs por mail a usuarios registrados en Captio
        public string PostLogs_v3_1(LogsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Logs";
                List<LogsDTO_v3_1_POST> registrosToProcess = new List<LogsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (LogsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Logs
        // Enviar un archive de Logs por mail a usuarios registrados en Captio
        // Retorna "00" si s'ha processat correctament i null en cas d'error
        public string PostLogs_v3_1(LogsDTO_v3_1_POST registro)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Logs";
                string result = null;

                var requestResult = PostURLWeb_(webApiUrl.ToString(), JsonConvert.SerializeObject(registro), ref Web_Response);
                if (Web_Response == "200")
                {
                    result = (result == null || result.Equals("00")) ? "00" : "0-";
                }
                else
                {
                    if (Web_Response.Split("#")[0] == "400-BadRequest")
                    {
                        Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                        int cont = 0;
                        foreach (Error_400_DTO_v3_1 resposta in res)
                        {
                            if (resposta.Result == null)
                            {
                                cont++;
                            }
                        }
                        if (cont == res.Count())
                        {
                            result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                        }
                        else if (cont > 0)
                        {
                            result = "0-";
                        }
                    }
                }
                return (result);
            }
            return (null);
        }



        //
        // GET /api/v3.1/Partner
        // Obtiene los id's de las compañías y el nombre de los entornos de cada una de ellas de Captio
        public PartnerDTO_v3_1[] GetPartner_v3_1(string filters)
        {
            PartnerDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Partner";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<PartnerDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<PartnerDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // GET /api/v3.1/Reports
        // Obtiene los reports de un estado entre dos fechas
        // Formato Fecha: "yyyy-MM-ddT23:59:59.99Z"
        // Status: 1 (Borrador), 2 (Aprobación solicitada), 3 (Rechazado), 4 (Aprobado), 6 (Liquidado)
        public ReportsDTO_v3_1[] GetReports_v3_1(string Status, string StartDate, string EndDate)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ReportsDTO_v3_1[] ReportResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Reports";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Status" + "\"" + ":" + Status).Append(",").Append("\"" + "StatusDate" + "\"" + ":" + "\"" + ">" + StartDate + "," + "<=" + EndDate + "\"").Append("}");
                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Status" + "\"" + ":" + Status).Append(",").Append("\"" + "StatusDate" + "\"" + ":" + "\"" + ">=" + StartDate + "," + "<=" + EndDate + "\"").Append("}");
                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append("\"" + "Status" + "\"" + ":" + Status).Append(",").Append("\"" + "StatusDate" + "\"" + ":" + "\"" + ">=" + StartDate + "," + "<=" + EndDate + "\"").Append("}");
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    ReportResult = JsonConvert.DeserializeObject<ReportsDTO_v3_1[]>(requestResult);
                    // Afegir capçalera url a les UrlKey
                    foreach (ReportsDTO_v3_1 aux in ReportResult)
                    {
                        aux.UrlKey = (String.IsNullOrEmpty(aux.UrlKey)) ? (aux.UrlKey) : (cabecera_url + aux.UrlKey);
                    }
                    return (ReportResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ReportsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/GetReports
        // Obtiene los Reports de Captio
        public ReportsDTO_v3_1[] GetReports_v3_1(string filters)
        {
            string cabecera_url = "http://api-storage.captio.net/api/GetFile?key=";
            ReportsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Reports";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ReportsDTO_v3_1[]>(requestResult);
                    // Afegir capçalera url a les UrlKey
                    foreach (ReportsDTO_v3_1 aux in Result)
                    {
                        aux.UrlKey = (String.IsNullOrEmpty(aux.UrlKey)) ? (aux.UrlKey) : (cabecera_url + aux.UrlKey);
                    }
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ReportsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Reports
        // Elimina Reports a Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteReports_v3_1(ReportsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports";
                List<ReportsDTO_v3_1_DELETE> registrosToProcess = new List<ReportsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (ReportsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Reports
        // Actualiza el nombre del report y el workflow de los reports en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchReports_v3_1(ReportsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports";
                List<ReportsDTO_v3_1_PATCH> registrosToProcess = new List<ReportsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (ReportsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "null")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Reports
        // Crea Reports en Captio
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostReports_v3_1(ReportsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports";
                List<ReportsDTO_v3_1_POST> registrosToProcess = new List<ReportsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ReportsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                int cont = 0;
                                foreach (Error_400_DTO_v3_1 resposta in res)
                                {
                                    if (resposta.Result == null)
                                    {
                                        cont++;
                                    }
                                }
                                if (cont == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else if (cont > 0)
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // POST /api/v3.1/Report
        // Crea un Report a Captio
        // Retorna "00" si s'inserta correctament
        // Retorna el texte dels errors concatenats si dona error per falta d'algun camp personalitzable o obligatori incorrecte
        // Retorna null si error de Captio
        // Retorna en el report el Report insertat o null 
        public string PostReport_v3_1(ReportsDTO_v3_1_POST registro, ref ReportsDTO_v3_1 report)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports";
                List<ReportsDTO_v3_1_POST> registrosToProcess = new List<ReportsDTO_v3_1_POST>
                {
                    registro
                };
                string result = "";
                report = null;
                var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                if (Web_Response == "200")
                {
                    result = "00";
                    string filtre = "\"" + "Name" + "\"" + ":" + "\"" + registro.Name + "\"";
                    report = GetReports_v3_1(filtre).LastOrDefault();
                    int intento = 1;
                    while ((report == null) && (intento < num_reintentos))
                    {
                        System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                        intento++;
                        report = GetReports_v3_1(filtre).LastOrDefault();
                    }
                }
                else
                {
                    if (Web_Response.Split("#")[0] == "400-BadRequest")
                    {
                        Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                        foreach (Error_400_DTO_v3_1_Validations resposta in res.First().Validations)
                        {
                            result = result + resposta.Message;
                        }
                    }
                    report = null;
                }
                return (result);
            }
            return (null);
        }



        //
        // GET /api/v3.1/Reports/Logs
        // Obtiene los Logs de los Reports de Captio
        public ReportsLogsDTO_v3_1[] GetReportsLogs_v3_1(string filters)
        {
            ReportsLogsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Reports/Logs";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ReportsLogsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ReportsLogsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Reports/Alerts
        // Obtiene las Alerts de los Reports de Captio
        public ReportsAlertsDTO_v3_1[] GetReportsAlerts_v3_1(string filters)
        {
            ReportsAlertsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Reports/Alerts";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<ReportsAlertsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<ReportsAlertsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PUT /api/v3.1/Reports/Reject
        // Rebutjar Reports a Captio
        // Retorna "00" si se actualizan todos los campos personalizados correctamente, "0-" si solo se actualizan algunos correctamente y "--" si no se actualiza ningún campo personalizado
        // Retorna null si error
        public string PutReportsReject_v3_1(ReportsRejectDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/Reject";
                List<ReportsRejectDTO_v3_1_PUT> registrosToProcess = new List<ReportsRejectDTO_v3_1_PUT>();
                string result = null;
                foreach (ReportsRejectDTO_v3_1_PUT registro in registros)
                {
                    registrosToProcess.Clear();
                    registrosToProcess.Add(registro);
                    var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                    while (requestResult.Contains("REPORT_WORKING_ON_ALERTS"))
                    {
                        // Log
                        Log.Info("---> Esperamos 1 minuto a que finalice el proceso de gestión de alertas del informe");
                        System.Threading.Thread.Sleep(60000);  // Esperar 1 minut
                        requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                    }
                    if (Web_Response == "200")
                    //if (requestResult == "null")
                    {
                        result = (result == null || result.Equals("00")) ? "00" : "0-";
                    }
                    else
                    {
                        if (Web_Response.Split("#")[0] == "400-BadRequest")
                        {
                            Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                            if (registrosToProcess.Count() == res.Count())
                            {
                                result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                            }
                            else
                            {
                                result = "0-";
                            }
                        }
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Reports/Approve
        // Aprova Reports a Captio
        // Retorna "00" si se actualizan todos los campos personalizados correctamente, "0-" si solo se actualizan algunos correctamente y "--" si no se actualiza ningún campo personalizado
        // Retorna null si error
        public string PutReportsApprove_v3_1(ReportsApproveDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/Approve";
                List<ReportsApproveDTO_v3_1_PUT> registrosToProcess = new List<ReportsApproveDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (ReportsApproveDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "[]")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Reports/RequestApprove
        // Solicita aprovació de Reports a Captio
        // Retorna una lista con los "ID" de los tickets que no pasan las validaciones
        // L'estructura del texto de los ID de los tickets es: [ID_Ticket_erroneo]-[ID_CustomField_Erroneo]-[Mensaje error]-[ID_Report]
        // Retorna "[]" (lista vacía) si se solita la aprobación de todos los informes correctamente
        // Retorna null si error
        public List<string> PutReportsRequestApprove_v3_1(ReportsRequestApproveDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();
            List<ExpensesDTO_v3_1> expensesCaptio = new List<ExpensesDTO_v3_1>();
            List<CustomFieldsDTO_v3_1> customFieldsCaptio = new List<CustomFieldsDTO_v3_1>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/RequestApprove";
                List<ReportsRequestApproveDTO_v3_1_PUT> registrosToProcess = new List<ReportsRequestApproveDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (ReportsRequestApproveDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        //if (requestResult == "[]")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                // Llegir els Id dels tickets que donen problemes
                                Error_400_DTO_v3_1[] estructuraError = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Substring(Web_Response.IndexOf('#') + 1));
                                string[] textes;
                                string ticket_error_texte = "";
                                foreach (Error_400_DTO_v3_1 validations in estructuraError)
                                {
                                    foreach (Error_400_DTO_v3_1_Validations error in validations.Validations)
                                    {
                                        try
                                        {
                                            textes = error.Message.Split("expense ");
                                            string aux1 = textes[1].Split(" ")[0];
                                            textes = error.Message.Split("custom field ");
                                            string aux2 = textes[1].Split(" ")[0];
                                            ticket_error_texte = aux1 + "-" + aux2 + "-" + error.Code + "-" + validations.Value;
                                            if (!tickets_Errors.Exists(x => x.Equals(ticket_error_texte)))
                                                {
                                                    tickets_Errors.Add(ticket_error_texte);
                                                }
                                        }
                                        catch
                                        { }
                                        try
                                        {
                                            textes = error.Message.Split("CustomField ");
                                            if (!customFieldsCaptio.Exists(x => x.ExternalId.Trim().Equals(textes[1].Split(",")[0].Trim())))
                                            {
                                                customFieldsCaptio.AddRange(GetCustomFields_v3_1("\"" + "ExternalId" + "\"" + ":" + "\"" + textes[1].Split(",")[0].Trim() + "\""));
                                            }
                                            string aux = customFieldsCaptio.Find(x => x.ExternalId.Trim().Equals(textes[1].Split(",")[0].Trim())).Id;
                                            ticket_error_texte = "-" + aux + "-" + error.Code + "-" + validations.Value;
                                            if (!tickets_Errors.Exists(x => x.Equals(ticket_error_texte)))
                                            {
                                                tickets_Errors.Add(ticket_error_texte);
                                            }
                                        }
                                        catch
                                        { }
                                        try
                                        {
                                            textes = error.Message.Split("Expense ");
                                            if (!expensesCaptio.Exists(x => x.ExternalId.Trim().Equals(textes[1].Split(" ")[0])))
                                            {
                                                expensesCaptio.AddRange(GetExpenses_v3_1("\"" + "ExternalId" + "\"" + ":" + "\"" + textes[1].Split(" ")[0] + "\""));
                                            }
                                            string aux = expensesCaptio.Find(x => x.ExternalId.Trim().Equals(textes[1].Split(" ")[0])).Id;
                                            ticket_error_texte = aux + "-" + "-" + error.Code + "-" + validations.Value;
                                            if (!tickets_Errors.Exists(x => x.Equals(ticket_error_texte)))
                                            {
                                                tickets_Errors.Add(ticket_error_texte);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                                result = "0-";
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                if (result == "00")
                {
                    return (JsonConvert.DeserializeObject<string[]>("[]").ToList());
                }
                else
                {
                    return (tickets_Errors);
                }
            }
            return (null);
        }



        // 
        // PUT /api/v3.1/Reports/RemoveExpenses
        // Elimina expenses de Reports a Captio
        // Retorna "00" si se actualizan todos los registros correctamente, "0-" si solo se actualizan algunos correctamente y "--" si no se actualiza ningún registro
        // Retorna null si error
        public string PutReportsRemoveExpenses_v3_1(ReportsRemoveExpensesDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            List<ReportsRemoveExpensesDTO_v3_1_PUT> registrosOK = new List<ReportsRemoveExpensesDTO_v3_1_PUT>();
            foreach (ReportsRemoveExpensesDTO_v3_1_PUT registro in registros)
            {
                if (registro.Expenses.Count() > 0)
                {
                    registrosOK.Add(registro);
                }
            }
            if (registrosOK.Count == 0)
            {
                return ("--");
            }

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/RemoveExpenses";
                List<ReportsRemoveExpensesDTO_v3_1_PUT> registrosToProcess = new List<ReportsRemoveExpensesDTO_v3_1_PUT>();
                string result = null;
                if (registrosOK.Count < registros.Count())
                {
                    result = "--";
                }
                int i = 0;
                foreach (ReportsRemoveExpensesDTO_v3_1_PUT registro in registrosOK)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registrosOK.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Reports/AddAdvances
        // Incorpora Expenses a Reports a Captio
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostReportsAddAdvances_v3_1(ReportsAddAdvancesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/AddAdvances";
                List<ReportsAddAdvancesDTO_v3_1_POST> registrosToProcess = new List<ReportsAddAdvancesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ReportsAddAdvancesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }

            return (null);
        }


        // 
        // POST /api/v3.1/Reports/AddExpenses
        // Incorpora Expenses a Reports a Captio
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostReportsAddExpenses_v3_1(ReportsAddExpensestDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/AddExpenses";
                List<ReportsAddExpensestDTO_v3_1_POST> registrosToProcess = new List<ReportsAddExpensestDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ReportsAddExpensestDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else 
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Reports/AddComplementaryExpenses
        // Incorpora Expenses a Reports a Captio
        // Retorna "00" si s'envien varis registres i s'inserten correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostReportsAddComplementaryExpenses_v3_1(ReportsAddComplementaryExpensesDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/AddComplementaryExpenses";
                List<ReportsAddComplementaryExpensesDTO_v3_1_POST> registrosToProcess = new List<ReportsAddComplementaryExpensesDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (ReportsAddComplementaryExpensesDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Reports/Periods
        // Actualiza el nombre del report y el workflow de los reports en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchReportsPeriods_v3_1(ReportsPeriodsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/Periods";
                List<ReportsPeriodsDTO_v3_1_PATCH> registrosToProcess = new List<ReportsPeriodsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (ReportsPeriodsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Reports/CustomFields
        // Actualiza el nombre del report y el workflow de los reports en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchReportsCustomFields_v3_1(ReportsCustomFieldsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports/CustomFields";
                List<ReportsCustomFieldsDTO_v3_1_PATCH> registrosToProcess = new List<ReportsCustomFieldsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (ReportsCustomFieldsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/SII
        // Obtiene los datos del SII de Captio
        public SIIDTO_v3_1[] GetSII_v3_1(string filters)
        {
            SIIDTO_v3_1[] SIIResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/SII";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    SIIResult = JsonConvert.DeserializeObject<SIIDTO_v3_1[]>(requestResult);
                    return (SIIResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<SIIDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PUT /api/v3.1/SII/ReportExported
        // Marca els reports com a exportats a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutSIIReportExported_v3_1(SIIReportExportedDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/SII/ReportExported";
                List<SIIReportExportedDTO_v3_1_PUT> registrosToProcess = new List<SIIReportExportedDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (SIIReportExportedDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        //
        // GET /api/v3.1/Travels
        // Obtiene los Travels de Captio
        public TravelsDTO_v3_1[] GetTravels_v3_1(string filters)
        {
            TravelsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Travels";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<TravelsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<TravelsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        //
        // GET /api/v3.1/Travels/Services
        // Obtiene los datos relacionados con los servicios de los Travels de Captio
        public TravelsServicesDTO_v3_1[] GetTravelsServices_v3_1(string filters)
        {
            TravelsServicesDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Travels/Services";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<TravelsServicesDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<TravelsServicesDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PUT /api/v3.1/Travels/Approve
        // Aprovació de les sol·licituds de viatge a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutTravelsApprove_v3_1(TravelsApproveDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Travels/Approve";
                List<TravelsApproveDTO_v3_1_PUT> registrosToProcess = new List<TravelsApproveDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (TravelsApproveDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Travels/RequestApprove
        // Solicita aprovació deles sol·licituds de viatge a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutTravelsRequestApprove_v3_1(TravelsRequestApproveDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Travels/RequestApprove";
                List<TravelsRequestApproveDTO_v3_1_PUT> registrosToProcess = new List<TravelsRequestApproveDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (TravelsRequestApproveDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Travels/CustomFields
        // Actualiza los custom fields de los Travels en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchTravelsCustomFields_v3_1(TravelsCustomFieldsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Travels/CustomFields";
                List<TravelsCustomFieldsDTO_v3_1_PATCH> registrosToProcess = new List<TravelsCustomFieldsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (TravelsCustomFieldsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Users
        // Obtiene los Users de Captio
        public UsersDTO_v3_1[] GetUsers_v3_1(string filters)
        {
            UsersDTO_v3_1[] UserResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    UserResult = JsonConvert.DeserializeObject<UsersDTO_v3_1[]>(requestResult);
                    return (UserResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }



        //
        // GET /api/v3.1/Users
        // Obtiene un Users de Captio a partir del Id
        // Retorna null en cas d'error o "" si no existeix
        public UsersDTO_v3_1 GetUser_v3_1(string id)
        {
            UsersDTO_v3_1[] UserResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users";
            string filters = "\"" + "Id" + "\"" + ":" + "\"" + id + "\"";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    UserResult = JsonConvert.DeserializeObject<UsersDTO_v3_1[]>(requestResult);
                    if (UserResult.Count() == 1)
                    {
                        return (UserResult[0]);
                    }
                    else
                    {
                        return (JsonConvert.DeserializeObject<UsersDTO_v3_1>(""));
                    }
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersDTO_v3_1>(""));
                }
            }
            else
            {
                return null;
            }
        }



        // 
        // DELETE /api/v3.1/Users
        // Elimina Users a Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteUsers_v3_1(UsersDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Reports";
                List<UsersDTO_v3_1_DELETE> registrosToProcess = new List<UsersDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users
        // Actualiza los custom fields de los Travels en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsers_v3_1(UsersDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_PATCH> registrosToProcess = new List<UsersDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }




        // 
        // PATCH /api/v3.1/Users
        // Actualiza los custom fields de los Travels en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna els errors_actualitzacio dels registres que no s'han insertat correctament.
        // El format dels errors_actualitzacio es [id_usuari]-[codi_error]
        // Retorna null si error
        public string PatchUsers_v3_1(UsersDTO_v3_1_PATCH[] registros, ref List<string> errors_actualitzacio)
        {
            string Web_Response = "";

            if (auth != null)
            {
                errors_actualitzacio.Clear();
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_PATCH> registrosToProcess = new List<UsersDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                foreach (Error_400_DTO_v3_1 error in res)
                                {
                                    foreach (Error_400_DTO_v3_1_Validations desc in error.Validations)
                                    {
                                        errors_actualitzacio.Add(new string(error.Value + "-" + desc.Code));
                                    }
                                }
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }






        // 
        // POST /api/v3.1/Users
        // Crea Usuarios en Captio
        // En cas de només insertar 1 usuari, retorna el "ID" del user insertat o el codi de l'error (ERROR::+codi error). 
        // Els codis de l'error poden ser: USER_EXIST_WITH_SAME_LOGIN, INVALID_LOGIN_FORMAT, PASSWORD_IS_NULL, INVALID_PASSWORD_FORMAT, PASSWORD_WITHOUT_UPPER_AND_LOWER_LETTERS, PASSWORD_WITHOUT_SYMBOLS, REPEATED_PASSWORD, REQUIRED_USER_CUSTOM_FIELDS
        // Retorna "00" si s'han insertat correctament tots els registres, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostUsers_v3_1(UsersDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_POST> registrosToProcess = new List<UsersDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseUsersDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseUsersDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.UserId);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if ((registros.Count() == 1) && (res.Count() == 1) && (res[0].Validations.Count() == 1))
                                {
                                    return ("ERROR::" + res[0].Validations[0].Code + "-" + res[0].Validations[0].Message);
                                }
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }




        // 
        // POST /api/v3.1/Users
        // Crea Usuarios en Captio
        // Retorna en el camp ids_registres_insertats el "ID" del user insertat o el codi de l'error ("ERROR::" + login_erroni + "::" + codi error + "-" + missatge error).
        // Els codis de l'error poden ser: USER_EXIST_WITH_SAME_LOGIN, INVALID_LOGIN_FORMAT, PASSWORD_IS_NULL, INVALID_PASSWORD_FORMAT, PASSWORD_WITHOUT_UPPER_AND_LOWER_LETTERS, PASSWORD_WITHOUT_SYMBOLS, REPEATED_PASSWORD, REQUIRED_USER_CUSTOM_FIELDS
        // Retorna "00" si s'han insertat correctament tots els registres, "0-" si només s'inserten algunes correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostUsers_v3_1(UsersDTO_v3_1_POST[] registros, ref List<string> ids_registres_insertats)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_POST> registrosToProcess = new List<UsersDTO_v3_1_POST>();
                string result = null;
                ids_registres_insertats.Clear();
                int i = 0;
                foreach (UsersDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseUsersDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseUsersDTO_v3_1_POST[]>(requestResult);
                                foreach(ResponseUsersDTO_v3_1_POST responses in res)
                                {
                                    ids_registres_insertats.Add(new string ( responses.Result.UserId ));
                                }
                            }
                            catch
                            {
                                return (null);
                            }
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                            //if ((res != null) && (res.Count() == 1) && (result == null))
                            //{
                            //    result = ("00");
                            //}
                            //else
                            //{
                            //    result = (result == null || result.Equals("00")) ? "00" : "0-";
                            //}
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                try
                                {
                                    ResponseUsersDTO_v3_1_POST[] res = JsonConvert.DeserializeObject<ResponseUsersDTO_v3_1_POST[]>(Web_Response.Split("#")[1]);
                                    foreach (ResponseUsersDTO_v3_1_POST responses in res)
                                    {
                                        if (responses.Result == null)
                                        {
                                            ids_registres_insertats.Add(new string("ERROR::" + ((responses.Key.Equals("Login")) ? responses.Value : "") + "::" + responses.Validations[0].Code + "-" + responses.Validations[0].Message));
                                            result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                        }
                                        else
                                        {
                                            ids_registres_insertats.Add(new string(responses.Result.UserId));
                                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                                        }
                                    }
                                }
                                catch
                                {
                                    return (null);
                                }
                                //if (registrosToProcess.Count() == res.Count(x => (x.Result == null)))
                                //{
                                //    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                //}
                                //else
                                //{
                                //    result = "0-";
                                //}
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // PUT /api/v3.1/Users
        // Actualització dels usuaris a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutUsers_v3_1(UsersDTO_v3_1_PUT[] registros)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_PUT> registrosToProcess = new List<UsersDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // PUT /api/v3.1/Users
        // Actualització dels usuaris a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna els errors_actualitzacio dels registres que no s'han insertat correctament.
        // El format dels errors_actualitzacio es [id_usuari]-[codi_error]
        // Retorna null si error
        public string PutUsers_v3_1(UsersDTO_v3_1_PUT[] registros, ref List<string> errors_actualitzacio)
        {
            List<string> tickets_Errors = new List<string>();

            string Web_Response = "";

            if (auth != null)
            {
                errors_actualitzacio.Clear();
                string webApiUrl = captioUrl + "/api/v3.1/Users";
                List<UsersDTO_v3_1_PUT> registrosToProcess = new List<UsersDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                foreach(Error_400_DTO_v3_1 error in res)
                                {
                                    foreach (Error_400_DTO_v3_1_Validations desc in error.Validations)
                                    {
                                        errors_actualitzacio.Add(new string(error.Value + "-" + desc.Code));
                                    }
                                }
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        //
        // GET /api/v3.1/Users/Banks
        // Obtiene los Logs de los Reports de Captio
        public UsersBanksDTO_v3_1[] GetUsersBanks_v3_1(string filters)
        {
            UsersBanksDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/Banks";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<UsersBanksDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersBanksDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PATCH /api/v3.1/Users/Banks
        // Actualiza la información bancaria de los usuarios en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersBanks_v3_1(UsersBanksDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Banks";
                List<UsersBanksDTO_v3_1_PATCH> registrosToProcess = new List<UsersBanksDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersBanksDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        //GET /api/v3.1/Users/Groups
        //Obtiene los Grupos de los usuarios definidos en Captio
        public UsersGroupsDTO_v3_1[] GetUsersGroups_v3_1(string filters)
        {
            UsersGroupsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/Groups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<UsersGroupsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersGroupsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }



        // 
        // PUT /api/v3.1/Users/Groups
        // Actualitzación del Grupo de Usuarios a los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutUsersGroups_v3_1(UsersGroupsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Groups";
                List<UsersGroupsDTO_v3_1_PUT> registrosToProcess = new List<UsersGroupsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersGroupsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // PUT /api/v3.1/Users/Groups
        // Actualitzación del Grupo de Usuarios a los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna els errors_actualitzacio dels registres que no s'han insertat correctament.
        // El format dels errors_actualitzacio es [id_usuari]-[codi_error]
        // Retorna null si error
        public string PutUsersGroups_v3_1(UsersGroupsDTO_v3_1_PUT[] registros, ref List<string> errors_actualitzacio)
        {
            string Web_Response = "";

            if (auth != null)
            {
                errors_actualitzacio.Clear();
                string webApiUrl = captioUrl + "/api/v3.1/Users/Groups";
                List<UsersGroupsDTO_v3_1_PUT> registrosToProcess = new List<UsersGroupsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersGroupsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                foreach (Error_400_DTO_v3_1 error in res)
                                {
                                    foreach (Error_400_DTO_v3_1_Validations desc in error.Validations)
                                    {
                                        errors_actualitzacio.Add(new string(error.Value + "-" + desc.Code));
                                    }
                                }
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // GET /api/v3.1/Users/Payments
        // Obtindre els Payments configurats de cada User
        public UsersPaymentsDTO_v3_1[] GetUsersPayments_v3_1(string filters)
        {
            UsersPaymentsDTO_v3_1[] UserResult;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/Payments";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    UserResult = JsonConvert.DeserializeObject<UsersPaymentsDTO_v3_1[]>(requestResult);
                    return (UserResult);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersPaymentsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Users/Payments
        // Elimina formas de pago de Users a Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteUsersPayments_v3_1(UsersPaymentsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Payments";
                List<UsersPaymentsDTO_v3_1_DELETE> registrosToProcess = new List<UsersPaymentsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (UsersPaymentsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Users/Payments
        // Incorpora tarjetas de crédito a los Usuarios de Captio
        // Retorna "00" si s'envien varis registres i s'inserten tots correctament, "0-" si només s'inserten alguns correctament i "--" si no s'inserta cap registre
        // Retorna null si error
        public string PostUsersPayments_v3_1(UsersPaymentsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Payments";
                List<UsersPaymentsDTO_v3_1_POST> registrosToProcess = new List<UsersPaymentsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (UsersPaymentsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Users/Workflows
        // Obtiene los Workflows de los usuarios definidos en Captio
        public UsersWorkflowsDTO_v3_1[] GetUsersWorkflows_v3_1(string filters)
        {
            UsersWorkflowsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/Workflows";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<UsersWorkflowsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersWorkflowsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // GET /api/v3.1/Users/Delegations
        // Obtindre les Delegacions actives dels Usuaris
        public UsersDelegationsDTO_v3_1[] GetUsersDelegations_v3_1(string filters)
        {
            UsersDelegationsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/Delegations";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<UsersDelegationsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersDelegationsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Users/Delegations
        // Elimina Delegacions dels Users a Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteUsersDelegations_v3_1(UsersDelegationsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Delegations";
                List<UsersDelegationsDTO_v3_1_DELETE> registrosToProcess = new List<UsersDelegationsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (UsersDelegationsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Users/Delegations
        // Crea Delegacions d'Usuaris de Captio
        // Retorna "00" si s'envien varis registres i es creen tots correctament, "0-" si només es creen alguns correctament i "--" si no es crea cap registre
        // Retorna null si error
        public string PostUsersDelegations_v3_1(UsersDelegationsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Delegations";
                List<UsersDelegationsDTO_v3_1_POST> registrosToProcess = new List<UsersDelegationsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (UsersDelegationsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/Delegations
        // Actualiza las Delegacions de los usuarios en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersDelegations_v3_1(UsersDelegationsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Delegations";
                List<UsersDelegationsDTO_v3_1_PATCH> registrosToProcess = new List<UsersDelegationsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersDelegationsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // GET /api/v3.1/Users/TravelGroups
        // Obtener los viajes activos de los Usuarios
        public UsersTravelGroupsDTO_v3_1[] GetUsersTravelGroups_v3_1(string filters)
        {
            UsersTravelGroupsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Users/TravelGroups";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<UsersTravelGroupsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<UsersTravelGroupsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PUT /api/v3.1/Users/TravelGroups
        // Actualitzación del Grupo de Usuarios a los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutUsersTravelGroups_v3_1(UsersTravelGroupsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/TravelGroups";

                List<UsersTravelGroupsDTO_v3_1_PUT> registrosToProcess = new List<UsersTravelGroupsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersTravelGroupsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Users/KmGroups
        // Añadir Grupos de Kilometraje a los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutUsersKmGroups_v3_1(UsersKmGroupsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/KmGroups";

                List<UsersKmGroupsDTO_v3_1_PUT> registrosToProcess = new List<UsersKmGroupsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersKmGroupsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // PUT /api/v3.1/Users/KmGroups
        // Añadir Grupos de Kilometraje a los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna els errors_actualitzacio dels registres que no s'han insertat correctament.
        // El format dels errors_actualitzacio es [id_usuari]-[codi_error]
        // Retorna null si error
        public string PutUsersKmGroups_v3_1(UsersKmGroupsDTO_v3_1_PUT[] registros, ref List<string> errors_actualitzacio)
        {
            string Web_Response = "";

            if (auth != null)
            {
                errors_actualitzacio.Clear();
                string webApiUrl = captioUrl + "/api/v3.1/Users/KmGroups";
                List<UsersKmGroupsDTO_v3_1_PUT> registrosToProcess = new List<UsersKmGroupsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersKmGroupsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                foreach (Error_400_DTO_v3_1 error in res)
                                {
                                    foreach (Error_400_DTO_v3_1_Validations desc in error.Validations)
                                    {
                                        errors_actualitzacio.Add(new string(error.Value + "-" + desc.Code));
                                    }
                                }
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }



        // 
        // PUT //api/v3.1/Users/CustomFields
        // Introducir valores a los Custom Fields de los Usuarios a Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutUsersCustomFields_v3_1(UsersCustomFieldsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/CustomFields";

                List<UsersCustomFieldsDTO_v3_1_PUT> registrosToProcess = new List<UsersCustomFieldsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (UsersCustomFieldsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // DELETE /api/v3.1/Users/Workflows/Joins
        // Elimina la relación de los workflows con los Users de Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteUsersWorkflowsJoin_v3_1(UsersWorkflowsJoinDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Workflows/Join";
                List<UsersWorkflowsJoinDTO_v3_1_DELETE> registrosToProcess = new List<UsersWorkflowsJoinDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (UsersWorkflowsJoinDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/Workflows/Joins
        // Defineix un Workflow per defecte als usuaris de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersWorkflowsJoin_v3_1(UsersWorkflowsJoinDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Workflows/Joins";
                List<UsersWorkflowsJoinDTO_v3_1_PATCH> registrosToProcess = new List<UsersWorkflowsJoinDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersWorkflowsJoinDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Users/Workflows/Join
        // Crea relacions entre els usuaris de Captio i els workflows de l'entorn
        // Retorna "00" si s'envien varis registres i es creen tots correctament, "0-" si només es creen alguns correctament i "--" si no es crea cap registre
        // Retorna null si error
        public string PostUsersWorkflowsJoin_v3_1(UsersWorkflowsJoinDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Workflows/Join";
                List<UsersWorkflowsJoinDTO_v3_1_POST> registrosToProcess = new List<UsersWorkflowsJoinDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (UsersWorkflowsJoinDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/Login
        // Actualitzar el Login dels usuaris de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersLogin_v3_1(UsersLoginDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Login";

                List<UsersLoginDTO_v3_1_PATCH> registrosToProcess = new List<UsersLoginDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersLoginDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/Certified
        // Actualitza que un usuari tingui activada la certificació (pugui incorporar imatges desde la galeria)
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersCertified_v3_1(UsersCertifiedDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/Certified";
                List<UsersCertifiedDTO_v3_1_PATCH> registrosToProcess = new List<UsersCertifiedDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersCertifiedDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/AlertsActive
        // Activa alertes als usuaris de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersAlertsActive_v3_1(UsersAlertsActiveDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/AlertsActive";
                List<UsersAlertsActiveDTO_v3_1_PATCH> registrosToProcess = new List<UsersAlertsActiveDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersAlertsActiveDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Users/EditExpenseWPicture
        // Actualitza que un usuari pugui Editar els camps dels Tickets escanejats de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchUsersEditExpenseWPicture_v3_1(UsersEditExpenseWPictureDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Users/EditExpenseWPicture";

                List<UsersEditExpenseWPictureDTO_v3_1_PATCH> registrosToProcess = new List<UsersEditExpenseWPictureDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (UsersEditExpenseWPictureDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Workflows
        // Obtiene los Workflows de Captio
        public WorkflowsDTO_v3_1[] GetWorkflows_v3_1(string filters)
        {
            WorkflowsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Workflows";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<WorkflowsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<WorkflowsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // DELETE /api/v3.1/Workflows
        // Elimina Workflows de Captio
        // Retorna "00" si s'esborren tots els registres correctament, "0-" si només s'eliminem alguns correctament i "--" si no s'elimina cap registre
        // Retorna null si error
        public string DeleteWorkflows_v3_1(WorkflowsDTO_v3_1_DELETE[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows";
                List<WorkflowsDTO_v3_1_DELETE> registrosToProcess = new List<WorkflowsDTO_v3_1_DELETE>();
                string result = null;
                int i = 0;
                foreach (WorkflowsDTO_v3_1_DELETE registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = DeleteURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Workflows
        // Actualiza el nom i els camps personalitzats dels workflows de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchWorkflows_v3_1(WorkflowsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows";
                List<WorkflowsDTO_v3_1_PATCH> registrosToProcess = new List<WorkflowsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (WorkflowsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Workflows
        // Crea Workflows a Captio
        // Retorna "00" si s'envien varis registres i es creen tots correctament, "0-" si només es creen alguns correctament i "--" si no es crea cap registre
        // Retorna null si error
        public string PostWorkflows_v3_1(WorkflowsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows";
                List<WorkflowsDTO_v3_1_POST> registrosToProcess = new List<WorkflowsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (WorkflowsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : ((registros.Count() == 1) ? "--" : "0-");
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Workflows
        // Actualizar el nombre de los Workflows en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutWorkflows_v3_1(WorkflowsDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows";
                List<WorkflowsDTO_v3_1_PUT> registrosToProcess = new List<WorkflowsDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (WorkflowsDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        // GET /api/v3.1/Workflows/Steps/Alerts
        // Obtiene la alertas activadas en cada paso de los Workflows de Captio
        public WorkflowsStepsAlertsDTO_v3_1[] GetWorkflowsStepsAlerts_v3_1(string filters)
        {
            WorkflowsStepsAlertsDTO_v3_1[] Result;
            int total_registres;
            int pagNum;
            string Web_Response = "";

            string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps/Alerts";

            if (auth != null)
            {
                pagNum = 1;
                StringBuilder sbUrl = new StringBuilder();
                if ((filters != null) && (filters != ""))
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                }
                else
                {
                    sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                }

                var requestResult = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);

                total_registres = Convert.ToInt32(requestResult.Substring(0, requestResult.IndexOf("#")));
                requestResult = requestResult.Substring(requestResult.IndexOf("#") + 1);

                if ((requestResult != null) && (requestResult != "[]"))
                {

                    if ((total_registres - (pageSize * pagNum)) > 0)
                    {
                        // Paginar la crida i obtindre la següent pàgina
                        pagNum++;
                        sbUrl.Clear();
                        if ((filters != null) && (filters != ""))
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                        }
                        else
                        {
                            sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                        }

                        var requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                        requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                        requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        while (((total_registres - (pageSize * pagNum)) > 0))
                        {
                            pagNum++;
                            sbUrl.Clear();
                            if ((filters != null) && (filters != ""))
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString()).Append("&filters={").Append(filters).Append("}");
                            }
                            else
                            {
                                sbUrl.Append(webApiUrl).Append("?pagNum=" + pagNum.ToString()).Append("&pageSize=" + pageSize.ToString());
                            }
                            requestResult_next = GetURLWeb_v3(sbUrl.ToString(), customerKey, ref Web_Response);
                            requestResult_next = requestResult_next.Substring(requestResult_next.IndexOf("#") + 1);
                            requestResult = requestResult.Insert(requestResult.Length - 1, "," + requestResult_next.Substring(1, requestResult_next.Length - 2));
                        }
                    }

                    Result = JsonConvert.DeserializeObject<WorkflowsStepsAlertsDTO_v3_1[]>(requestResult);
                    return (Result);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<WorkflowsStepsAlertsDTO_v3_1[]>("[]"));
                }
            }
            else
            {
                return null;
            }
        }


        // 
        // PATCH /api/v3.1/Workflows/Steps/Alerts
        // Activar las alertas específicas en los pasos del los workflows de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchWorkflowsStepsAlerts_v3_1(WorkflowsStepsAlertsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps/Alerts";
                List<WorkflowsStepsAlertsDTO_v3_1_PATCH> registrosToProcess = new List<WorkflowsStepsAlertsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsAlertsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Workflows/Steps/Supervisor
        // Actualizar el supervisor en una paso de los Workflows en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutWorkflowsStepsSupervisor_v3_1(WorkflowsStepsSupervisorDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps/Supervisor";
                List<WorkflowsStepsSupervisorDTO_v3_1_PUT> registrosToProcess = new List<WorkflowsStepsSupervisorDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsSupervisorDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PUT /api/v3.1/Workflows/Steps/SupervisorWithoutEmail
        // Assignar supervisores a los pasos de los Workflows sin que se envíe el correo de validación a estos supervisores en Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PutWorkflowsStepsSupervisorWithoutEmail_v3_1(WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps/SupervisorWithoutEmail";
                List<WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT> registrosToProcess = new List<WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PutURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Workflows/Steps
        // Actualiza camps dels workflows de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchWorkflowsSteps_v3_1(WorkflowsStepsDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps";
                List<WorkflowsStepsDTO_v3_1_PATCH> registrosToProcess = new List<WorkflowsStepsDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // POST /api/v3.1/Workflows/Steps
        // Crea los pasos de los Workflows en Captio
        // Retorna "00" si s'envien varis registres i es creen tots correctament, "0-" si només es creen alguns correctament i "--" si no es crea cap registre
        // Retorna null si error
        public string PostWorkflowsSteps_v3_1(WorkflowsStepsDTO_v3_1_POST[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps";
                List<WorkflowsStepsDTO_v3_1_POST> registrosToProcess = new List<WorkflowsStepsDTO_v3_1_POST>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsDTO_v3_1_POST registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PostURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            ResponseDTO_v3_1_POST[] res;
                            try
                            {
                                res = JsonConvert.DeserializeObject<ResponseDTO_v3_1_POST[]>(requestResult);
                            }
                            catch
                            {
                                res = null;
                            }
                            if ((res != null) && (res.Count() == 1) && (result == null))
                            {
                                result = (res[0].Result.Id);
                            }
                            else
                            {
                                result = (result == null || result.Equals("00")) ? "00" : "0-";
                            }
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        // 
        // PATCH /api/v3.1/Workflows/Steps/Resources
        // Actualiza los nombres de los pasos de los workflows de Captio
        // Retorna "00" si s'actualitzen tots els registres correctament, "0-" si només s'actualitzen alguns correctament i "--" si no s'actualitza cap registre
        // Retorna null si error
        public string PatchWorkflowsStepsResources_v3_1(WorkflowsStepsResourcesDTO_v3_1_PATCH[] registros)
        {
            string Web_Response = "";

            if (auth != null)
            {
                string webApiUrl = captioUrl + "/api/v3.1/Workflows/Steps/Resources";
                List<WorkflowsStepsResourcesDTO_v3_1_PATCH> registrosToProcess = new List<WorkflowsStepsResourcesDTO_v3_1_PATCH>();
                string result = null;
                int i = 0;
                foreach (WorkflowsStepsResourcesDTO_v3_1_PATCH registro in registros)
                {
                    if (i < max_registres)
                    {
                        registrosToProcess.Add(registro);
                        i++;
                    }
                    if ((i == max_registres) || (registro.Equals(registros.Last())))
                    {
                        var requestResult = PatchURLWeb(webApiUrl.ToString(), JsonConvert.SerializeObject(registrosToProcess), ref Web_Response);
                        if (Web_Response == "200")
                        {
                            result = (result == null || result.Equals("00")) ? "00" : "0-";
                        }
                        else
                        {
                            if (Web_Response.Split("#")[0] == "400-BadRequest")
                            {
                                Error_400_DTO_v3_1[] res = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(Web_Response.Split("#")[1]);
                                if (registrosToProcess.Count() == res.Count())
                                {
                                    result = ((result == null) || result.Equals("--")) ? "--" : "0-";
                                }
                                else
                                {
                                    result = "0-";
                                }
                            }
                        }
                        i = 0;
                        registrosToProcess.Clear();
                    }
                }
                return (result);
            }
            return (null);
        }


        //
        //
        private string GetURLWeb_v3(string url, string customerKey, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ GET (inicio) : " + url.ToString());
                    var result = client.DownloadString(url.ToString());
                    if (result == "[]")
                        result = "0#" + result;
                    else
                        result = client.ResponseHeaders["x-total-count"].ToString() + "#" + result;
                    if (this.captioLogAPI) Log.Info("@@@ GET (fin) : " + url.ToString());
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                }
                            }
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ GET (fin) : " + url.ToString());
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }

        //
        private string PostURLWeb(string url, string parametre, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    if (parametre[0] == '[' && parametre[parametre.Length - 1] == ']')
                    {
                        parametre = parametre.Substring(1, parametre.Length - 2);
                    }
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ POST (inicio) : " + url.ToString() + " [" + parametre + "]");
                    var result = client.UploadString(url.ToString(), "POST", "[" + parametre + "]");
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + url.ToString() + " [" + parametre + "]");
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                }
                            }
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + url.ToString() + " [" + parametre + "]");
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }

        //
        private string PostURLWeb_(string url, string parametre, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ POST (inicio) : " + url.ToString() + " [" + parametre + "]");
                    var result = client.UploadString(url.ToString(), "POST", parametre);
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + url.ToString() + " [" + parametre + "]");
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                }
                            }
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + url.ToString() + " [" + parametre + "]");
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }


        //
        private string PutURLWeb(string url, string parametre, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            string errorCode = null;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    if (parametre[0] == '[' && parametre[parametre.Length - 1] == ']')
                    {
                        parametre = parametre.Substring(1, parametre.Length - 2);
                    }
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ PUT (inicio) : " + url.ToString() + " [" + parametre + "]");
                    var result = client.UploadString(url.ToString(), "PUT", "[" + parametre + "]");
                    if (this.captioLogAPI) Log.Info("@@@ PUT (fin) : " + url.ToString() + " [" + parametre + "]");
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                    errorCode = errorCode + (Validation.Code) + "-";
                                }
                            }
                            errorCode = errorCode.Substring(0, errorCode.Length - 1);
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ PUT (fin) : " + url.ToString() + " [" + parametre + "]");
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return (errorCode);
        }

        //
        private string PatchURLWeb(string url, string parametre, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    if (parametre[0] == '[' && parametre[parametre.Length - 1] == ']')
                    {
                        parametre = parametre.Substring(1, parametre.Length - 2);
                    }
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ PATCH (inicio) : " + url.ToString() + " [" + parametre + "]");
                    var result = client.UploadString(url.ToString(), "PATCH", "[" + parametre + "]");
                    if (this.captioLogAPI) Log.Info("@@@ PATCH (fin) : " + url.ToString() + " [" + parametre + "]");
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                }
                            }
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ PATCH (fin) : " + url.ToString() + " [" + parametre + "]");
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }

        //
        private string DeleteURLWeb(string url, string parametre, ref string webResponse)
        {
            bool error = false;
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                try
                {
                    CompruebaToken();
                    if (parametre[0] == '[' && parametre[parametre.Length - 1] == ']')
                    {
                        parametre = parametre.Substring(1, parametre.Length - 2);
                    }
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + auth.access_token;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Headers.Add("customerKey", customerKey);
                    client.Encoding = Encoding.UTF8;
                    if (this.captioLogAPI) Log.Info("@@@ DELETE (inicio) : " + url.ToString() + " [" + parametre + "]");
                    var result = client.UploadString(url.ToString(), "DELETE", "[" + parametre + "]");
                    if (this.captioLogAPI) Log.Info("@@@ DELETE (fin) : " + url.ToString() + " [" + parametre + "]");
                    webResponse = "200";
                    return (result);
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    error = (reintento && (intento >= this.num_reintentos));
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Error_400_DTO_v3_1[] ErrorsResult = JsonConvert.DeserializeObject<Error_400_DTO_v3_1[]>(responseText);
                            foreach (Error_400_DTO_v3_1 ErrorResult in ErrorsResult)
                            {
                                foreach (Error_400_DTO_v3_1_Validations Validation in ErrorResult.Validations)
                                {
                                    // Log
                                    Log.Error(Validation.Message);
                                }
                            }
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ DELETE (fin) : " + url.ToString() + " [" + parametre + "]");
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                    reintento = false;
                }
            }
            if (error)
            {
                // Log
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }

        //
        //
        private AuthDTO GetToken_v3(ref string webResponse)
        {
            int intento = 0;
            bool reintento = false;
            while (intento < this.num_reintentos)
            {
                reintento = false;
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    WebClient client = new WebClient();
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    NameValueCollection parameters = new NameValueCollection
                    {
                        { "grant_type", this.grant_type },
                        { "scope", this.scope },
                        { "client_id", this.client_id },
                        { "client_secret", this.client_secret }
                    };
                    if (this.captioLogAPI) Log.Info("@@@ POST (inicio) : " + this.captioUrlToken + " [" + this.customerKey + "]" + " (B2it Credentials)");
                    var result = client.UploadValues(this.captioUrlToken, "POST", parameters);
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + this.captioUrlToken + " [" + this.customerKey + "]" + " (B2it Credentials)");
                    var response = System.Text.Encoding.Default.GetString(result);
                    var obj = JsonConvert.DeserializeObject<AuthDTO>(response);
                    intento = this.num_reintentos;
                    webResponse = "200";
                    return obj;
                }
                catch (WebException ex)
                {
                    // Log
                    Log.Error(ex.Message);
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    reintento = CompruebaSiReintentoRespuesta(response, ref intento);
                    string responseText = "";
                    if (response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        webResponse = (int)response.StatusCode + "-" + response.StatusCode + "#" + responseText.Trim('\"');
                    }
                    else
                    {
                        webResponse = "503" + "-" + "503" + "#" + "Service Unavailable";
                    }
                    if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + this.captioUrlToken + " [" + this.customerKey + "]" + " (B2it Credentials)");
                    //return null;
                }
                if (reintento)
                {
                    // Log
                    Log.Error(" --> Esperamos : " + (((Convert.ToDecimal(tiempo_espera_reintento) / 1000) / 60)).ToString("0.000", CultureInfo.GetCultureInfo("es-ES")) + " minutos");
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);  // Esperamos 1 minuto
                }
            }
            if (reintento && (intento >= this.num_reintentos))
            {
                // Log
                Log.Error(webResponse);
                Log.Error("**** PROGRAMA FINALIZADO POR ERROR EN ACCESO A LA API DE CAPTIO ****");
                this.Errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "API_Error",
                    DescripcionError = "API Access Error",
                });
            }
            return null;
        }

    }
}
