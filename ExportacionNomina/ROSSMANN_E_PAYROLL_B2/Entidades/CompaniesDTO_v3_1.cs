namespace CaptioB2it.Entidades
{
    // /Companies/Vats
    public class CompaniesVatsDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CountryId { get; set; }
        public string Active { get; set; }
        public string Deleted { get; set; }
        public CompaniesVatsDTO_v3_1_Rates[] Rates { get; set; }
    }
    public class CompaniesVatsDTO_v3_1_Rates
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Active { get; set; }
        public string Deleted { get; set; }
        public string[] Categories { get; set; }
        public CompaniesVatsDTO_v3_1_Rates_Period[] Periods { get; set; }
    }
    public class CompaniesVatsDTO_v3_1_Rates_Period
    {
        public string Id { get; set; }
        public string Percentage { get; set; }
        public string StartDate { get; set; }
        public string Deleted { get; set; }
    }

    // /Companies/Rules
    public class CompaniesRulesDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    // /Companies/Alerts
    public class CompaniesAlertsDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Active { get; set; }
    }

    // /Companies/Groups
    public class CompaniesGroupsDTO_v3_1
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
    }
    
    // /Companies/Currency
    public class CompaniesCurrencyDTO_v3_1
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string ISOCode { get; set; }
    }

    // /Companies/KmGroups
    public class CompaniesKmGroupsDTO_v3_1
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string Fee { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    // /Companies/Payments
    public class CompaniesPaymentsDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Reimbursable { get; set; }
        public string Reconcilable { get; set; }
    }

    // /Companies/Categories
    public class CompaniesCategoriesDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Deleted { get; set; }
        public string Active { get; set; }
        public string Amount { get; set; }
        public string OnlyKM { get; set; }
        public string Code { get; set; }
        public string Updated { get; set; }
        public string VATPercentage { get; set; }
        public string MaxAmount { get; set; }
        public string SelfLimited { get; set; }
        public string UseCuotaVAT { get; set; }
        public string CurrencyId { get; set; }
        public CompaniesCategoryDTO_v3_1_Languages[] Languages { get; set; }
        public CompaniesCategoryDTO_v3_1_Subcategories[] Subcategories { get; set; }
    }
    public class CompaniesCategoryDTO_v3_1_Languages
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
    public class CompaniesCategoryDTO_v3_1_Subcategories
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Deleted { get; set; }
        public string Active { get; set; }
        public string Amount { get; set; }
        public string OnlyKM { get; set; }
        public string Code { get; set; }
        public string Updated { get; set; }
        public string VATPercentage { get; set; }
        public string MaxAmount { get; set; }
        public string SelfLimited { get; set; }
        public string UseCuotaVAT { get; set; }
        public string CurrencyId { get; set; }
        public CompaniesCategoryDTO_v3_1_Languages[] Languages { get; set; }
    }

    public class CompaniesCategoriesDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    public class CompaniesCategoriesDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string Active { get; set; }
        public string SelfLimited { get; set; }
        public string MaxAmount { get; set; }
        public string OnlyKM { get; set; }
        public CompaniesCategoriesDTO_v3_1_PATCH_Languages[] Languages { get; set; }
    }
    public class CompaniesCategoriesDTO_v3_1_PATCH_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class CompaniesCategoriesDTO_v3_1_POST
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string Active { get; set; }
        public string SelfLimited { get; set; }
        public string MaxAmount { get; set; }
        public string OnlyKM { get; set; }
        public CompaniesCategoriesDTO_v3_1_POST_Subcategories[] Subcategories { get; set; }
        public CompaniesCategoriesDTO_v3_1_POST_Languages[] Languages { get; set; }
    }
    public class CompaniesCategoriesDTO_v3_1_POST_Subcategories
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string Active { get; set; }
        public string SelfLimited { get; set; }
        public string MaxAmount { get; set; }
        public string OnlyKM { get; set; }
        public CompaniesCategoriesDTO_v3_1_POST_Languages[] Languages { get; set; }
    }
    public class CompaniesCategoriesDTO_v3_1_POST_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

    // /Companies/Groups/Users
    public class CompaniesGroupsUsersDTO_v3_1
    {
        public string Id { get; set; }
        public CompaniesGroupsUsersDTO_v3_1_Users[] Users { get; set; }
    }
    public class CompaniesGroupsUsersDTO_v3_1_Users
    {
        public string Id { get; set; }
    }

    // /Companies/UserCustomFields
    public class CompaniesUserCustomFieldsDTO_v3_1
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string IsRequired { get; set; }
    }

    public class CompaniesUserCustomFieldsDTO_v3_1_PUT
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string IsRequired { get; set; }
    }

    // /Companies/Categories/Groups 
    public class CompaniesCategoriesGroupsDTO_v3_1
    {
        public string Id { get; set; }
        public CompaniesCategoriesGroupsDTO_v3_1_Groups[] Groups { get; set; }
    }
    public class CompaniesCategoriesGroupsDTO_v3_1_Groups
    {
        public string Id { get; set; }
    }

    // /Companies/VatIdentificationNumber
    public class CompaniesVatIdentificationNumberDTO_v3_1
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyVATIN { get; set; }
    }

    // /Companies/SubCategories
    public class CompaniesSubCategoriesDTO_v3_1_POST
    {
        public string Id { get; set; }
        public CompaniesSubCategoriesDTO_v3_1_POST_SubCategories[] SubCategories { get; set; }
    }
    public class CompaniesSubCategoriesDTO_v3_1_POST_SubCategories
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Account { get; set; }
        public string Active { get; set; }
        public string SelfLimited { get; set; }
        public string MaxAmount { get; set; }
        public string OnlyKM { get; set; }
        public CompaniesSubCategoriesDTO_v3_1_POST_SubCategories_Languages[] Languages { get; set; }
    }
    public class CompaniesSubCategoriesDTO_v3_1_POST_SubCategories_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

}
