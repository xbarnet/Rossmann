
namespace CaptioB2it.Entidades
{
    // /Reports
    public class ReportsDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }
        public string CreationDate { get; set; }
        public string LiquidationDate { get; set; }
        public string ReimbursableAmount { get; set; }
        public string GenerateAdvanceSettlement { get; set; }
        public string UrlKey { get; set; }
        public ReportsDTO_v3_1_GeneratedAdvance GeneratedAdvance { get; set; }
        public string AvailableAlerts { get; set; }
        public ReportsDTO_v3_1_User User { get; set; }
        public ReportsDTO_v3_1_Amount Amount { get; set; }
        public ReportsDTO_v3_1_CustomField[] CustomFields { get; set; }
        public ReportsDTO_v3_1_Workflow Workflow { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    public class ReportsDTO_v3_1_GeneratedAdvance
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
    }
    public class ReportsDTO_v3_1_User
    {
        public string Id { get; set; }
    }
    public class ReportsDTO_v3_1_Amount
    {
        public string Value { get; set; }
        public ReportsDTO_v3_1_Amount_Currency Currency { get; set; }
    }
    public class ReportsDTO_v3_1_Amount_Currency
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string ISOCode { get; set; }
    }
    public class ReportsDTO_v3_1_CustomField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CodeValue { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }
    public class ReportsDTO_v3_1_Workflow
    {
        public string Id { get; set; }
        public ReportsDTO_v3_1_Workflow_Step Step { get; set; }
    }
    public class ReportsDTO_v3_1_Workflow_Step
    {
        public string Id { get; set; }
    }

    public class ReportsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    public class ReportsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ReportsDTO_v3_1_PATCH_Workflow Workflow { get; set; }
    }
    public class ReportsDTO_v3_1_PATCH_Workflow
    {
        public string Id { get; set; }
    }

    public class ReportsDTO_v3_1_POST
    {
        public string Name { get; set; }
        public ReportsDTO_v3_1_POST_User User { get; set; }
        public ReportsDTO_v3_1_POST_Workflow Workflow { get; set; }
        public ReportsDTO_v3_1_POST_CustomField[] CustomFields { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public ReportsDTO_v3_1_POST_Rule Rule { get; set; }
    }
    public class ReportsDTO_v3_1_POST_User
    {
        public string Id { get; set; }
    }
    public class ReportsDTO_v3_1_POST_Workflow
    {
        public string Id { get; set; }
    }
    public class ReportsDTO_v3_1_POST_CustomField
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
    public class ReportsDTO_v3_1_POST_Rule
    {
        public string Id { get; set; }
    }

    // /Reports/Logs
    public class ReportsLogsDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }
        public ReportsLogsDTO_v3_1_Logs[] Logs { get; set; }
    }
    public class ReportsLogsDTO_v3_1_Logs
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string StatusId { get; set; }
        public string StatusDate { get; set; }
        public string Comments { get; set; }
        public string StepId { get; set; }
        public string DelegantUserId { get; set; }
    }

    // /Reports/Alerts
    public class ReportsAlertsDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }
        public ReportsAlertsDTO_v3_1_Alerts[] Alerts { get; set; }
    }
    public class ReportsAlertsDTO_v3_1_Alerts
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    // /Reports/Reject
    public class ReportsRejectDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Comment { get; set; }
        public string CC { get; set; }
        public bool SendEmail { get; set; }
    }

    // /Reports/Approve
    public class ReportsApproveDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Comment { get; set; }
        public string CC { get; set; }
        public bool SendEmail { get; set; }
    }

    // /Reports/RemoveExpenses
    public class ReportsRemoveExpensesDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public ReportsRemoveExpensestDTO_v3_1_PUT_Expenses[] Expenses { get; set; }
    }
    public class ReportsRemoveExpensestDTO_v3_1_PUT_Expenses
    {
        public string Id { get; set; }
        public string Comments { get; set; }
    }

    // /Reports/RequestApprove
    public class ReportsRequestApproveDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Comment { get; set; }
        public string CC { get; set; }
        public bool SendEmail { get; set; }
    }

    // /Reports/AddAdvances
    public class ReportsAddAdvancesDTO_v3_1_POST
    {
        public string Id { get; set; }
        public ReportsAddAdvancesDTO_v3_1_POST_Advances[] Advances { get; set; }
    }
    public class ReportsAddAdvancesDTO_v3_1_POST_Advances
    {
        public string Id { get; set; }
    }

    // /Reports/AddExpenses
    public class ReportsAddExpensestDTO_v3_1_POST
    {
        public string Id { get; set; }
        public ReportsAddExpensestDTO_v3_1_POST_Expenses[] Expenses { get; set; }
    }
    public class ReportsAddExpensestDTO_v3_1_POST_Expenses
    {
        public string Id { get; set; }
        public string Comments { get; set; }
    }

    // /Reports/AddComplementaryExpenses
    public class ReportsAddComplementaryExpensesDTO_v3_1_POST
    {
        public string Id { get; set; }
        public ReportsAddComplementaryExpensesDTO_v3_1_POST_Expenses[] Expenses { get; set; }
    }
    public class ReportsAddComplementaryExpensesDTO_v3_1_POST_Expenses
    {
        public string Id { get; set; }
        public string Comments { get; set; }
    }

    // /Reports/Periods
    public class ReportsPeriodsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    // /Reports/CustomFields
    public class ReportsCustomFieldsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public ReportsCustomFieldsDTO_v3_1_PATCH_CustomFields[] CustomFields { get; set; }
    }
    public class ReportsCustomFieldsDTO_v3_1_PATCH_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

}
