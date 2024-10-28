
namespace CaptioB2it.Entidades
{
    // /AssignmentObjects
    public class AssignmentObjectsDTO_v3_1
    {
        // RelationType = 0 'Assignment object at environment level' - RelationType = 1 'Assignment object at user level'.
        public string Id { get; set; }
        public string Name { get; set; }
        public string Deleted { get; set; }
        public string RelationType { get; set; }
    }

    public class AssignmentObjectsDTO_v3_1_POST
    {
        // Create Assignment Objects and return the Id. RelationType = 0 'Assignment object at environment level' - RelationType = 1 'Assignment object at user level'. Type: 1 CF expense, 2: report, 5: workflow, 6: advance
        public string Name { get; set; }
        public string RelationType { get; set; }
        public string Required { get; set; }
        public string Type { get; set; }
    }

    public class AssignmentObjectsDTO_v3_1_DELETE
    {
        // Create Assignment Objects and return the Id. RelationType = 0 'Assignment object at environment level' - RelationType = 1 'Assignment object at user level'. Type: 1 CF expense, 2: report, 5: workflow, 6: advance
        public string Id { get; set; }
    }

    // /AssignmentObjects/Values
    public class AssignmentObjectsValuesDTO_v3_1
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesDTO_v3_1_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesDTO_v3_1_Values
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string Deleted { get; set; }
    }

    public class AssignmentObjectsValuesDTO_v3_1_POST
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesDTO_v3_1_POST_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesDTO_v3_1_POST_Values
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class AssignmentObjectsValuesDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesDTO_v3_1_DELETE_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesDTO_v3_1_DELETE_Values
    {
        public string Id { get; set; }
    }

    // /AssignmentObjects/ValuesUsers
    public class AssignmentObjectsValuesUsersDTO_v3_1
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesUsersDTO_v3_1_Values
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_Values_Users[] Users { get; set; }
    }
    public class AssignmentObjectsValuesUsersDTO_v3_1_Values_Users
    {
        public string Id { get; set; }
    }

    public class AssignmentObjectsValuesUsersDTO_v3_1_POST
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_POST_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesUsersDTO_v3_1_POST_Values
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users[] Users { get; set; }
    }

    public class AssignmentObjectsValuesUsersDTO_v3_1_POST_Values_Users
    {
        public string Id { get; set; }
    }

    public class AssignmentObjectsValuesUsersDTO_v3_1_DELETE
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values[] Values { get; set; }
    }
    public class AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values
    {
        public string Id { get; set; }
        public AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users[] Users { get; set; }
    }

    public class AssignmentObjectsValuesUsersDTO_v3_1_DELETE_Values_Users
    {
        public string Id { get; set; }
    }


}
