namespace CaptioB2it.Entidades
{
    // /Expenses
    // Get Expenses. To get the image you have to link "http://api-storage.captio.net/api/GetFile?key=" and the UrlKey
    public class ExpensesDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Date { get; set; }
        public string CreationDate { get; set; }
        public string Merchant { get; set; }
        public ExpensesDTO_v3_1_Amount ExpenseAmount { get; set; }
        public ExpensesDTO_v3_1_Amount FinalAmount { get; set; }
        public string Comment { get; set; }
        public string IsMileage { get; set; }
        public string Reconciled { get; set; }
        public string ReconciledPayments { get; set; }
        public string UrlKey { get; set; }
        public ExpensesDTO_v3_1_Attachments[] Attachments { get; set; }
        public string RepositoryId { get; set; }
        public ExpensesDTO_v3_1_Category Category { get; set; }
        public ExpensesDTO_v3_1_PaymentMethod PaymentMethod { get; set; }
        public ExpensesDTO_v3_1_Invoice Invoice { get; set; }
        public string InvoiceReviewed { get; set; }
        public string VATExempt { get; set; }
        public string VATExemptAmount { get; set; }
        public ExpensesDTO_v3_1_VATRates[] VATRates { get; set; }
        public string TIN { get; set; }
        public string InvoiceNumber { get; set; }
        public string UUID { get; set; }
        public ExpensesDTO_v3_1_CustomField[] CustomFields { get; set; }
        public ExpensesDTO_v3_1_User User { get; set; }
        public ExpensesDTO_v3_1_Report Report { get; set; }
        public ExpensesDTO_v3_1_Payment Payment { get; set; }
        public ExpensesDTO_v3_1_MileageInfo MileageInfo { get; set; }
    }
    public class ExpensesDTO_v3_1_Amount
    {
        public string Value { get; set; }
        public ExpensesDTO_v3_1_Amount_Currency Currency { get; set; }
    }
    public class ExpensesDTO_v3_1_Amount_Currency
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string ISOCode { get; set; }
    }
    public class ExpensesDTO_v3_1_Attachments
    {
        public string UrlKey { get; set; }
    }
    public class ExpensesDTO_v3_1_Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string VatPercent { get; set; }
        public string Deductible { get; set; }
        public string DeductiblePercentage { get; set; }
        public ExpensesDTO_v3_1_Category_Parent Parent { get; set; }
    }
    public class ExpensesDTO_v3_1_Category_Parent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string VatPercent { get; set; }
    }
    public class ExpensesDTO_v3_1_PaymentMethod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IsReimbursable { get; set; }
        public string IsReconcilable { get; set; }
    }
    public class ExpensesDTO_v3_1_Invoice
    {
        public string Number { get; set; }
        public string Provider { get; set; }
        public ExpensesDTO_v3_1_Invoice_Lines[] Lines { get; set; }
        public string Deductible { get; set; }
    }
    public class ExpensesDTO_v3_1_Invoice_Lines
    {
        public string TaxBase { get; set; }
        public string VAT { get; set; }
    }
    public class ExpensesDTO_v3_1_VATRates
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
        public string Percentage { get; set; }
    }
    public class ExpensesDTO_v3_1_CustomField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CodeValue { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }
    public class ExpensesDTO_v3_1_User
    {
        public string Id { get; set; }
    }
    public class ExpensesDTO_v3_1_Report
    {
        public string Id { get; set; }
    }
    public class ExpensesDTO_v3_1_Payment
    {
        public string Id { get; set; }
    }
    public class ExpensesDTO_v3_1_MileageInfo
    {
        public string KMGroup { get; set; }
        public string PredefinedJourney { get; set; }
        public string MileageRate { get; set; }
        public string Distance { get; set; }
    }

    public class ExpensesDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    // /Expenses/Alerts
    public class ExpensesAlertsDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Date { get; set; }
        public ExpensesAlertsDTO_v3_1_Report Report { get; set; }
        public ExpensesAlertsDTO_v3_1_Alerts[] Alerts { get; set; }
    }
    public class ExpensesAlertsDTO_v3_1_Report
    {
        public string Id { get; set; }
    }
    public class ExpensesAlertsDTO_v3_1_Alerts
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] Expenses { get; set; }
    }

    // /Expenses/Payments
    public class ExpensesPaymentsDTO_v3_1
    {
        public string Id { get; set; }
        public ExpensesPaymentsDTO_v3_1_Payments[] Payments { get; set; }
    }
    public class ExpensesPaymentsDTO_v3_1_Payments
    {
        public string Id { get; set; }
    }

    // /Expenses/Incomplete
    // Get Incomplete Expenses. To get the image you have to link "http://api-storage.captio.net/api/GetFile?key=" and the UrlKey
    public class ExpensesIncompleteDTO_v3_1
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string CreationDate { get; set; }
        public string Merchant { get; set; }
        public ExpensesIncompleteDTO_v3_1_ExpenseAmount ExpenseAmount { get; set; }
        public string UrlKey { get; set; }
        public ExpensesIncompleteDTO_v3_1_Attachments[] Attachments { get; set; }
        public ExpensesIncompleteDTO_v3_1_Category Category { get; set; }
        public ExpensesIncompleteDTO_v3_1_PaymentMethod PaymentMethod { get; set; }
        public ExpensesIncompleteDTO_v3_1_User User { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_ExpenseAmount
    {
        public string Value { get; set; }
        public ExpensesIncompleteDTO_v3_1_ExpenseAmount_Currency Currency { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_ExpenseAmount_Currency
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string ISOCode { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_Attachments
    {
        public string UrlKey { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_PaymentMethod
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class ExpensesIncompleteDTO_v3_1_User
    {
        public string Id { get; set; }
    }

    // /Expenses/CertifiedUrls
    //Get certified urls.To get the attachment you have to link "http://api-storage.captio.net/api/GetFile?key=" and the KeyUrl
    public class ExpensesCertifiedUrlsDTO_v3_1
    {
        public string Id { get; set; }
        public string UrlKey { get; set; }
        public ExpensesCertifiedUrlsDTO_v3_1_Attachments[] Attachments { get; set; }
    }
    public class ExpensesCertifiedUrlsDTO_v3_1_Attachments
    {
        public string UrlKey { get; set; }
    }

    // /Expenses/Mileage
    public class ExpensesMileageDTO_v3_1_POST
    {
        public string UserId { get; set; }
        public string RepositoryId { get; set; }
        public string PaymentMethodId { get; set; }
        public string ProjectId { get; set; }
        public string CategoryId { get; set; }
        public string RouteId { get; set; }
        public string KmGroupId { get; set; }
        public string Distance { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public ExpensesMileageDTO_v3_1_POST_CustomFields[] CustomFields { get; set; }
    }
    public class ExpensesMileageDTO_v3_1_POST_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }

    // /Expenses/Manually
    public class ExpensesManuallyDTO_v3_1_POST
    {
        public string UserId { get; set; }
        public string RepositoryId { get; set; }
        public string CurrencyId { get; set; }
        public string PaymentMethodId { get; set; }
        public string ProjectId { get; set; }
        public string CategoryId { get; set; }
        public string Amount { get; set; }
        public string Merchant { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public ExpensesManuallyDTO_v3_1_POST_CustomFields[] CustomFields { get; set; }
    }
    public class ExpensesManuallyDTO_v3_1_POST_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }

    // /Expenses/Update
    public class ExpensesUpdateDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public ExpensesUpdateDTO_v3_1_PATCH_CustomField[] CustomFields { get; set; }
    }
    public class ExpensesUpdateDTO_v3_1_PATCH_CustomField
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    // /Expenses/UpdateAmount
    public class ExpensesUpdateAmountDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Amount { get; set; }
    }

}
