using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;

namespace CaptioB2it.Utilidades
{
    public class GestionAppConfig
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public AppSettings ReadAppSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                var config = builder.Build();
                return(config.GetSection("AppSettings").Get<AppSettings>());
            }
            catch (ConfigurationErrorsException)
            {
                // Log
                Log.Error("Error reading app settings");
                return null;
            }
        }


        public void UpdateAppSettings(string key, string value)
        {
            try
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("appsettings.json"));
                jObject["AppSettings"][key] = value;

                File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + "appsettings.json", JsonConvert.SerializeObject(jObject, Formatting.Indented));
            }
            catch (ConfigurationErrorsException)
            {
                // Log
                Log.Error("Error writing app settings : " + key + ":" + value);
            }
        }
    }
}
