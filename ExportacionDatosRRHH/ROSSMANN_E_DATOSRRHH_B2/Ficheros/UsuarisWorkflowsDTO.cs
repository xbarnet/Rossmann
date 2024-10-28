using System;
using System.Collections.Generic;


namespace CaptioB2it.Ficheros
{

    public class UsuariosCargaDTO
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Codigo_empresa { get; set; }
        public string Codigo_empleado { get; set; }
        public string Centro_coste { get; set; }
        public string Grupo_km { get; set; }
        public string Grupo_usuarios { get; set; }
        public string Workflow_informes { get; set; }
        public string Workflow_anticipos { get; set; }
    }


    public class WorkflowsCargaDTO
    {
        public string Nombre { get; set; }
        public string Type { get; set; }
        public List<WorkflowsCargaDTO_etapas> Etapas { get; set; }
    }
    public class WorkflowsCargaDTO_etapas
    {
        public string Nombre_etapa { get; set; }
        public string Login_supervisor { get; set; }
    }

}
