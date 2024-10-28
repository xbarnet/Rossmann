
namespace CaptioB2it.Entidades
{
    // /Users
    public class UsersDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string ExternalLogin { get; set; }
        public string AuthenticationType { get; set; }
        public string LanguageCode { get; set; }
        public string Active { get; set; }
        public string Certified { get; set; }
        public string EditExpenseWPicture { get; set; }
        public UsersDTO_v3_1_KmGroup KmGroup { get; set; }
        public UsersDTO_v3_1_UserOptions UserOptions { get; set; }
        public UsersDTO_v3_1_CustomFields[] CustomFields { get; set; }
    }
    public class UsersDTO_v3_1_KmGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class UsersDTO_v3_1_UserOptions
    {
        public string CompanyCode { get; set; }
        public string EmployeeCode { get; set; }
        public string CostCentre { get; set; }
        public string CompanyVATIN { get; set; }
        public string TaxPayerId { get; set; }
    }
    public class UsersDTO_v3_1_CustomFields
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string IsRequired { get; set; }
    }

    public class UsersDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    public class UsersDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public string ExternalLogin { get; set; }
        public string AuthenticationType { get; set; }
        public string ForceChangePasswordOnFirstLogin { get; set; }
        public UsersDTO_v3_1_PATCH_Options Options { get; set; }
    }
    public class UsersDTO_v3_1_PATCH_Options
    {
        public string CostCentre { get; set; }
        public string EmployeeCode { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyVATIN { get; set; }
        public string TaxPayerId { get; set; }
    }

    public class UsersDTO_v3_1_POST
    {
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public string ExternalLogin { get; set; }
        public string AuthenticationType { get; set; }
        public UsersDTO_v3_1_POST_Options Options { get; set; }
        public string LanguageCode { get; set; }
        public string ForceChangePasswordOnFirstLogin { get; set; }
    }
    public class UsersDTO_v3_1_POST_Options
    {
        public string CostCentre { get; set; }
        public string EmployeeCode { get; set; }
        public string CompanyCode { get; set; }
        public string TaxPayerId { get; set; }
    }

    public class UsersDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public string ExternalLogin { get; set; }
        public string AuthenticationType { get; set; }
        public string ForceChangePasswordOnFirstLogin { get; set; }
        public UsersDTO_v3_1_PUT_Options Options { get; set; }
    }
    public class UsersDTO_v3_1_PUT_Options
    {
        public string CostCentre { get; set; }
        public string EmployeeCode { get; set; }
        public string CompanyCode { get; set; }
    }

    // /Users/Banks
    public class UsersBanksDTO_v3_1
    {
        public string Id { get; set; }
        public string Bic { get; set; }
        public string Iban { get; set; }
    }

    public class UsersBanksDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Bic { get; set; }
        public string Iban { get; set; }
    }

    // /Users/Groups
    public class UsersGroupsDTO_v3_1
    {
        public string Id { get; set; }
        public UsersGroupsDTO_v3_1_Groups[] Groups { get; set; }
    }
    public class UsersGroupsDTO_v3_1_Groups
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class UsersGroupsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string[] Groups { get; set; }
    }

    // /Users/Payments
    public class UsersPaymentsDTO_v3_1
    {
        public string Id { get; set; }
        public UsersPaymentsDTO_v3_1_Payments[] Payments { get; set; }
    }
    public class UsersPaymentsDTO_v3_1_Payments
    {
        public string Id { get; set; }
        public string PaymentId { get; set; }
        public string Value { get; set; }
        public string IdentifierType { get; set; }
    }

    public class UsersPaymentsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public UsersPaymentsDTO_v3_1_DELETE_Payments[] Payments { get; set; }
    }
    public class UsersPaymentsDTO_v3_1_DELETE_Payments
    {
        public string Id { get; set; }
    }

    public class UsersPaymentsDTO_v3_1_POST
    {
        public string Id { get; set; }
        public UsersPaymentsDTO_v3_1_POST_Payments[] Payments { get; set; }
    }
    public class UsersPaymentsDTO_v3_1_POST_Payments
    {
        public string PaymentId { get; set; }
        public string Value { get; set; }
        public string IdentifierType { get; set; }
    }


    // /Users/Workflows
    public class UsersWorkflowsDTO_v3_1
    {
        public string Id { get; set; }
        public UsersWorkflowsDTO_v3_1_Workflows[] Workflows { get; set; }
    }
    public class UsersWorkflowsDTO_v3_1_Workflows
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string CreatedDate { get; set; }
        public string Type { get; set; }
        public string TypeActivationStep { get; set; }
        public string IsDefault { get; set; }
    }

    // /Users/Delegations
    // User Delegations.Type = 1: Total.Type = 2: Partial
    public class UsersDelegationsDTO_v3_1
    {
        public string Id { get; set; }
        public UsersDelegationsDTO_v3_1_SupplantUsers[] SupplantUsers { get; set; }
    }
    public class UsersDelegationsDTO_v3_1_SupplantUsers
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class UsersDelegationsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public UsersDelegationsDTO_v3_1_DELETE_SupplantUsers[] SupplantUsers { get; set; }
    }
    public class UsersDelegationsDTO_v3_1_DELETE_SupplantUsers
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class UsersDelegationsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public UsersDelegationsDTO_v3_1_PATCH_SupplantUsers[] SupplantUsers { get; set; }
    }
    public class UsersDelegationsDTO_v3_1_PATCH_SupplantUsers
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class UsersDelegationsDTO_v3_1_POST
    {
        public string Id { get; set; }
        public UsersDelegationsDTO_v3_1_POST_SupplantUsers[] SupplantUsers { get; set; }
    }
    public class UsersDelegationsDTO_v3_1_POST_SupplantUsers
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }
    
    // /Users/TravelGroups
    public class UsersTravelGroupsDTO_v3_1
    {
        public string Id { get; set; }
        public string ActiveTravelGroup { get; set; }
    }

    public class UsersTravelGroupsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string ActiveTravelGroup { get; set; }
    }

    // /Users/KmGroups
    public class UsersKmGroupsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
    }

    // /Users/CustomFields
    public class UsersCustomFieldsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public UsersCustomFieldsDTO_v3_1_PUT_CustomFields[] CustomFields { get; set; }
    }
    public class UsersCustomFieldsDTO_v3_1_PUT_CustomFields
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    // /Users/Workflows/Join
    public class UsersWorkflowsJoinDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public UsersWorkflowsJoinDTO_v3_1_DELETE_Workflows[] Workflows { get; set; }
    }
    public class UsersWorkflowsJoinDTO_v3_1_DELETE_Workflows
    {
        public string Id { get; set; }
        public string Default { get; set; }
    }

    public class UsersWorkflowsJoinDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public UsersWorkflowsJoinDTO_v3_1_PATCH_Workflows[] Workflows { get; set; }
    }
    public class UsersWorkflowsJoinDTO_v3_1_PATCH_Workflows
    {
        public string Id { get; set; }
        public string Default { get; set; }
    }

    public class UsersWorkflowsJoinDTO_v3_1_POST
    {
        public string Id { get; set; }
        public UsersWorkflowsJoinDTO_v3_1_POST_Workflows[] Workflows { get; set; }
    }
    public class UsersWorkflowsJoinDTO_v3_1_POST_Workflows
    {
        public string Id { get; set; }
        public string Default { get; set; }
    }

    // /Users/Login
    public class UsersLoginDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Login { get; set; }
    }

    // /Users/Certified
    public class UsersCertifiedDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Certified { get; set; }
    }

    // /Users/AlertsActive
    public class UsersAlertsActiveDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public UsersAlertsActiveDTO_v3_1_PATCH_Alerts[] Alerts { get; set; }
    }
    public class UsersAlertsActiveDTO_v3_1_PATCH_Alerts
    {
        public string Id { get; set; }
        public string Active { get; set; }
    }

    // /Users/EditExpenseWPicture
    public class UsersEditExpenseWPictureDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Editable { get; set; }
    }

}
