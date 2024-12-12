
namespace CaptioB2it.Entidades
{
    // /Workflows
    public class WorkflowsDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string CreatedDate { get; set; }
        public string Type { get; set; }
        public string TypeActivationStep { get; set; }
        public WorkflowsDTO_v3_1_Steps[] Steps { get; set; }
        public WorkflowsDTO_v3_1_CustomFields[] CustomFields { get; set; }
    }
    public class WorkflowsDTO_v3_1_Steps
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string Description { get; set; }
        public string MaxValue { get; set; }
        public WorkflowsDTO_v3_1_Steps_Permissions Permissions { get; set; }
        public string SupervisorId { get; set; }
    }
    public class WorkflowsDTO_v3_1_Steps_Permissions
    {
        public string Alerts { get; set; }
        public string DeleteReceipt { get; set; }
        public string EditReceipt { get; set; }
        public string Email { get; set; }
        public string SkipStep { get; set; }
        public string AdvancesSettings { get; set; }
    }
    public class WorkflowsDTO_v3_1_CustomFields
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CodeValue { get; set; }
        public string Value { get; set; }
    }

    public class WorkflowsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    public class WorkflowsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public WorkflowsDTO_v3_1_PATCH_CustomFields[] CustomFields { get; set; }
    }
    public class WorkflowsDTO_v3_1_PATCH_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class WorkflowsDTO_v3_1_POST
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeActivationStep { get; set; }
        public WorkflowsDTO_v3_1_POST_CustomFields[] CustomFields { get; set; }
        public WorkflowsDTO_v3_1_POST_Steps[] Steps { get; set; }
    }
    public class WorkflowsDTO_v3_1_POST_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
        public string ItemId { get; set; }
    }
    public class WorkflowsDTO_v3_1_POST_Steps
    {
        public WorkflowsDTO_v3_1_POST_Steps_Languages[] Languages { get; set; }
        public string MaxValue { get; set; }
        public string SupervisorId { get; set; }
        public string ExternalValidation { get; set; }
        public string RejectAction { get; set; }
        public WorkflowsDTO_v3_1_POST_Steps_Permissions Permissions { get; set; }
        public string ActiveAllAlerts { get; set; }
    }
    public class WorkflowsDTO_v3_1_POST_Steps_Languages
    {
        public string Code { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }
    public class WorkflowsDTO_v3_1_POST_Steps_Permissions
    {
        public string Alerts { get; set; }
        public string DeleteReceipt { get; set; }
        public string EditReceipt { get; set; }
        public string Email { get; set; }
        public string SkipStep { get; set; }
        public string AdvancesSettings { get; set; }
    }

    public class WorkflowsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    // /Workflows/Steps/Alerts
    public class WorkflowsStepsAlertsDTO_v3_1
    {
        public string Id { get; set; }
        public WorkflowsStepsAlertsDTO_v3_1_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsAlertsDTO_v3_1_Steps
    {
        public string Id { get; set; }
        public WorkflowsStepsAlertsDTO_v3_1_Steps_StepAlerts[] StepAlerts { get; set; }
    }
    public class WorkflowsStepsAlertsDTO_v3_1_Steps_StepAlerts
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
    }

    public class WorkflowsStepsAlertsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public WorkflowsStepsAlertsDTO_v3_1_PATCH_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsAlertsDTO_v3_1_PATCH_Steps
    {
        public string Id { get; set; }
        public WorkflowsStepsAlertsDTO_v3_1_PATCH_Steps_Alerts[] Alerts { get; set; }
    }
    public class WorkflowsStepsAlertsDTO_v3_1_PATCH_Steps_Alerts
    {
        public string Id { get; set; }
        public string Active { get; set; }
    }

    // /Workflows/Steps/Supervisor
    public class WorkflowsStepsSupervisorDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public WorkflowsStepsSupervisorDTO_v3_1_PUT_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsSupervisorDTO_v3_1_PUT_Steps
    {
        public string Id { get; set; }
        public string ExternalValidation { get; set; }
        public string SupervisorId { get; set; }
    }

    // /Workflows/Steps/SupervisorWithoutEmail
    public class WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsSupervisorWithoutEmailDTO_v3_1_PUT_Steps
    {
        public string Id { get; set; }
        public string ExternalValidation { get; set; }
        public string SupervisorId { get; set; }
    }

    // /Workflows/Steps
    public class WorkflowsStepsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public WorkflowsStepsDTO_v3_1_PATCH_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsDTO_v3_1_PATCH_Steps
    {
        public string Id { get; set; }
        public string MaxValue { get; set; }
        public string ActiveAllAlerts { get; set; }
        public WorkflowsStepsDTO_v3_1_PATCH_Steps_Permissions Permissions { get; set; }
        public string RejectAction { get; set; }
    }
    public class WorkflowsStepsDTO_v3_1_PATCH_Steps_Permissions
    {
        public string Alerts { get; set; }
        public string DeleteReceipt { get; set; }
        public string EditReceipt { get; set; }
        public string Email { get; set; }
        public string SkipStep { get; set; }
        public string AdvancesSettings { get; set; }
    }

    public class WorkflowsStepsDTO_v3_1_POST
    {
        public string Id { get; set; }
        public WorkflowsStepsDTO_v3_1_POST_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsDTO_v3_1_POST_Steps
    {
        public WorkflowsStepsDTO_v3_1_POST_Steps_Language[] Languages { get; set; }
        public string MaxValue { get; set; }
        public string SupervisorId { get; set; }
        public string ExternalValidation { get; set; }
        public string RejectAction { get; set; }
        public WorkflowsStepsDTO_v3_1_POST_Steps_Permissions Permissions { get; set; }
        public string ActiveAllAlerts { get; set; }
    }
    public class WorkflowsStepsDTO_v3_1_POST_Steps_Language
    {
        public string Code { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }
    public class WorkflowsStepsDTO_v3_1_POST_Steps_Permissions
    {
        public string Alerts { get; set; }
        public string DeleteReceipt { get; set; }
        public string EditReceipt { get; set; }
        public string Email { get; set; }
        public string SkipStep { get; set; }
        public string AdvancesSettings { get; set; }
    }

    // /Workflows/Steps/Resources
    public class WorkflowsStepsResourcesDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public WorkflowsStepsResourcesDTO_v3_1_PATCH_Steps[] Steps { get; set; }
    }
    public class WorkflowsStepsResourcesDTO_v3_1_PATCH_Steps
    {
        public string Id { get; set; }
        public WorkflowsStepsResourcesDTO_v3_1_PATCH_Steps_Resources[] Resources { get; set; }
    }
    public class WorkflowsStepsResourcesDTO_v3_1_PATCH_Steps_Resources
    {
        public string Code { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }

}

