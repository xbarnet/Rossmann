
namespace CaptioB2it.Entidades
{
    // /Travels
    public class TravelsDTO_v3_1
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }
        public string CreationDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ExcludedOwner { get; set; }
        public TravelsDTO_v3_1_User User { get; set; }
        public TravelsDTO_v3_1_Workflow Workflow { get; set; }
        public TravelsDTO_v3_1_CustomFields[] CustomFields { get; set; }
        public TravelsDTO_v3_1_InternalGuests[] InternalGuests { get; set; }
        public TravelsDTO_v3_1_ExternalGuests[] ExternalGuests { get; set; }
        public TravelsDTO_v3_1_Services Services { get; set; }
    }
    public class TravelsDTO_v3_1_User
    {
        public string Id { get; set; }
    }
    public class TravelsDTO_v3_1_Workflow
    {
        public string Id { get; set; }
        public TravelsDTO_v3_1_Workflow_Step Step { get; set; }
    }
    public class TravelsDTO_v3_1_Workflow_Step
    {
        public string Id { get; set; }
    }
    public class TravelsDTO_v3_1_CustomFields
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CodeValue { get; set; }
        public string Value { get; set; }
    }
    public class TravelsDTO_v3_1_InternalGuests
    {
        public string Id { get; set; }
    }
    public class TravelsDTO_v3_1_ExternalGuests
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
    public class TravelsDTO_v3_1_Services
    {
        public string Flight { get; set; }
        public string Train { get; set; }
        public string Hotel { get; set; }
        public string Vehicle { get; set; }
        public string Ship { get; set; }
        public string Other { get; set; }
    }

    // /Travels/Services
    public class TravelsServicesDTO_v3_1
    {
        public string Id { get; set; }
        public TravelsServicesDTO_v3_1_Flights[] Flights { get; set; }
        public TravelsServicesDTO_v3_1_Trains[] Trains { get; set; }
        public TravelsServicesDTO_v3_1_Vehicles[] Vehicles { get; set; }
        public TravelsServicesDTO_v3_1_Ships[] Ships { get; set; }
        public TravelsServicesDTO_v3_1_Hotels[] Hotels { get; set; }
        public TravelsServicesDTO_v3_1_Others[] Others { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Flights
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartMinHour { get; set; }
        public string StartMaxHour { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
        public string FlightCode { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Trains
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartMinHour { get; set; }
        public string StartMaxHour { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Vehicles
    {
        public string Id { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string PickupHour { get; set; }
        public string ReturnHour { get; set; }
        public string PickupPlace { get; set; }
        public string ReturnPlace { get; set; }
        public string Type { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Ships
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartMinHour { get; set; }
        public string StartMaxHour { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Hotels
    {
        public string Id { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string RoomType { get; set; }
        public string BoardType { get; set; }
        public string Category { get; set; }
    }
    public class TravelsServicesDTO_v3_1_Others
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartHour { get; set; }
        public string EndHour { get; set; }
        public string Description { get; set; }
    }

    // /Travels/Approve
    public class TravelsApproveDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Observations { get; set; }
    }

    // /Travels/RequestApprove
    public class TravelsRequestApproveDTO_v3_1_PUT
    {
        public string Id { get; set; }
        public string Observations { get; set; }
    }

    // /Travels/CustomFields
    public class TravelsCustomFieldsDTO_v3_1_PATCH
    {
        public string Id { get; set; }
        public TravelsCustomFieldsDTO_v3_1_PATCH_CustomFields[] CustomFields { get; set; }
    }
    public class TravelsCustomFieldsDTO_v3_1_PATCH_CustomFields
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

}
