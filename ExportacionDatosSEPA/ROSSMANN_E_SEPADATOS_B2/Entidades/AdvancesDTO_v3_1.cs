
namespace CaptioB2it.Entidades
{
    // /Advances
    public class AdvancesDTO_v3_1
    {
        public AdvancesDTO_v3_1_CustomFields[] CustomFields { get; set; }
        public AdvancesDTO_v3_1_Currencies[] Currencies { get; set; }
        public AdvancesDTO_v3_1_Workflow Workflow { get; set; }
        public AdvancesDTO_v3_1_Report Report { get; set; }
        public AdvancesDTO_v3_1_User User { get; set; }
        public AdvancesDTO_v3_1_Cash Cash { get; set; }
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public string RequestedDate { get; set; }
        public string PickUpDate { get; set; }
        public string RefundDate { get; set; }
        public string DeliveryDate { get; set; }
        public string ConcessionDate { get; set; }
        public string Amount { get; set; }
        public string StepId { get; set; }
        public string CurrencyId { get; set; }
        public string IsDisposal { get; set; }
    }
    public class AdvancesDTO_v3_1_CustomFields
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CodeValue { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }
    public class AdvancesDTO_v3_1_Currencies
    {
        public string Id { get; set; }
        public string Active { get; set; }
        public string Pending { get; set; }
        public string IsUserEditedExchangeRate { get; set; }
        public string ExchangeRate { get; set; }
        public string LineOrigin { get; set; }
        public string PreviousId { get; set; }
        public AdvancesDTO_v3_1_Currencies_Source Source { get; set; }
        public AdvancesDTO_v3_1_Currencies_Target Target { get; set; }
    }
    public class AdvancesDTO_v3_1_Currencies_Source
    {
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string Amount { get; set; }
        public string ReturnedAmount { get; set; }
        public string RemainingAmount { get; set; }
    }
    public class AdvancesDTO_v3_1_Currencies_Target
    {
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string Amount { get; set; }
        public string ReturnedAmount { get; set; }
        public string RemainingAmount { get; set; }
    }
    public class AdvancesDTO_v3_1_Workflow
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
    }
    public class AdvancesDTO_v3_1_Report
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
    }
    public class AdvancesDTO_v3_1_User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class AdvancesDTO_v3_1_Cash
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class AdvancesDTO_v3_1_POST
    {
        public string UserId { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public string WorkflowId { get; set; }
        public AdvancesDTO_v3_1_POST_Currencies[] Currencies { get; set; }
        public AdvancesDTO_v3_1_POST_Travel Travel { get; set; }
        public AdvancesDTO_v3_1_POST__CustomFields[] CustomFields { get; set; }
    }
    public class AdvancesDTO_v3_1_POST_Currencies
    {
        public string CurrencyId { get; set; }
        public string Amount { get; set; }
    }
    public class AdvancesDTO_v3_1_POST_Travel
    {
        public string Days { get; set; }
        public string DailyAmount { get; set; }
    }
    public class AdvancesDTO_v3_1_POST__CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    // /Advances/Approve
    public class AdvancesApproveDTO_v3_1_PUT
    {
        public string Id { get; set; }
    }

}
