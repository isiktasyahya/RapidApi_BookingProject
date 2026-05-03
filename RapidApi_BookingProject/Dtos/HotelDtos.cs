namespace RapidApi_BookingProject.Dtos
{
    // ── DESTINATION (dest_id bulmak için) ──
    public class DestinationResponseDto
    {
        public List<DestinationItemDto> Data { get; set; }
    }

    public class DestinationItemDto
    {
        public string Dest_Id { get; set; }
        public string Dest_Type { get; set; }
        public string City_Name { get; set; }
    }

    // ── HOTEL SEARCH (otel listesi için) ──
    public class HotelSearchResponseDto
    {
        public HotelSearchDataDto Data { get; set; }
    }

    public class HotelSearchDataDto
    {
        public List<HotelSearchItemDto> Hotels { get; set; }
    }

    public class HotelSearchItemDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("hotel_id")]
        public long HotelId { get; set; }
        public HotelPropertyDto Property { get; set; }
    }

    public class HotelPropertyDto
    {
        public long HotelId { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public double? ReviewScore { get; set; }
        public string ReviewScoreWord { get; set; }
        public int? ReviewCount { get; set; }
        public int? PropertyClass { get; set; }
        public List<string> PhotoUrls { get; set; }
        public HotelPriceDto PriceBreakdown { get; set; }
    }

    public class HotelPriceDto
    {
        public HotelGrossPriceDto GrossPrice { get; set; }
    }

    public class HotelGrossPriceDto
    {
        public string Currency { get; set; }
        public double Value { get; set; }
    }

    // ── HOTEL DETAIL (getHotelDetails için) ──
    public class HotelDetailResponseDto
    {
        public HotelDetailDataDto Data { get; set; }
    }

    public class HotelDetailDataDto
    {
        public long Hotel_Id { get; set; }
        public string Hotel_Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Country_Trans { get; set; }
        public string Countrycode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Review_Nr { get; set; }
        public double? Review_Score { get; set; }
        public string Currency_Code { get; set; }
        public int? Available_Rooms { get; set; }
        public int? Hotel_Include_Breakfast { get; set; }
        public string Accommodation_Type_Name { get; set; }
        public List<string> Hotel_Facilities { get; set; }
        public List<string> Family_Facilities { get; set; }
        public HotelDetailPriceDto Gross_Amount_Per_Night { get; set; }
        public HotelDetailProductPriceDto Product_Price_Breakdown { get; set; }
        public HotelDetailFacilitiesBlockDto Facilities_Block { get; set; }
        public List<HotelDetailHighlightDto> Property_Highlight_Strip { get; set; }

        // Oda fotoğrafları
        public Dictionary<string, HotelRoomDto> Rooms { get; set; }
    }

    public class HotelDetailProductPriceDto
    {
        public HotelDetailPriceDto Gross_Amount_Per_Night { get; set; }
        public HotelDetailPriceDto All_Inclusive_Amount { get; set; }
    }

    public class HotelDetailFacilitiesBlockDto
    {
        public string Name { get; set; }
        public List<HotelDetailFacilityDto> Facilities { get; set; }
    }

    public class HotelDetailFacilityDto
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class HotelDetailHighlightDto
    {
        public string Name { get; set; }
    }

    public class HotelDetailPriceDto
    {
        public double? Value { get; set; }
        public string Currency { get; set; }
    }

    // ── ODA VE FOTOĞRAFLAR ──
    public class HotelRoomDto
    {
        public List<HotelRoomPhotoDto> Photos { get; set; }
        public string Description { get; set; }
    }

    public class HotelRoomPhotoDto
    {
        public string Url_Original { get; set; }
        public string Url_Max750 { get; set; }
        public string Url_Max1280 { get; set; }
    }
}