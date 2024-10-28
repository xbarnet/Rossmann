
namespace CaptioB2it.Entidades
{
    // /Accountings/Banks
    public class AccountingsBanksDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class AccountingsBanksDTO_v3_1_POST
    {
        public string Name { get; set; }
    }

    // /Accountings/Payments
    public class AccountingsPaymentsDTO_v3_1
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Reference { get; set; }
        public AccountingsPaymentsDTO_v3_1_Expense Expense { get; set; }
        public string Reconcile { get; set; }
        public string OperationDate { get; set; }
        public string OriginalAmount { get; set; }
        public string ContractAmount { get; set; }
        public string OperationCountry { get; set; }
        public string OriginalCurrency { get; set; }
        public string OperationCurrency { get; set; }
        public string Provider { get; set; }
        public string CreditCard { get; set; }
        public string PaymentMethodId { get; set; }
        public string BankId { get; set; }
        public string CreationDate { get; set; }
    }
    public class AccountingsPaymentsDTO_v3_1_Expense
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
    }

    public class AccountingsPaymentsDTO_v3_1_POST
    {
        public string OperationDate { get; set; }
        public string OriginalAmount { get; set; }
        public string ContractAmount { get; set; }
        public string OperationCountry { get; set; }
        public string OriginalCurrency { get; set; }
        public string OperationCurrency { get; set; }
        public string Provider { get; set; }
        public string CreditCard { get; set; }
        public string Name { get; set; }
        public string BankId { get; set; }
        public string PaymentMethodId { get; set; }
        public string Reference { get; set; }
    }

}
