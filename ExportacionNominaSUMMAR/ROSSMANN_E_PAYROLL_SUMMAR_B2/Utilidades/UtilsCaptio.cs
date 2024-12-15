using CaptioB2it.Entidades;
using CaptioB2it.Ficheros;
//using OfficeOpenXml.Drawing.Slicer.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace CaptioB2it.Utilidades
{

    public class UtilsCaptio
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int tiempo_espera_reintento = 30000;

        private AccesoCaptio_v3 captio_v3;
        private List<Ficheros.LogErroresDTO> errores;

        public UtilsCaptio(AccesoCaptio_v3 accesoCaptio_v3)
        {
            this.captio_v3 = accesoCaptio_v3;
            this.errores = new List<Ficheros.LogErroresDTO>();
        }

        public List<Ficheros.LogErroresDTO> ObtenerErrores()
        {
            return (this.errores);
        }



        public void EliminarErrores()
        {
            this.errores.Clear();
        }


        //
        // Obtiene el user de Captio del id
        // Devuelve null si no existe
        public UsersDTO_v3_1 GetUsuario(string id, ref List<UsersDTO_v3_1> usersCaptio)
        {
            UsersDTO_v3_1 user;

            // Obtener el usuario del Ticket
            if (!usersCaptio.Exists(x => x.Id.Equals(id)))
            {
                usersCaptio.AddRange(captio_v3.GetUsers_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id + "\""));
            }
            try
            {
                user = usersCaptio.Find(x => x.Id.Equals(id));
                return (user);
            }
            catch
            {
                // Log
                Log.Error("Usuario [" + id + "] inexistente en Captio" );
                return (null);
            }
        }


        //
        // Obtiene el user de Captio que coincide el employeeCode en el campo Employee Code (código empleado)
        // Devuelve null si no existe
        public UsersDTO_v3_1 GetUsuario_x_CodigoEmpleado(string employeeCode, List<UsersDTO_v3_1> usersCaptio)
        {
            // Obtener el usuario del Ticket
            try
            {
                var result = usersCaptio.FindLast(x => x.UserOptions.EmployeeCode.Equals(employeeCode));
                return (result);
                //foreach (UsersDTO_v3_1 usuarioCaptio in usersCaptio)
                //{
                //    if (Array.Exists(usuarioCaptio.CustomFields, (x => x.Name.Equals(nombre_CustomField))))
                //    {
                //        if ((Array.FindLast(usuarioCaptio.CustomFields, (x => x.Name.Equals(nombre_CustomField)))).Name.Equals(nombre_CustomField))
                //        {
                //            return (usuarioCaptio);
                //        }
                //    }
                //}
            }
            catch
            {
                // Log
                //Log.Error("Usuario INEXISTENTE en CAPTIO.  Usuario no encontrado con el valor " + valorCF + " en el Campo Personalizado " + nombre_CustomField);
                Log.Error("Usuario INEXISTENTE en CAPTIO.  Usuario no encontrado con el valor " + employeeCode + " en el campo EmployeeCode");
                return (null);
            }
            //return (null);
        }


        //
        // Añadir/Actualizar la tabla Users/Workflows de Captio con el id_usuari
        public void Actualitzar_UserWorkflows(string id_usuari, ref List<UsersWorkflowsDTO_v3_1> userWorkflowsCaptio)
        {
            if (!userWorkflowsCaptio.Exists(x => x.Id.Equals(id_usuari)))
            {
                userWorkflowsCaptio.AddRange(captio_v3.GetUsersWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id_usuari + "\""));
            }
        }


        //
        // Añadir/Actualizar la tabla Users/Workflows de Captio del la taula ids_usuari
        public void Actualitzar_UserWorkflows(List<string> ids_usuari, ref List<UsersWorkflowsDTO_v3_1> userWorkflowsCaptio)
        {
            string ids = "";
            int cont = 0;
            foreach (string id_usuari in ids_usuari)
            {
                if (!userWorkflowsCaptio.Exists(x => x.Id.Equals(id_usuari)))
                {
                    ids = ids + ((String.IsNullOrEmpty(ids)) ? "" : ",") + id_usuari;
                    cont++;
                }
                if (cont >= 100)
                {
                    ids = "[" + ids + "]";
                    userWorkflowsCaptio.AddRange(captio_v3.GetUsersWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + ids + "\""));
                    cont = 0;
                    ids = "";
                }
            }
            if (cont > 0)
            {
                ids = "[" + ids + "]";
                userWorkflowsCaptio.AddRange(captio_v3.GetUsersWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + ids + "\""));
            }
        }



        //
        // Obtiene el workflow de Captio de un usuario
        // tipo = 1 (workflow de informes). tipo = 2 (workflow de anticipos). tipo = 3 (workflow de viajes)
        // Devuelve null si no existe
        public WorkflowsDTO_v3_1 GetWorkflowUsuario(string user_id, int tipo, ref List<WorkflowsDTO_v3_1> workflowsCaptio, List<UsersWorkflowsDTO_v3_1> workflowsUsuariosCaptio)
        {
            WorkflowsDTO_v3_1 workflow;
            string filtre = "";
            string workflowUser_id = "";

            // Obtener Workflow del usuario
            try
            {
                List<UsersWorkflowsDTO_v3_1_Workflows> workflowsUser = new List<UsersWorkflowsDTO_v3_1_Workflows>();
                workflowsUser.AddRange(Array.FindAll(workflowsUsuariosCaptio.Find(x => x.Id.Equals(user_id)).Workflows, w => w.Type.Equals(tipo.ToString())));
                workflowUser_id = (workflowsUser.Exists(x => x.IsDefault.ToLower().Equals("true"))) ? (workflowsUser.Last(x => x.IsDefault.ToLower().Equals("true")).Id) : (workflowsUser.Last().Id);

                if (!workflowsCaptio.Exists(x => x.Id.Equals(workflowUser_id)))
                {
                    filtre = "\"" + "Id" + "\"" + ":" + "\"" + workflowUser_id + "\"";
                    workflowsCaptio.AddRange(captio_v3.GetWorkflows_v3_1(filtre));
                }
                workflow = workflowsCaptio.Find(x => x.Id.Equals(workflowUser_id));
                return (workflow);
            }
            catch
            {
                return (null);
            }
        }



        //
        // Obtiene el Step_ID de un Captio de un workflow en el que se encuentra el id_usuario como aprobador
        // Devuelve null si no existe
        public string GetStepId(string workflow_id, string id_usuario, ref List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            try
            {
                if (!workflowsCaptio.Exists(x => x.Id.Equals(workflow_id)))
                {
                    workflowsCaptio.AddRange(captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + workflow_id + "\""));
                }
                WorkflowsDTO_v3_1 workflow = workflowsCaptio.Find(x => x.Id.Equals(workflow_id));

                string result = Array.Find(workflow.Steps, x => x.SupervisorId.Equals(id_usuario)).Id;

                return (result);
            }
            catch
            {
                return (null);
            }
        }



        //
        // Crea un report en Captio y asigna el valorCF por defecto a los custom fields obligatorios del informe
        // Devuelve null si error o  "00" si se inserta correctamente
        public string CrearReport_conCustomFields(ReportsDTO_v3_1_POST reportToAdd, string valorCF)
        {
            string resposta = "";
            ReportsDTO_v3_1 report = new ReportsDTO_v3_1();

            resposta = captio_v3.PostReport_v3_1(reportToAdd, ref report);
            if ((resposta == null) || (resposta == "00"))
            {
                return (resposta);
            }
            else
            {
                if (resposta.Contains("The CustomField") && resposta.Contains("is required"))
                {
                    try
                    {
                        List<ReportsDTO_v3_1_POST_CustomField> customFields = new List<ReportsDTO_v3_1_POST_CustomField>();
                        if (reportToAdd.CustomFields != null)
                        {
                            customFields.AddRange(reportToAdd.CustomFields);
                        }
                        string[] customFields_resposta = resposta.Split(".");
                        string id_customField = "";
                        foreach (string customField_resposta in customFields_resposta)
                        {
                            id_customField = customField_resposta.Replace("The CustomField", "").Replace("is required", "").Trim();
                            if (!String.IsNullOrEmpty(id_customField))
                            {
                                customFields.Add(new ReportsDTO_v3_1_POST_CustomField
                                {
                                    Id = id_customField,
                                    Value = valorCF
                                });
                            }
                        }
                        reportToAdd = (new ReportsDTO_v3_1_POST
                        {
                            Name = reportToAdd.Name,
                            User = reportToAdd.User,
                            Workflow = reportToAdd.Workflow,
                            CustomFields = customFields.ToArray()
                        });
                        return (CrearReport_conCustomFields(reportToAdd, valorCF));
                    }
                    catch
                    {
                        return (null);
                    }
                }
                else
                {
                    return (null);
                }
            }
        }



        //
        // Obtiene el Id del último informe aplicando el filtre como condición en la selección de los informes
        // Devuelve null si el informe no existe
        public string GetIdInforme(string filtre)
        {
            string id_informe = null;
            bool trobat = false;
            int intento = 0;

            while ((!trobat) && (intento < 10))
            {
                try
                {
                    id_informe = captio_v3.GetReports_v3_1(filtre).Last().Id;
                    trobat = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);
                    intento++;
                    id_informe = null;
                }
            }
            return (id_informe);
        }


        //
        // Obtiene el último informe aplicando el filtre como condición en la selección de los informes
        // Devuelve null si el informe no existe
        public ReportsDTO_v3_1 GetInforme(string filtre)
        {
            ReportsDTO_v3_1 result = null;
            bool trobat = false;
            int intento = 0;

            while ((!trobat) && (intento < 10))
            {
                try
                {
                    result = captio_v3.GetReports_v3_1(filtre).Last();
                    trobat = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);
                    intento++;
                    result = null;
                }
            }
            return (result);
        }



        //
        // Obtiene el último workflow aplicando el filtre como condición en la selección del workflow
        // Devuelve null si el informe no existe
        public WorkflowsDTO_v3_1 GetLastWorkflow(string filtre)
        {
            WorkflowsDTO_v3_1 result = null;
            bool trobat = false;
            int intento = 10;

            while ((!trobat) && (intento > 0))
            {
                try
                {
                    result = captio_v3.GetWorkflows_v3_1(filtre).Last();
                    trobat = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(tiempo_espera_reintento / intento);
                    intento--;
                    result = null;
                }
            }
            return (result);
        }



        //
        // Elimina todos los tickets de una Categoria
        // Devuelve el número de tickets eliminados o -1 en caso de error
        public int DeleteTicketsDeUnaCategoria(string categoryName)
        {
            ExpensesDTO_v3_1[] expenses;
            List<ExpensesDTO_v3_1_DELETE> expensesToDelete = new List<ExpensesDTO_v3_1_DELETE>();
            List<ReportsRemoveExpensesDTO_v3_1_PUT> expensesReportToDelete = new List<ReportsRemoveExpensesDTO_v3_1_PUT>();
            List<ReportsRemoveExpensestDTO_v3_1_PUT_Expenses> expensesReportToDelete_Expenses = new List<ReportsRemoveExpensestDTO_v3_1_PUT_Expenses>();
            bool error = false;

            int contador = 0;

            expenses = captio_v3.GetExpenses_v3_1("\"" + "Category_Name" + "\"" + ":" + "\"" + categoryName + "\"");

            foreach (ExpensesDTO_v3_1 expense in expenses)
            {
                try
                {
                    if ((expense.Report != null) && (!String.IsNullOrEmpty(expense.Report.Id)))
                    {
                        // Treure el ticket del report
                        expensesReportToDelete.Clear();
                        expensesReportToDelete_Expenses.Clear();
                        expensesReportToDelete_Expenses.Add(new ReportsRemoveExpensestDTO_v3_1_PUT_Expenses
                        {
                            Id = expense.Id,
                            Comments = categoryName
                        });
                        expensesReportToDelete.Add(new ReportsRemoveExpensesDTO_v3_1_PUT
                        {
                            Id = expense.Report.Id,
                            Expenses = expensesReportToDelete_Expenses.ToArray()
                        });
                        if (captio_v3.PutReportsRemoveExpenses_v3_1(expensesReportToDelete.ToArray()) == "00")
                        {
                            // Log
                            Log.Info("ELIMINADO TICKET : " + expense.ExternalId.Trim() + " DEL INFORME: " + expense.Report.Id);
                        }
                    }

                    // Esborrar ticket
                    expensesToDelete.Clear();
                    expensesToDelete.Add(new ExpensesDTO_v3_1_DELETE
                    {
                        Id = expense.Id
                    });
                    if (captio_v3.DeleteExpenses_v3_1(expensesToDelete.ToArray()) == "00")
                    {
                        contador++;
                        // Log
                        Log.Info("ELIMINADO TICKET : " + expense.ExternalId.Trim() + " DE LA CATEGORIA : " + categoryName);
                    }
                }
                catch (Exception ex)
                {
                    // Log
                    Log.Error(ex.Message);

                    // Log Error
                    errores.Add(new Ficheros.LogErroresDTO
                    {
                        FechaHora = DateTime.Now,
                        TipoError = "System Error",
                        DescripcionError = "Captio Access error"
                    });
                    error = true;
                }
            }
            if (error)
            {
                return (-1);
            }
            else
            {
                return (contador);
            }
        }


        //
        // Assignar despeses a un informe
        public void AddTicketsToReport(string informe_id, List<ExpensesDTO_v3_1> expenses)
        {
            List<ReportsAddExpensestDTO_v3_1_POST> reportExpensesToAdd = new List<ReportsAddExpensestDTO_v3_1_POST>();
            List<ReportsAddExpensestDTO_v3_1_POST_Expenses> expensesToAdd = new List<ReportsAddExpensestDTO_v3_1_POST_Expenses>();
            List<ReportsRequestApproveDTO_v3_1_PUT> reportsToRequestApprove = new List<ReportsRequestApproveDTO_v3_1_PUT>();

            // Assignar les despeses a l'informe
            expensesToAdd.Clear();
            reportExpensesToAdd.Clear();
            foreach (ExpensesDTO_v3_1 expense in expenses)
            {
                expensesToAdd.Add(new ReportsAddExpensestDTO_v3_1_POST_Expenses
                {
                    Id = expense.Id
                });
                Log.Info("---> Añadiendo ticket Id " + expense.Id + " al Report_Id : " + informe_id);
            }
            reportExpensesToAdd.Add(new ReportsAddExpensestDTO_v3_1_POST
            {
                Id = informe_id,
                Expenses = expensesToAdd.ToArray()
            });
            Log.Info("---> Añadiendo tickets al Report_Id : " + informe_id);
            captio_v3.PostReportsAddExpenses_v3_1(reportExpensesToAdd.ToArray());
        }


        // 
        // Solicita la aprobación de un informe, elimina los tickets que no hayan pasado la validación y elimina el report en caso de que se hayan eliminado todos los tickets
        // Parámetros: reportToRequestApprove es el report que solicita aprobación, user es el usuario del informe a aprobar, expenses son los tiquets que contiene el informe a aprobar
        // Devuelve 0 si todo OK, -1 en caso de error, -2 si se ha elimnado el informe, y el número de tickets eliminados en caso de que haya tiquets que no hayan pasado las validaciones
        public int RequestApproveInforme(ReportsRequestApproveDTO_v3_1_PUT informeToRequestApprove, UsersDTO_v3_1 user, List<ExpensesDTO_v3_1> expenses)
        {
            List<ReportsRemoveExpensesDTO_v3_1_PUT> reportExpensesToRemove = new List<ReportsRemoveExpensesDTO_v3_1_PUT>();
            List<ReportsRemoveExpensestDTO_v3_1_PUT_Expenses> expensesToRemove = new List<ReportsRemoveExpensestDTO_v3_1_PUT_Expenses>();
            List<ReportsDTO_v3_1_DELETE> reportsToDelete = new List<ReportsDTO_v3_1_DELETE>();
            List<ReportsRequestApproveDTO_v3_1_PUT> reportsToApprove = new List<ReportsRequestApproveDTO_v3_1_PUT>();
            List<string> ids_report_expenses = new List<string>();

            foreach (ExpensesDTO_v3_1 expense in expenses)
            {
                ids_report_expenses.Add(expense.Id);
            }

            reportsToApprove.Clear();
            reportsToApprove.Add(informeToRequestApprove);
            //
            // Sol·licitar aprobació de l'informe
            Log.Info("---> Solicitando aprobación del Report : " + informeToRequestApprove.Id + ".  Del usuario : " + user.Name + " (Id : " + user.Id + ")");
            var result = captio_v3.PutReportsRequestApprove_v3_1(reportsToApprove.ToArray());

            // Comprovar si hi ha tickets que cal eliminar (no han passat validacions)
            if (result != null)
            {
                if (result.Count() > 0)
                {
                    expensesToRemove.Clear();
                    // Eliminar els tickets que no passen la validació
                    foreach (string id in result)
                    {
                        // REPORT_MANDATORY_CUSTOMFIELD_VALUE_NOT_FOUND
                        if (id.Split("-")[1].Equals("REPORT_MANDATORY_CUSTOMFIELD_VALUE_NOT_FOUND"))
                        {
                            expensesToRemove.Add(new ReportsRemoveExpensestDTO_v3_1_PUT_Expenses
                            {
                                Id = id.Split("-")[0],
                                Comments = ""
                            });
                            ids_report_expenses.Remove(id.Split("-")[0]);
                        }

                        // CUSTOM_FIELD_VALUE_NOT_ALLOWED
                        else if (id.Split("-")[1].Equals("CUSTOM_FIELD_VALUE_NOT_ALLOWED"))
                        {
                            string expenseId = "";
                            var customFieldExternalId = captio_v3.GetCustomFields_v3_1("\"" + "ExternalId" + "\"" + ":" + "\"" + id.Split("-")[0] + "\"").First().Id;

                            foreach (ExpensesDTO_v3_1 expense in expenses)
                            {
                                var valor = Array.FindAll(expense.CustomFields, x => x.Id.Equals(customFieldExternalId));

                                foreach (ExpensesDTO_v3_1_CustomField customField in valor)
                                {
                                    if (String.IsNullOrEmpty(customField.Value))
                                    {
                                        // treure el ticket
                                        expenseId = expense.Id;
                                        expensesToRemove.Add(new ReportsRemoveExpensestDTO_v3_1_PUT_Expenses
                                        {
                                            Id = expenseId,
                                            Comments = ""
                                        });
                                        ids_report_expenses.Remove(expenseId);
                                        break;
                                    }
                                }
                            }

                        }


                    }
                    // Log
                    Log.Info("---> Eliminado tickets que no han passado validación del Report : " + informeToRequestApprove.Id + ".  Del usuario : " + user.Name + " (Id : " + user.Id + ")");
                    reportExpensesToRemove.Clear();
                    reportExpensesToRemove.Add(new ReportsRemoveExpensesDTO_v3_1_PUT
                    {
                        Id = informeToRequestApprove.Id,
                        Expenses = expensesToRemove.ToArray()
                    });
                    captio_v3.PutReportsRemoveExpenses_v3_1(reportExpensesToRemove.ToArray());

                    // Comprovar si l'informe té algún ticket
                    var ticketsInforme = captio_v3.GetExpenses_v3_1("\"" + "Report_Id" + "\"" + ":" + "\"" + informeToRequestApprove.Id + "\"");
                    if (ticketsInforme.Length == 0)
                    {
                        // L'informe no té cap ticket. S'elimina l'informe
                        Log.Info("---> Report sin tickets : " + informeToRequestApprove.Id + ".  Del usuario : " + user.Name + " (Id : " + user.Id + ")");
                        Log.Info("---> Se elimina el report : " + informeToRequestApprove.Id + ".  Del usuario : " + user.Name + " (Id : " + user.Id + ")");

                        reportsToDelete.Clear();
                        reportsToDelete.Add(new ReportsDTO_v3_1_DELETE { Id = informeToRequestApprove.Id });
                        captio_v3.DeleteReports_v3_1(reportsToDelete.ToArray());

                        return (-2);

                    }
                    else
                    {
                        // L'informe té tickets. S'envia a aprovar l'informe
                        // Sol·lictar aprovació
                        List<ExpensesDTO_v3_1> reportExpenses = new List<ExpensesDTO_v3_1>();
                        foreach (ExpensesDTO_v3_1 expense in expenses)
                        {
                            if (ids_report_expenses.Exists(x => x.Equals(expense.Id)))
                            {
                                reportExpenses.Add(expense);
                            }
                        }
                        RequestApproveInforme(informeToRequestApprove, user, reportExpenses);
                        return (result.Count());
                    }
                }
                else
                {
                    // S'ha aprovat correctament l'informe
                    return (0);
                }
            }
            else
            {
                // Log Error
                errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "System Error",
                    DescripcionError = "Captio Access error"
                });
                return (-1);
            }

        }


        //
        // Fa l'aprovación d'un informe
        // Aprova el número d'etapes que s'indiquin
        // Devuelve 0 si todo OK, -1 en caso de error
        public int AppoveInforme(ReportsApproveDTO_v3_1_PUT informeToApprove, int num_etapes)
        {
            List<ReportsApproveDTO_v3_1_PUT> reportsToAprove = new List<ReportsApproveDTO_v3_1_PUT>();
            bool error = false;
            string res = "";

            reportsToAprove.Add(informeToApprove);

            // Aprovar l'informe
            for (int i = 0; i < num_etapes; i++)
            {
                res = captio_v3.PutReportsApprove_v3_1(reportsToAprove.ToArray());
                error = error || (res == "00");
            }

            if (error)
            {
                return (-1);
            }
            else
            {
                return (0);
            }
        }



        //
        // Devuelve el nombre del informe del último informe creado según el filtro de informes
        // Devuelve null en caso de no existir ningún informe
        public string GetNombreInforme(string filter)
        {
            string result = "";
            List<ReportsDTO_v3_1> informes = new List<ReportsDTO_v3_1>();
            double i = 0;
            int contador = 0;
            DateTime fechaGeneracion = DateTime.Now;
            DateTime fechaCreacion = fechaGeneracion.AddDays(i);

            // "CreationDate":">2020-04-13T00:00:00.00Z"
            string filtre = (String.IsNullOrEmpty(filter)) ? ("\"" + "CreationDate" + "\"" + ":" + "\"" + ">" + fechaCreacion.ToString("yyyy-MM-dd") + "T00:00:00.00Z" + "\"") : (filter + "," + "\"" + "CreationDate" + "\"" + ":" + "\"" + ">" + fechaCreacion.ToString("yyyy-MM-dd") + "T00:00:00.00Z" + "\"");
            informes.AddRange(captio_v3.GetReports_v3_1(filtre));
            while ((informes.Count() == 0) && (contador > 10))
            {
                contador++;
                i = (i - 1) * contador;
                fechaCreacion = fechaGeneracion.AddDays(i);
                filtre = (String.IsNullOrEmpty(filter)) ? ("\"" + "CreationDate" + "\"" + ":" + "\"" + ">" + fechaCreacion.ToString("yyyy-MM-dd") + "T00:00:00.00Z" + "\"") : (filter + "," + "\"" + "CreationDate" + "\"" + ":" + "\"" + ">" + fechaCreacion.ToString("yyyy-MM-dd") + "T00:00:00.00Z" + "\"");
                informes.AddRange(captio_v3.GetReports_v3_1(filtre));
            }

            if (informes.Count() == 0)
            {
                informes.AddRange(captio_v3.GetReports_v3_1((String.IsNullOrEmpty(filter)) ? ("") : (filter)));
            }

            if (informes.Count() > 0)
            {
                result = informes.Last().Name;
                return (result);
            }
            else
            {
                return (null); ;
            }
        }



        //
        // Rutina per eliminar els workflows on apareixen els ids_usuaris com a supervisors
        public bool EliminarWorkflows_UsuarisSupervisors(string[] ids_usuaris, List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            bool error = false;
            List<WorkflowsDTO_v3_1_DELETE> workflowsToDelete = new List<WorkflowsDTO_v3_1_DELETE>();

            foreach (WorkflowsDTO_v3_1 workflow in workflowsCaptio)
            {
                foreach (string supervisor in ids_usuaris)
                {
                    if ((workflow.Steps != null) && (Array.Exists(workflow.Steps, s => s.SupervisorId.Equals(supervisor))))
                    {
                        // Eliminar el workflow
                        workflowsToDelete.Add(new WorkflowsDTO_v3_1_DELETE
                        {
                            Id = workflow.Id
                        });
                    }
                }
            }
            if (workflowsToDelete.Count() > 0)
            {
                var aux = captio_v3.DeleteWorkflows_v3_1(workflowsToDelete.ToArray());
                if (aux != "00")
                {
                    // Log
                    Log.Error("Error al eliminar workflows");
                    // Log Error
                    errores.Add(new LogErroresDTO
                    {
                        FechaHora = DateTime.Now,
                        TipoError = "Error API Captio",
                        DescripcionError = "Error al eliminar workflows"
                    });
                    error = true;
                }
            }
            return (error);
        }


        // 
        // Rutina per eliminar un report sense enviar mail a l'usuari
        // Rebutja l'informe fins a l'estat borrador y després l'elimina.  No envia mail a l'usuari
        public bool EliminarReport(ReportsDTO_v3_1 report)
        {
            bool error = false;

            try
            {
                // Passar report a estat borrador
                List<ReportsRejectDTO_v3_1_PUT> reportsToReject = new List<ReportsRejectDTO_v3_1_PUT>
                {
                    new ReportsRejectDTO_v3_1_PUT
                    {
                        Id = report.Id,
                        CC = null,
                        Comment = null,
                        SendEmail = false
                    }
                };
                ReportsDTO_v3_1 aux = report;
                while ((!aux.Status.Equals("1")) && (!aux.Status.Equals("3")))
                {
                    var res1 = captio_v3.PutReportsReject_v3_1(reportsToReject.ToArray());
                    error = ((!String.IsNullOrEmpty(res1)) && (res1 == "00"));
                    aux = captio_v3.GetReports_v3_1("\"" + "Id" + "\"" + ":" + "\"" + report.Id + "\"").First();
                }

                // Elminar report
                List<ReportsDTO_v3_1_DELETE> reportsToDelete = new List<ReportsDTO_v3_1_DELETE>
                {
                    new ReportsDTO_v3_1_DELETE
                    {
                        Id = report.Id
                    }
                };
                var res2 = captio_v3.DeleteReports_v3_1(reportsToDelete.ToArray());
                error = ((!String.IsNullOrEmpty(res2)) && (res2 != "00"));
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);
            }

            return (error);
        }



        //
        // Obtindre el Id del darrer informe segons el filtre de selecció
        public string ObtenerIdInforme(string filtre)
        {
            string id_informe = "";
            bool error = false;
            int intento = 0;

            while ((!error) && (intento < 10))
            {
                try
                {
                    id_informe = captio_v3.GetReports_v3_1(filtre).Last().Id;
                    error = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(tiempo_espera_reintento);
                    intento++;
                    id_informe = "";
                }
            }

            return (id_informe);
        }


        //
        // Obtindre el darrer informe segons el filtre de selecció
        public ReportsDTO_v3_1 ObtenerInforme(string filtre)
        {
            ReportsDTO_v3_1 informe = new ReportsDTO_v3_1();
            bool error = false;
            int intento = 0;

            while ((!error) && (intento < 10))
            {
                try
                {
                    informe = captio_v3.GetReports_v3_1(filtre).Last();
                    error = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(30000);
                    intento++;
                    informe = null;
                }
            }

            return (informe);
        }


        //
        // Rutina per enviar un mail desde Captio
        // Language: es-ES,en-US,ca-ES,pt-PT,fr-FR,de-DE,ja-JP,it-IT,eu-ES,gl-ES
        // Language: es (Español), ca (Catalan), en (Inglés), fr (Frances), pt(Portugués), it(Italiano), de(alemán), ja(Japonés), eu(Euskera), gl(Gallego)
        // LogStatus: 0 (ERROR), 1 (WARNING), 2 (OK)
        // TipusLog = 1 : log con todos los campos
        // TipusLog = 2 : log solo con los dos primeros campos
        public bool EnviarLog_x_Mail(List<GestionLogError.LogDTO> Logs, string nameFileAttached, string Mail_To, string Subject, string Language, string LogStatus, int TipusLog)
        {
            bool error = false;

            //string FileAttached = "";

            try
            {
                // Preparamos el fichero de log
                File.WriteAllText(nameFileAttached, GestionLogError.ConvertirLogToString(Logs, TipusLog, true));

                // Enviamos el mail
                error = EnviarFitxerLog_x_Mail(Mail_To, Subject, nameFileAttached, Language, LogStatus);

                // Eliminamos el fichero de log
                if (!error)
                {
                    File.Delete(nameFileAttached);
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);

                // Log Error
                errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "System Error",
                    DescripcionError = "Captio Access error"
                });
                error = true;
            }

            return (error);
        }


        // Obtindre el Id de CustomField a partir del seu valor
        private string ObtenerIdCustomField(string id_CustomField, string value, string codeValue, ref List<CustomFieldsDTO_v3_1> customFields, ref List<CustomFieldsItemsDTO_v3_1> customFieldsItems, ref List<AssignmentObjectsValuesDTO_v3_1> assignmentObjectsValues)
        {
            string result = null;

            if (!customFields.Exists(x => x.Id.Equals(id_CustomField) && x.Deleted.ToLower().Equals("false")))
            {
                customFields.AddRange(captio_v3.GetCustomFields_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id_CustomField + "\"" + "," + "\"" + "Deleted" + "\"" + ":0"));
            }
            CustomFieldsDTO_v3_1 CF = customFields.Find((x => x.Id.Equals(id_CustomField) && x.Deleted.ToLower().Equals("false")));
            if (!(CF.DataType.Equals("6") || CF.DataType.Equals("9") || CF.DataType.Equals("11") || CF.DataType.Equals("12")))
            {
                return (null);
            }

            if (CF.AssignmentObjectId == null)
            {
                // CustomFields

            }
            else
            {
                // AssignmentObjects
                List<AssignmentObjectsValuesDTO_v3_1_Values> valoresAssignmentObjects = new List<AssignmentObjectsValuesDTO_v3_1_Values>();
                if (!assignmentObjectsValues.Exists(x => x.Id.Equals(CF.AssignmentObjectId)))
                {
                    assignmentObjectsValues.AddRange(captio_v3.GetAssignmentObjectsValues_v3_1("\"" + "Id" + "\"" + ":" + "\"" + CF.AssignmentObjectId + "\""));
                }
                valoresAssignmentObjects.AddRange(assignmentObjectsValues.Find(x => x.Id.Equals(CF.AssignmentObjectId)).Values.ToList());
                if ((value != null) && (codeValue != null))
                {
                    try { result = valoresAssignmentObjects.Find(x => x.Value.Equals(value) && x.Code.Equals(codeValue) && x.Deleted.ToLower().Equals("false")).Id; }
                    catch { result = null; }
                }
                else if (value != null)
                {
                    try { result = valoresAssignmentObjects.Find(x => x.Value.Equals(value) && x.Deleted.ToLower().Equals("false")).Id; }
                    catch { result = null; }
                }
                else if (codeValue != null)
                {
                    try { result = valoresAssignmentObjects.Find(x => x.Code.Equals(codeValue) && x.Deleted.ToLower().Equals("false")).Id; }
                    catch { result = null; }
                }
            }


            return (result);
        }


        //
        // Obtiene el workflow de Captio del id
        // Devuelve null si no existe
        public WorkflowsDTO_v3_1 GetWorkflow(string id, ref List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            WorkflowsDTO_v3_1 workflow;

            // Obtener el usuario del Ticket
            if (!workflowsCaptio.Exists(x => x.Id.Equals(id)))
            {
                workflowsCaptio.AddRange(captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id + "\""));
            }
            try
            {
                workflow = workflowsCaptio.Find(x => x.Id.Equals(id));
                return (workflow);
            }
            catch
            {
                // Log
                Log.Error("Workflow [" + id + "] inexistente en Captio");
                return (null);
            }
        }



        //
        // Rutina per enviar un mail desde Captio
        // Language: es-ES,en-US,ca-ES,pt-PT,fr-FR,de-DE,ja-JP,it-IT,eu-ES,gl-ES
        // Language: es (Español), ca (Catalan), en (Inglés), fr (Frances), pt(Portugués), it(Italiano), de(alemán), ja(Japonés), eu(Euskera), gl(Gallego)
        // LogStatus: 0 (ERROR), 1 (WARNING), 2 (OK)
        // TipusLog = 1 : log con todos los campos
        // TipusLog = 2 : log solo con los dos primeros campos
        public bool EnviarLog_x_Mail(List<GestionLogError.LogDTO> Logs, string nameFileAttached, string Mail_To, string Subject, string Language, string LogStatus, bool EliminarFitxerLocal, int TipusLog)
        {
            bool error = false;

            //string FileAttached = "";

            try
            {
                // Preparamos el fichero de log
                File.WriteAllText(nameFileAttached, GestionLogError.ConvertirLogToString(Logs, TipusLog, true));

                // Enviamos el mail
                error = EnviarFitxerLog_x_Mail(Mail_To, Subject, nameFileAttached, Language, LogStatus);

                // Eliminamos el fichero de log
                if ((!error) && (EliminarFitxerLocal))
                {
                    File.Delete(nameFileAttached);
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);

                // Log Error
                errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "System Error",
                    DescripcionError = "Captio Access error"
                });
                error = true;
            }

            return (error);
        }



        //
        // Rutina per enviar un mail desde Captio
        // Language: es-ES,en-US,ca-ES,pt-PT,fr-FR,de-DE,ja-JP,it-IT,eu-ES,gl-ES
        // Language: es (Español), ca (Catalan), en (Inglés), fr (Frances), pt(Portugués), it(Italiano), de(alemán), ja(Japonés), eu(Euskera), gl(Gallego)
        // LogStatus: 0 (ERROR), 1 (WARNING), 2 (OK)
        public bool EnviarFitxerLog_x_Mail(string Mail_To, string Subject, string FileAttached, string Language, string LogStatus)
        {
            bool error = false;
            string sendTo = "";
            string logFileName = "";
            string logContent = "";
            string taskName = Subject;
            string language = "";

            try
            {
                // Preparamos el mail
                foreach (var address in Mail_To.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (UtilesGenericas.IsValidEmail(address))
                    {
                        sendTo = (String.IsNullOrEmpty(sendTo)) ? (address) : (sendTo + ";" + address);
                    }
                    else
                    {
                        // Log
                        Log.Error("Error en el envio de Mail. Dirección incorrecta: " + address);
                        // Log Error
                        errores.Add(new LogErroresDTO
                        {
                            FechaHora = DateTime.Now,
                            TipoError = "Smtp_Error",
                            DescripcionError = "Dirección incorrecta : " + address
                        });
                        error = true;
                    }
                }

                logFileName = Path.GetFileName(FileAttached);
                logContent = UtilesGenericas.EncodeStringToBase64(File.ReadAllText(FileAttached));
                // Language: es-ES,en-US,ca-ES,pt-PT,fr-FR,de-DE,ja-JP,it-IT,eu-ES,gl-ES
                switch (Language)
                {
                    case "es":
                        language = "es-ES";
                        break;
                    case "en":
                        language = "en-US";
                        break;
                    case "ca":
                        language = "ca-ES";
                        break;
                    case "pt":
                        language = "pt-PT";
                        break;
                    case "fr":
                        language = "fr-FR";
                        break;
                    case "de":
                        language = "de-DE";
                        break;
                    case "ja":
                        language = "ja-JP";
                        break;
                    case "it":
                        language = "it-IT";
                        break;
                    case "eu":
                        language = "eu-ES";
                        break;
                    case "gl":
                        language = "gl-ES";
                        break;
                    default:
                        language = "en-US";
                        break;
                }

                // Envio del mail
                var aux = captio_v3.PostLogs_v3_1(new LogsDTO_v3_1_POST { SendTo = sendTo, LogStatus = LogStatus, TaskName = taskName, LogFileName = logFileName, LogContent = logContent, LanguageCode = language });
                if (aux == null)
                {
                    // Log
                    Log.Error("Error al enviar log");

                    // Log Error
                    errores.Add(new Ficheros.LogErroresDTO
                    {
                        FechaHora = DateTime.Now,
                        TipoError = "API Error",
                        DescripcionError = "API Access error"
                    });
                    error = true;
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(ex.Message);

                // Log Error
                errores.Add(new Ficheros.LogErroresDTO
                {
                    FechaHora = DateTime.Now,
                    TipoError = "System Error",
                    DescripcionError = "Captio Access error"
                });
                error = true;
            }

            return (error);
        }




        //
        // Contabilidad
        private string ObtenerEstadoInforme(ReportsDTO_v3_1 informe)
        {
            string result = "";
            switch (informe.Status)
            {
                case "1":
                    result = "Borrador";
                    break;
                case "2":
                    result = "Aptobación solicitada";
                    break;
                case "3":
                    result = "Rechazado";
                    break;
                case "4":
                    result = "Aprobado";
                    break;
                case "6":
                    result = "Liquidado";
                    break;
                default:
                    break;
            }
            return (result);
        }


        //
        // Contabilidad
        // Rutina per retornar concatenades totes les alertes d'un Report
        public string ObtenerReportAlerts(string report_id, List<ReportsAlertsDTO_v3_1> report_alerts)
        {
            string result = "";

            if ((report_alerts.Count() > 0) && (report_alerts.Exists(x => x.Id.Equals(report_id) && x.Alerts.Count() > 0)))
            {
                foreach (ReportsAlertsDTO_v3_1_Alerts alert in report_alerts.Find(x => x.Id.Equals(report_id)).Alerts)
                {
                    result = result + alert.Name + "-";
                }
                result = result.Substring(0, result.Length - 1);
            }
            return (result);
        }


        //
        // Contabilidad
        // Rutina per retornar concatenades totes les Alerts d'una Expense
        // id_expense -> id de la Expense
        // alerts --> alertes del Report
        public string ObtenerExpenseAlerts(string expense_id, List<ExpensesAlertsDTO_v3_1> alerts)
        {
            string result = "";
            string fecha = "";

            List<ExpensesDTO_v3_1> expenses = new List<ExpensesDTO_v3_1>();
            string externalID_aux = "";

            if (alerts.Count > 0)
            {
                foreach (ExpensesAlertsDTO_v3_1 alert in alerts)
                {
                    if (alert.Id == expense_id)
                    {
                        fecha = DateTime.Parse(alert.Date).ToString("dd/MM/yyyy");
                        if (alert.Alerts.Length > 0)
                        {
                            foreach (ExpensesAlertsDTO_v3_1_Alerts alertaTicket in alert.Alerts)
                            {
                                result = result + "[" + fecha + "] " + alertaTicket.Name;
                                if (alertaTicket.Expenses != null)
                                {
                                    foreach (string expense_alert_id in alertaTicket.Expenses)
                                    {
                                        if (!expenses.Exists(x => x.Id.Equals(expense_alert_id)))
                                        {
                                            expenses.Add(captio_v3.GetExpense_v3_1(expense_alert_id));
                                        }
                                        if (expenses.Exists(x => x.Id.Equals(expense_alert_id)))
                                        {
                                            externalID_aux = expenses.Find(x => x.Id.Equals(expense_alert_id)).ExternalId.Trim();
                                            //result = result + " " + captio_v3.GetExpense_v3_1(expense_alert_id).ExternalId.TrimEnd();
                                        }
                                        else
                                        {
                                            externalID_aux = "";
                                        }
                                        result = result + " " + externalID_aux;
                                    }
                                }
                                result = result + "-";
                            }
                            result = result.Substring(0, result.Length - 1);
                        }
                    }
                }

            }
            return (result);
        }


        //
        // Contabilidad
        // Rutina per retornar concatenades totes les Alerts d'una Expense
        // id_expense -> id de la Expense
        // alerts --> alertes del Report
        private string ObtenerExpenseAlerts(ExpensesDTO_v3_1 expense, ExpensesAlertsDTO_v3_1[] alerts)
        {
            string result = "";
            string fecha = "";

            if (alerts != null)
            {
                foreach (ExpensesAlertsDTO_v3_1 alert in alerts)
                {
                    if (alert.Id == expense.Id)
                    {
                        fecha = DateTime.Parse(alert.Date).ToString("dd/MM/yyyy");
                        if (alert.Alerts.Length > 0)
                        {
                            foreach (ExpensesAlertsDTO_v3_1_Alerts alertaTicket in alert.Alerts)
                            {
                                result = result + "[" + fecha + "] " + alertaTicket.Name;
                                if (alertaTicket.Expenses != null)
                                {
                                    foreach (string expense_id in alertaTicket.Expenses)
                                    {
                                        result = result + " " + captio_v3.GetExpense_v3_1(expense_id).ExternalId.TrimEnd();
                                    }
                                }
                                result = result + "-";
                            }
                            result = result.Substring(0, result.Length - 1);
                        }
                    }
                }

            }
            return (result);
        }



        //
        // Contabilidad
        // Rutina per retornar concatenats tots els logs d'un Report
        private string ObtenerReportLogs(ReportsDTO_v3_1 report, WorkflowsDTO_v3_1 workflow, List<UsersDTO_v3_1> users, List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            string result = "";
            List<ReportsLogsDTO_v3_1_Logs> logs = new List<ReportsLogsDTO_v3_1_Logs>();
            string usuario = "";
            string fecha = "";
            string status = "";

            if (workflow == null)
            {
                workflow = captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + report.Workflow.Id + "\"").First();
            }

            logs.AddRange(captio_v3.GetReportsLogs_v3_1("\"" + "Id" + "\"" + ":" + "\"" + report.Id.ToString() + "\"").First().Logs);

            if (logs.Count > 0)
            {
                foreach (ReportsLogsDTO_v3_1_Logs log in logs)
                {
                    usuario = users.Find(x => x.Id.Equals(log.UserId)).Name;
                    fecha = DateTime.Parse(log.StatusDate).ToString();
                    switch (log.StatusId)
                    {
                        case "1":
                            // Borrador
                            status = "Borrador";
                            break;
                        case "2":
                            // Aprobación solicitada
                            status = "Aprobación solicitada";
                            break;
                        case "3":
                            // Rechazado
                            status = "Rechazado";
                            break;
                        case "4":
                            // Aprobado
                            status = "Aprobado";
                            break;
                        case "6":
                            // Liquidado
                            status = "Liquidado";
                            break;
                        default:
                            break;
                    }
                    if ((log.StepId != null) && (log.StatusId == "1"))
                    {
                        // Nom del usuari que rep el pas del workflow
                        if (Array.Exists(workflow.Steps, x => x.Id.Equals(log.StepId)))
                        {
                            status = Array.Find(workflow.Steps, x => x.Id.Equals(log.StepId)).Name;
                        }
                        else
                        {
                            status = Array.Find(workflowsCaptio.Find(s => Array.Exists(s.Steps, id => id.Id.Equals(log.StepId))).Steps, x => x.Id.Equals(log.StepId)).Name;
                        }
                    }
                    result = result + fecha + " - Usuario:" + usuario + " - Estado:" + status;
                    result = result + ((log.Comments == "") ? "" : "(" + log.Comments + ")") + "#";
                }
                result = result.Substring(0, result.Length - 1);
            }
            return (result);
        }



        //
        // Contabilidad
        // Rutina per retornar el DeductiveTaxPayable del SII d'una expense
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_DeductiveTaxPayable(List<SIIDTO_v3_1_VatDetail> vatDetail)
        {
            string result = "";

            foreach(SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.DeductibleTaxPayable))
                {
                    result = vat.DeductibleTaxPayable;
                    break;
                }
            }

            return (result);
        }


        //
        // Contabilidad
        // Rutina per retornar el EquivalenceSurchargeRate del SII d'una expense
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_EquivalenceSurchargeRate(List<SIIDTO_v3_1_VatDetail> vatDetail)
        {
            string result = "";

            foreach (SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.EquivalenceSurchargeRate))
                {
                    result = vat.EquivalenceSurchargeRate;
                    break;
                }
            }

            return (result);
        }


        //
        // Contabilidad
        // Rutina per retornar el EquivalenceSurchargePayable del SII d'una expense
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_EquivalenceSurchargePayable(List<SIIDTO_v3_1_VatDetail> vatDetail)
        {
            string result = "";

            foreach (SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.EquivalenceSurchargePayable))
                {
                    result = vat.EquivalenceSurchargePayable;
                    break;
                }
            }

            return (result);
        }



        //
        // Contabilidad
        // Rutina per retornar el TaxRate del SII d'una expense
        // int i --> Indica quin ha de tornar dels que hagi (el primer, el segon, el tercer, ...)
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_TaxRate(List<SIIDTO_v3_1_VatDetail> vatDetail, int i, string invoiceTotalAmount)
        {
            string result = "";
            int contador = 0;

            foreach (SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.TaxRate))
                {
                    contador++;
                    if (contador == i)
                    {
                        result = vat.TaxRate;
                        break;
                    }
                }
            }
            return (result);
        }

        //
        // Contabilidad
        // Rutina per retornar el TaxPayable del SII d'una expense
        // int i --> Indica quin ha de tornar dels que hagi (el primer, el segon, el tercer, ...)
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_TaxPayable(List<SIIDTO_v3_1_VatDetail> vatDetail, int i, string invoiceTotalAmount)
        {
            string result = "";
            int contador = 0;
            decimal taxPayable = 0;
            string taxRate = "";

            foreach (SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.TaxPayable))
                {
                    contador++;
                    if (contador == i)
                    {
                        result = vat.TaxPayable;
                        taxRate = vat.TaxRate;
                        break;
                    }
                }
            }
            
            if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(taxRate) && !String.IsNullOrEmpty(invoiceTotalAmount))
            {
                taxPayable = decimal.Parse(result, CultureInfo.GetCultureInfo("en-US"));
                if (!(taxPayable > 0))
                {
                    decimal totalFactura = decimal.Parse(invoiceTotalAmount, CultureInfo.GetCultureInfo("en-US"));
                    decimal taxRate_decimal = decimal.Parse(taxRate, CultureInfo.GetCultureInfo("en-US"));
                    result = (totalFactura * (taxRate_decimal / 100)).ToString("0.00", CultureInfo.GetCultureInfo("en-US"));
                }
            }
            return (result);
        }

        //
        // Contabilidad
        // Rutina per retornar el TaxableBase del SII d'una expense
        // int i --> Indica quin ha de tornar dels que hagi (el primer, el segon, el tercer, ...)
        // vatDetail --> vatDetail de la expense del SII
        public string ObtenerSII_TaxableBase(List<SIIDTO_v3_1_VatDetail> vatDetail, int i, string invoiceTotalAmount)
        {
            string result = "";
            int contador = 0;
            decimal taxableBase = 0;
            string taxRate = "";

            foreach (SIIDTO_v3_1_VatDetail vat in vatDetail)
            {
                if (!String.IsNullOrEmpty(vat.TaxableBase))
                {
                    contador++;
                    if (contador == i)
                    {
                        result = vat.TaxableBase;
                        taxRate = vat.TaxRate;
                        break;
                    }
                }
            }

            if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(taxRate) && !String.IsNullOrEmpty(invoiceTotalAmount))
            {
                taxableBase = decimal.Parse(result, CultureInfo.GetCultureInfo("en-US"));
                if (!(taxableBase > 0))
                {
                    decimal totalFactura = decimal.Parse(invoiceTotalAmount, CultureInfo.GetCultureInfo("en-US"));
                    decimal taxRate_decimal = decimal.Parse(taxRate, CultureInfo.GetCultureInfo("en-US"));
                    result = ((totalFactura) - (totalFactura * (taxRate_decimal / 100))).ToString("0.00", CultureInfo.GetCultureInfo("en-US"));
                }
            }

            return (result);
        }


        ////
        //// Obtiene el workflow de Captio del id
        //// Devuelve null si no existe
        //public WorkflowsDTO_v3_1 GetWorkflow(string id, ref List<WorkflowsDTO_v3_1> workflowsCaptio)
        //{
        //    WorkflowsDTO_v3_1 workflow;

        //    // Obtener el usuario del Ticket
        //    if (!workflowsCaptio.Exists(x => x.Id.Equals(id)))
        //    {
        //        workflowsCaptio.AddRange(captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id + "\""));
        //    }
        //    try
        //    {
        //        workflow = workflowsCaptio.Find(x => x.Id.Equals(id));
        //        return (workflow);
        //    }
        //    catch
        //    {
        //        // Log
        //        Log.Error("Workflow [" + id + "] inexistente en Captio");
        //        return (null);
        //    }
        //}


        //
        // Contabilidad
        // Obtiene el workflow de Captio de un informe
        // Devuelve null si no existe o no tiene workflow
        public WorkflowsDTO_v3_1 ObtenerWorkflowInforme(ReportsDTO_v3_1 informe, ref List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            if ((informe.Workflow != null) && (informe.Workflow.Id != null))
            {

                return (GetWorkflow(informe.Workflow.Id, ref workflowsCaptio));
            }
            return (null);
        }





        //
        // Contabilidad
        // Devuelve el nombre de la etapa de aprobación de un informe
        private string ObtenerEtapaWorkflowInforme(ReportsDTO_v3_1 informe, List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            string result = "";

            if ((informe.Workflow != null) && (informe.Workflow.Id != null) && (informe.Workflow.Step != null) && (informe.Workflow.Step.Id != null))
            {
                WorkflowsDTO_v3_1 workflow = new WorkflowsDTO_v3_1();
                workflow = workflowsCaptio.Find(x => x.Id.Equals(informe.Workflow.Id));
                if (workflow == null)
                {
                    workflow = captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + informe.Workflow.Id + "\"").First();
                }

                result = (Array.Exists(workflow.Steps, x => x.Id.Equals(informe.Workflow.Step.Id))) ? Array.Find(workflow.Steps, x => x.Id.Equals(informe.Workflow.Step.Id)).Name : "";
            }
            return (result);
        }



        //
        // Contabilidad
        // Devuelve el codigo del usuario (EmployeeCode) aprobador de la última etapa de un workflow
        private string ObtenerCodigoUsuarioAprobador(string id_workflow, List<UsersDTO_v3_1> users, List<WorkflowsDTO_v3_1> workflowsCaptio)
        {
            string result = "";
            try
            {
                //WorkflowsDTO_v3_1 workflow = utils.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id_workflow + "\"").ToList().FirstOrDefault();
                WorkflowsDTO_v3_1 workflow = new WorkflowsDTO_v3_1();
                workflow = workflowsCaptio.Find(x => x.Id.Equals(id_workflow));
                if (workflow == null)
                {
                    workflow = captio_v3.GetWorkflows_v3_1("\"" + "Id" + "\"" + ":" + "\"" + id_workflow + "\"").First();
                }
                if (workflow != null)
                {
                    if (workflow.Steps.Count() == 0)
                    {
                        // Log
                        Log.Error(" -> Workflow sin etapas de aprobación. ID : " + id_workflow);
                    }
                    else if (workflow.Steps.Count() > 1)
                    {
                        // Log
                        Log.Info(" -> Workflow con más de una etapa de aprobación. ID : " + id_workflow);
                        Log.Info(" -> Selecciónamos el código de usuario de la última etapa");
                        result = users.Find(x => x.Id.Equals(workflow.Steps[workflow.Steps.Count() - 1].SupervisorId)).UserOptions.EmployeeCode;
                    }
                    else
                    {
                        result = users.Find(x => x.Id.Equals(workflow.Steps[0].SupervisorId)).UserOptions.EmployeeCode;
                    }
                }
                else
                {
                    Log.Error(" -> Error al obtener el Workflow.  Workflow inexistente");
                }
            }
            catch (Exception ex)
            {
                // Log
                Log.Error(" -> Error al obtener el Código del Usuario del aprobador del ticket");
                Log.Error(ex.Message);
            }
            return (result);
        }

        public string ObtindreInicialsNom(UsersDTO_v3_1 user)
        {
            string result = "";

            string nomComplert = (user == null) ? "" : ((String.IsNullOrEmpty(user.Name)) ? "" : user.Name.Trim());

            string[] nomSeparat = nomComplert.Split(" ");

            foreach(string nom in nomSeparat)
            {
                if (!String.IsNullOrEmpty(nom))
                {
                    result = result + nom[0];
                }
            }

            return (result);
        }


        public string ObtindreValorCampPersonalitzatUsuari(UsersDTO_v3_1 user, string nomCampPersonalitzat)
        {
            string result = "";

            if (user != null)
            {
                List<UsersDTO_v3_1_CustomFields> userCustomFields = new List<UsersDTO_v3_1_CustomFields>();
                userCustomFields.AddRange(user.CustomFields);

                if (userCustomFields.Exists(x => x.Name.Equals(nomCampPersonalitzat)))
                {
                    result = (String.IsNullOrEmpty((userCustomFields.Find(x => x.Name.Equals(nomCampPersonalitzat))).Value)) ? "" : (userCustomFields.Find(x => x.Name.Equals(nomCampPersonalitzat))).Value;
                }
            }

            return (result);
        }



        // Devuelve la suma total de FinalAmount de los tickets de gasto
        public decimal SumaTotalFinalAmountExpenses(List<ExpensesDTO_v3_1> expenses)
        {
            decimal total_expenses = 0;

            foreach (ExpensesDTO_v3_1 expense in expenses)
            {
                try
                {
                    total_expenses = total_expenses + decimal.Parse(expense.FinalAmount.Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                }
                catch (Exception ex)
                {
                    // Log
                    Log.Error("Ticket no incluido en la suma de FinalAmount : " + expense.ExternalId);
                    Log.Error(ex.Message);
                }
            }

            return (total_expenses);
        }


        public string ObtenerValueCampoPersonalizado(ExpensesDTO_v3_1 expense, string id_campoPersonalizado, List<CustomFieldsDTO_v3_1> customFieldsCaptio)
        {
            string result = "";
            List<CustomFieldsDTO_v3_1> llista_CFs = new List<CustomFieldsDTO_v3_1>();
            string id_CF = "";
            string externalNameCampoPersonalizado = customFieldsCaptio.Find(x => x.Id.Equals(id_campoPersonalizado)).ExternalName;

            if (customFieldsCaptio.Exists(x => x.Type.Equals("1") && x.ExternalName.Equals(externalNameCampoPersonalizado)))
            {
                llista_CFs.AddRange(customFieldsCaptio.FindAll(x => x.Type.Equals("1") && x.ExternalName.Equals(externalNameCampoPersonalizado)));
            }

            foreach (CustomFieldsDTO_v3_1 camp_personalitzat in llista_CFs)
            {
                if (Array.Exists(expense.CustomFields, x => x.Id.Equals(camp_personalitzat.Id)))
                {
                    id_CF = camp_personalitzat.Id;
                    break;
                }
            }

            if (!String.IsNullOrEmpty(id_CF))
            {
                result = (Array.Exists(expense.CustomFields, x => x.Id.Equals(id_CF))) ? (String.IsNullOrEmpty(Array.Find(expense.CustomFields, x => x.Id.Equals(id_CF)).Value) ? "" : Array.Find(expense.CustomFields, x => x.Id.Equals(id_CF)).Value) : "";
            }
            return (result);
        }


        public string ObtenerCodeValueCampoPersonalizado(ExpensesDTO_v3_1 expense, string id_campoPersonalizado, List<CustomFieldsDTO_v3_1> customFieldsCaptio)
        {
            string result = "";
            List<CustomFieldsDTO_v3_1> llista_CFs = new List<CustomFieldsDTO_v3_1>();
            string id_CF = "";
            string externalNameCampoPersonalizado = customFieldsCaptio.Find(x => x.Id.Equals(id_campoPersonalizado)).ExternalName;

            if (customFieldsCaptio.Exists(x => x.Type.Equals("1") && x.ExternalName.Equals(externalNameCampoPersonalizado)))
            {
                llista_CFs.AddRange(customFieldsCaptio.FindAll(x => x.Type.Equals("1") && x.ExternalName.Equals(externalNameCampoPersonalizado)));
            }

            foreach (CustomFieldsDTO_v3_1 camp_personalitzat in llista_CFs)
            {
                if (Array.Exists(expense.CustomFields, x => x.Id.Equals(camp_personalitzat.Id)))
                {
                    id_CF = camp_personalitzat.Id;
                    break;
                }
            }

            if (!String.IsNullOrEmpty(id_CF))
            {
                result = (Array.Exists(expense.CustomFields, x => x.Id.Equals(id_CF))) ? (String.IsNullOrEmpty(Array.Find(expense.CustomFields, x => x.Id.Equals(id_CF)).CodeValue) ? "" : Array.Find(expense.CustomFields, x => x.Id.Equals(id_CF)).CodeValue) : "";
            }
            return (result);
        }


        public string ObtenerInicialesUsuario (UsersDTO_v3_1 user)
        {
            string result = "";

            string nombreUsuario = (user == null) ? "" : ((String.IsNullOrEmpty(user.Name)) ? "" : user.Name.Trim());

            foreach(string texto in nombreUsuario.Split(" "))
            {
                if (!String.IsNullOrEmpty(texto.Trim()))
                {
                    result = result + texto.Trim()[0];
                }
            }
            return (result);
        }


        public string GetPaymentMethod_Name(ExpensesDTO_v3_1 expense)
        {
            string result = ((expense != null) && (expense.PaymentMethod != null) && (!String.IsNullOrEmpty(expense.PaymentMethod.Name))) ? (expense.PaymentMethod.Name) : "";

            return (result);
        }


        public string GetCategoryName_Expense(ExpensesDTO_v3_1 expense)
        {
            string result = ((expense != null) && (expense.Category != null) && (!String.IsNullOrEmpty(expense.Category.Name))) ? (expense.Category.Name) : "";

            return (result);
        }


        public string GetCategoryCode_Expense(ExpensesDTO_v3_1 expense, List<CompaniesCategoriesDTO_v3_1> categoriesCaptio)
        {

            if ((expense == null) || (expense.Category == null))
            {
                return ("");
            }
            else
            {
                string result = (!String.IsNullOrEmpty(expense.Category.Code)) ? (expense.Category.Code) : "";
                if (String.IsNullOrEmpty(result))
                {
                    if ((categoriesCaptio.Exists(x => x.Id.Equals(expense.Category.Code))) && (!String.IsNullOrEmpty(categoriesCaptio.Find(x => x.Id.Equals(expense.Category.Code)).Code)))
                    {
                        result = categoriesCaptio.Find(x => x.Id.Equals(expense.Category.Code)).Code;
                    }
                }
                return (result);
            }
        }



        public string GetTasaKM_Expense(ExpensesDTO_v3_1 expense)
        {

            if ((expense == null) || (expense.MileageInfo == null) || (!expense.IsMileage.ToLower().Equals("true")))
            {
                return ("");
            }
            else
            {
                string result = (!String.IsNullOrEmpty(expense.MileageInfo.MileageRate)) ? (expense.MileageInfo.MileageRate) : "";
                return (result);
            }
        }

    }
}
