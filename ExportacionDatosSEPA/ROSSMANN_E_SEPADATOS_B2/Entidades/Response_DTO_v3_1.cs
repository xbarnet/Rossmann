
namespace CaptioB2it.Entidades
{
    public class Error_400_DTO_v3_1
    {
        public Error_400_DTO_v3_1_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public Error_400_DTO_v3_1_Validations[] Validations { get; set; }
        public int Status { get; set; }
    }
    public class Error_400_DTO_v3_1_Result
    {
        public string Id { get; set; }
    }
    public class Error_400_DTO_v3_1_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class ResponseDTO_v3_1_POST
    {
        public ResponseDTO_v3_1_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ResponseDTO_v3_1_Validations[] Validations { get; set; }
        public int Status { get; set; }
    }
    public class ResponseDTO_v3_1_Result
    {
        public string Id { get; set; }
    }
    public class ResponseDTO_v3_1_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class ResponseUsersDTO_v3_1_POST
    {
        public ResponseDTO_v3_1_POST_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ResponseDTO_v3_1_POST_Validations[] Validations { get; set; }
        public string Status { get; set; }
    }
    public class ResponseDTO_v3_1_POST_Result
    {
        public string UserId { get; set; }
    }
    public class ResponseDTO_v3_1_POST_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class ResponseUsersPaymentsDTO_v3_1_POST
    {
        public ResponseUsersPaymentsDTO_v3_1_POST_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ResponseUsersPaymentsDTO_v3_1_POST_Validations[] Validations { get; set; }
        public string Status { get; set; }
    }
    public class ResponseUsersPaymentsDTO_v3_1_POST_Result
    {
        public ResponseUsersPaymentsDTO_v3_1_POST_Result_Payments Payments { get; set; }
    }
    public class ResponseUsersPaymentsDTO_v3_1_POST_Result_Payments
    {
        public string PaymentId { get; set; }
        public string UserPaymentId { get; set; }
        public string IdentifierType { get; set; }
    }
    public class ResponseUsersPaymentsDTO_v3_1_POST_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class ResponseCustomFieldItemsDTO_v3_1_POST
    {
        public ResponseCustomFieldItemsDTO_v3_1_POST_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ResponseCustomFieldItemsDTO_v3_1_POST_Validations[] Validations { get; set; }
        public int Status { get; set; }
    }
    public class ResponseCustomFieldItemsDTO_v3_1_POST_Result
    {
        public string Id { get; set; }
        public string[] Items { get; set; }
    }
    public class ResponseCustomFieldItemsDTO_v3_1_POST_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class ResponseWorkflowsDTO_v3_1_POST
    {
        public ResponseWorkflowsDTO_v3_1_POST_Result Result { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ResponseWorkflowsDTO_v3_1_POST_Validations[] Validations { get; set; }
        public string Status { get; set; }
    }
    public class ResponseWorkflowsDTO_v3_1_POST_Result
    {
        public string ExternalId { get; set; }
    }
    public class ResponseWorkflowsDTO_v3_1_POST_Validations
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
