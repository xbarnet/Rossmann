using System;
using System.Net;
using System.Text;

namespace CaptioB2it.Utilidades
{
    public class AccesoWS
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string wsUrl;
        string wsUrlUser;
        string wsUrlPassword;
        Boolean captioLogAPI = false;


        public AccesoWS(string WsUrl, string WsUrlUser, string WsUrlPassword)
        {
            this.wsUrl = WsUrl;
            this.wsUrlUser = WsUrlUser;
            this.wsUrlPassword = WsUrlPassword;
        }


        public string PostURL(string url, string parametre)
        {
            string ApiUrl = wsUrl + url;
            try
            {
                WebClient client = new WebClient();
                client.Credentials = new NetworkCredential(wsUrlUser, wsUrlPassword);
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Encoding = Encoding.UTF8;

                if (this.captioLogAPI) Log.Info("@@@ POST (inicio) : " + ApiUrl.ToString()  + parametre);

                var result = client.UploadString(ApiUrl.ToString(), "POST", parametre);

                if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + ApiUrl.ToString() + parametre);

                return (result);
            }
            catch (WebException ex)
            {
                // Log
                Log.Error("ERROR EN LA LLAMADA POST (" + ApiUrl.ToString() + "): " + ex.Message);
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (this.captioLogAPI) Log.Info("@@@ POST (fin) : " + url.ToString() + parametre);

                return null;
            }
        }


    }
}
