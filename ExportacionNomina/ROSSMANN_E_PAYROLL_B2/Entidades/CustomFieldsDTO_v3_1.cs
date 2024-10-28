namespace CaptioB2it.Entidades
{
    // /CustomFields
    public class CustomFieldsDTO_v3_1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalName { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Type { get; set; }
        public string Mandatory { get; set; }
        public string Deleted { get; set; }
        public string Filterable { get; set; }
        public string AssignmentObjectId { get; set; }
        public string AffectAllCategories { get; set; }
        public CustomFieldsDTO_v3_1_Categories[] Categories { get; set; }
        public string AffectAllUsers { get; set; }
        public string KeyDefaultValue { get; set; }
        public string BooleanDefaultValue { get; set; }
        public string Order { get; set; }
        public string UpdatedDate { get; set; }
        public string ValidationExpression { get; set; }
        public string ExternalId { get; set; }
        public CustomFieldsDTO_v3_1_Languages[] Languages { get; set; }
    }
    public class CustomFieldsDTO_v3_1_Categories
    {
        public string Id { get; set; }
    }
    public class CustomFieldsDTO_v3_1_Languages
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class CustomFieldsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
    }

    public class CustomFieldsDTO_v3_1_POST
    {
        public string Name { get; set; }
        public string ExternalName { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Type { get; set; }
        public string Mandatory { get; set; }
        public string Filterable { get; set; }
        public string AssignmentObjectId { get; set; }
        public string AffectAllCategories { get; set; }
        public string KeyDefaultValue { get; set; }
        public string BooleanDefaultValue { get; set; }
        public string Order { get; set; }
        public string FieldWSID { get; set; }
        public CustomFieldsDTO_v3_1_POST_Languages[] Languages { get; set; }
        public CustomFieldsDTO_v3_1_POST_Categories[] Categories { get; set; }
    }
    public class CustomFieldsDTO_v3_1_POST_Languages
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
    public class CustomFieldsDTO_v3_1_POST_Categories
    {
        public string Id { get; set; }
    }

    public class CustomFieldsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalName { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Type { get; set; }
        public string Mandatory { get; set; }
        public string Filterable { get; set; }
        public string AssignmentObjectId { get; set; }
        public string AffectAllCategories { get; set; }
        public string KeyDefaultValue { get; set; }
        public string AffectAlert { get; set; }
        public string BooleanDefaultValue { get; set; }
        public string Order { get; set; }
        public string FieldWSID { get; set; }
        public CustomFieldsDTO_v3_1_PUT_Languages[] Languages { get; set; }
        public CustomFieldsDTO_v3_1_PUT_Categories[] Categories { get; set; }
    }
    public class CustomFieldsDTO_v3_1_PUT_Languages
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
    public class CustomFieldsDTO_v3_1_PUT_Categories
    {
        public string Id { get; set; }
    }

    // /CustomFields/Items
    public class CustomFieldsItemsDTO_v3_1
    {
        public string Id { get; set; }
        public CustomFieldItemsDTO_v3_1_CustomFieldItems[] CustomFieldItems { get; set; }
    }
    public class CustomFieldItemsDTO_v3_1_CustomFieldItems
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Deleted { get; set; }
        public string IsDetailRequired { get; set; }
        public string Extras { get; set; }
        public string ExtraInfo { get; set; }
        public string Code { get; set; }
        public string Updated { get; set; }
        public CustomFieldItemsDTO_v3_1_CustomFieldItems_Languages[] Languages { get; set; }
    }
    public class CustomFieldItemsDTO_v3_1_CustomFieldItems_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class CustomFieldsItemsDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public CCustomFieldsItemsDTO_v3_1_DELETE_Items[] Items { get; set; }
    }
    public class CCustomFieldsItemsDTO_v3_1_DELETE_Items
    {
        public string Id { get; set; }
    }

    public class CustomFieldsItemsDTO_v3_1_POST
    {
        public string Id { get; set; }
        public CustomFieldsItemsDTO_v3_1_POST_Items[] Items { get; set; }
    }
    public class CustomFieldsItemsDTO_v3_1_POST_Items
    {
        public string Name { get; set; }
        public string Extras { get; set; }
        public string IsDetailRequired { get; set; }
        public string ExtraInfo { get; set; }
        public string Code { get; set; }
        public CustomFieldsItemsDTO_v3_1_POST_Items_Languages[] Languages { get; set; }
    }
    public class CustomFieldsItemsDTO_v3_1_POST_Items_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class CustomFieldsItemsDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public CustomFieldsItemsDTO_v3_1_PUT_Items[] Items { get; set; }
    }
    public class CustomFieldsItemsDTO_v3_1_PUT_Items
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Extras { get; set; }
        public string IsDetailRequired { get; set; }
        public string ExtraInfo { get; set; }
        public string Code { get; set; }
        public CustomFieldsItemsDTO_v3_1_PUT_Items_Languages[] Languages { get; set; }
    }
    public class CustomFieldsItemsDTO_v3_1_PUT_Items_Languages
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

}

