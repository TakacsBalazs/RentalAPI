using Rental.API.Models.Dtos;

namespace Rental.API.Models.Responses
{
    public class BookingDetailResponse : BookingResponse
    {
        public bool AmIOwner { get; set; }

        public UserDto Owner { get; set; }

        public int? PickupCode { get; set; }
    }
}
