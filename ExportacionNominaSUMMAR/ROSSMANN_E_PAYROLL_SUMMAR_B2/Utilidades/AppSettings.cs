using Newtonsoft.Json;

namespace CaptioB2it.Utilidades
{
    [JsonObject("AppSettings")]
    public class AppSettings
    {
        [JsonProperty("EntornoCaptio")]
        public string EntornoCaptio { get; set; }

        [JsonProperty("CustomerKey")]
        public string CustomerKey { get; set; }

        [JsonProperty("FechaUltimaEjecucion")]
        public string FechaUltimaEjecucion { get; set; }

        [JsonProperty("LogEjecucion")]
        public string LogEjecucion { get; set; }

        [JsonProperty("NombreFicheroExportacion_Contabilidad")]
        public string NombreFicheroExportacion_Contabilidad { get; set; }

        [JsonProperty("DirectorioFicherosEntrada")]
        public string DirectorioFicherosEntrada { get; set; }

        [JsonProperty("DirectorioFicherosProcesados")]
        public string DirectorioFicherosProcesados { get; set; }

        [JsonProperty("DirectorioFicherosSalida")]
        public string DirectorioFicherosSalida { get; set; }

        [JsonProperty("SeparadorCSV")]
        public string SeparadorCSV { get; set; }

        [JsonProperty("NombreCampoPersonalizadoUsuario_CodigoSUMMAR")]
        public string NombreCampoPersonalizadoUsuario_CodigoSUMMAR { get; set; }
    }

}