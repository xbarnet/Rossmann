
namespace CaptioB2it.Entidades
{
    // /SII
    public class SIIDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string SIIStatus { get; set; }
        public string SIIStatusDate { get; set; }
        public SIIDTO_v3_1_ExpensesSIIData[] ExpensesSIIData { get; set; }
    }
    public class SIIDTO_v3_1_ExpensesSIIData
    {
        public string Id { get; set; }
        public string ExpenseId { get; set; }
        public string ExpenseExternalId { get; set; }
        public string SIIStatus { get; set; }
        public string SIIStatusDate { get; set; }
        public string ApprovalDate { get; set; }
        public string IsFirstSemester2017 { get; set; }
        public string SpanishNIF { get; set; }
        public string IssuerTypeID { get; set; }
        public string IssuerID { get; set; }
        public string CompanyName { get; set; }
        public string Year { get; set; }
        public string Period { get; set; }
        public string InvoiceNumber { get; set; }
        public string TransactionDate { get; set; }
        public string IssueDate { get; set; }
        public string CollectionPaymentDate { get; set; }
        public string InvoiceTypeCode { get; set; }
        public string SpecialSystemIdentificationCode { get; set; }
        public string TransactionDescription { get; set; }
        public string InvoiceTotalAmount { get; set; }
        public string AEATErrorMessage { get; set; }
        public SIIDTO_v3_1_VatDetail[] VatDetail { get; set; }
        public string CustomerVAT { get; set; }
        public string CountryCode { get; set; }
        public string IGIC { get; set; }
        public string IPSI { get; set; }
    }
    public class SIIDTO_v3_1_VatDetail
    {
        public string TaxRate { get; set; }
        public string TaxableBase { get; set; }
        public string TaxPayable { get; set; }
        public string DeductibleTaxPayable { get; set; }
        public string EquivalenceSurchargeRate { get; set; }
        public string EquivalenceSurchargePayable { get; set; }
    }

    public class SIIReportExportedDTO_v3_1_PUT
    {
        public string Id { get; set; }
    }
}
