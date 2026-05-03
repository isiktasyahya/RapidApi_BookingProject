using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RapidApi_BookingProject.Dtos;
using System.Text.Json;

namespace RapidApi_BookingProject.Controllers
{
    public class HotelController : Controller
    {
        private readonly HttpClient _client;

        public HotelController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. DÖVİZ ÇEK
            var currencies = new[] { "USD", "EUR", "GBP", "AED", "JPY" };
            double usdRate = 38;

            try
            {
                foreach (var currency in currencies)
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://currency-conversion-and-exchange-rates.p.rapidapi.com/convert?from={currency}&to=TRY&amount=1"),
                        Headers =
                        {
                            { "x-rapidapi-key", "dc1c270273mshedea0f305f4d655p13843ejsnb9bc204007f8" },
                            { "x-rapidapi-host", "currency-conversion-and-exchange-rates.p.rapidapi.com" },
                        }
                    };

                    using (var response = await _client.SendAsync(request))
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        var values = JsonConvert.DeserializeObject<CurrencyDto>(jsonData);
                        var rate = values.result;

                        switch (currency)
                        {
                            case "USD": ViewBag.Usd = rate.ToString("F2"); usdRate = rate; break;
                            case "EUR": ViewBag.Eur = rate.ToString("F2"); break;
                            case "GBP": ViewBag.Gbp = rate.ToString("F2"); break;
                            case "AED": ViewBag.Aed = rate.ToString("F2"); break;
                            case "JPY": ViewBag.Jpy = rate.ToString("F2"); break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DÖVİZ HATA: " + ex.Message);
                ViewBag.Usd = "—"; ViewBag.Eur = "—";
                ViewBag.Gbp = "—"; ViewBag.Aed = "—"; ViewBag.Jpy = "—";
            }

            // 2. AKARYAKIT ÇEK
            try
            {
                var fuelRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://fuel-gas-price-api.p.rapidapi.com/national"),
                    Headers =
                    {
                        { "x-rapidapi-key", "cbcb4bd047mshee3d98e846f3fcdp1ff70ajsn9ed1a634e668" },
                        { "x-rapidapi-host", "fuel-gas-price-api.p.rapidapi.com" },
                    }
                };

                using (var fuelResponse = await _client.SendAsync(fuelRequest))
                {
                    var fuelJson = await fuelResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("FUEL JSON: " + fuelJson);
                    var fuelData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(fuelJson);

                    var regular = fuelData.GetProperty("data").GetProperty("regular").GetProperty("current").GetDouble();
                    var diesel = fuelData.GetProperty("data").GetProperty("diesel").GetProperty("current").GetDouble();

                    ViewBag.Benzin = ((regular * usdRate / 3.785 + 17.00) * 1.20).ToString("F2");
                    ViewBag.Motorin = ((diesel * usdRate / 3.785 + 10.00) * 1.20).ToString("F2");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AKARYAKIT HATA: " + ex.Message);
                ViewBag.Benzin = "—";
                ViewBag.Motorin = "—";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string destId, string arrivalDate, string departureDate, int adults)
        {
            TempData["ArrivalDate"] = arrivalDate;
            TempData["DepartureDate"] = departureDate;
            TempData["Adults"] = adults;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var destRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query={destId}"),
                Headers =
                {
                    { "x-rapidapi-key", "da3b83105cmsh002393b27e10bb3p100447jsn7c57f49c7365" },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                }
            };

            var destResponse = await _client.SendAsync(destRequest);
            var destBody = await destResponse.Content.ReadAsStringAsync();
            var destResult = System.Text.Json.JsonSerializer.Deserialize<DestinationResponseDto>(destBody, options);
            var cityDestId = destResult.Data.First().Dest_Id;

            var hotelRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels" +
                    $"?dest_id={cityDestId}&search_type=CITY" +
                    $"&arrival_date={arrivalDate}&departure_date={departureDate}" +
                    $"&adults={adults}&room_qty=1&languagecode=en-us&currency_code=TRY"),
                Headers =
                {
                    { "x-rapidapi-key", "da3b83105cmsh002393b27e10bb3p100447jsn7c57f49c7365" },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                }
            };

            var hotelResponse = await _client.SendAsync(hotelRequest);
            var hotelBody = await hotelResponse.Content.ReadAsStringAsync();
            var hotelResult = System.Text.Json.JsonSerializer.Deserialize<HotelSearchResponseDto>(hotelBody, options);

            var hotels = hotelResult.Data.Hotels.Select(h => {
                h.Property.HotelId = h.HotelId;
                return h.Property;
            }).ToList();

            return View("Index", hotels);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var arrivalDate = TempData["ArrivalDate"]?.ToString() ?? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var departureDate = TempData["DepartureDate"]?.ToString() ?? DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            var adults = TempData["Adults"]?.ToString() ?? "1";

            if (DateTime.TryParse(arrivalDate, out var arrDate))
                arrivalDate = arrDate.ToString("yyyy-MM-dd");

            if (DateTime.TryParse(departureDate, out var depDate))
                departureDate = depDate.ToString("yyyy-MM-dd");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/getHotelDetails" +
                    $"?hotel_id={id}" +
                    $"&arrival_date={arrivalDate}" +
                    $"&departure_date={departureDate}" +
                    $"&adults={adults}" +
                    $"&room_qty=1&languagecode=en-us&currency_code=TRY"),
                Headers =
                {
                    { "x-rapidapi-key", "da3b83105cmsh002393b27e10bb3p100447jsn7c57f49c7365" },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                }
            };

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var hotelDetail = System.Text.Json.JsonSerializer.Deserialize<HotelDetailResponseDto>(body, options);

            ViewBag.ArrivalDate = arrivalDate;
            ViewBag.DepartureDate = departureDate;
            ViewBag.Adults = adults;

            return View(hotelDetail.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Convert(string from, string to, double amount)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://currency-conversion-and-exchange-rates.p.rapidapi.com/convert?from={from}&to={to}&amount={amount}"),
                Headers =
                {
                    { "x-rapidapi-key", "dc1c270273mshedea0f305f4d655p13843ejsnb9bc204007f8" },
                    { "x-rapidapi-host", "currency-conversion-and-exchange-rates.p.rapidapi.com" },
                }
            };

            using (var response = await _client.SendAsync(request))
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<CurrencyDto>(jsonData);
                return Json(new { result = values.result.ToString("F2"), from, to, amount });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Weather(string city)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-weather5.p.rapidapi.com/weather?location={city}&format=json&u=f"),
                Headers =
                {
                    { "x-rapidapi-key", "dc1c270273mshedea0f305f4d655p13843ejsnb9bc204007f8" },
                    { "x-rapidapi-host", "yahoo-weather5.p.rapidapi.com" },
                }
            };

            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
                return Json(result);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCrypto()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://coinranking1.p.rapidapi.com/coins?symbols[]=BTC&symbols[]=ETH&symbols[]=BNB&symbols[]=SOL&symbols[]=XRP&limit=5"),
                Headers =
                {
                    { "x-rapidapi-key", "dc1c270273mshedea0f305f4d655p13843ejsnb9bc204007f8" },
                    { "x-rapidapi-host", "coinranking1.p.rapidapi.com" },
                }
            };

            using (var response = await _client.SendAsync(request))
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
                return Json(result);
            }
        }
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SearchAdvanced(
    string cityName,
    string arrivalDate,
    string departureDate,
    int adults = 1,
    string childrenAge = "",
    int roomQty = 1,
    int? priceMin = null,
    int? priceMax = null,
    string sortBy = "",
    string currencyCode = "TRY",
    int pageNumber = 1)
        {
            TempData["ArrivalDate"] = arrivalDate;
            TempData["DepartureDate"] = departureDate;
            TempData["Adults"] = adults;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. ADIM — dest_id bul
            var destRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query={cityName}"),
                Headers =
        {
            { "x-rapidapi-key", "da3b83105cmsh002393b27e10bb3p100447jsn7c57f49c7365" },
            { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
        }
            };

            var destResponse = await _client.SendAsync(destRequest);
            var destBody = await destResponse.Content.ReadAsStringAsync();
            var destResult = System.Text.Json.JsonSerializer.Deserialize<DestinationResponseDto>(destBody, options);
            var cityDestId = destResult.Data.First().Dest_Id;

            // 2. ADIM — URL oluştur
            var url = $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels" +
                      $"?dest_id={cityDestId}&search_type=CITY" +
                      $"&arrival_date={arrivalDate}&departure_date={departureDate}" +
                      $"&adults={adults}&room_qty={roomQty}" +
                      $"&currency_code={currencyCode}&languagecode=tr" +
                      $"&page_number={pageNumber}";

            if (!string.IsNullOrEmpty(childrenAge)) url += $"&children_age={childrenAge}";
            if (priceMin.HasValue) url += $"&price_min={priceMin}";
            if (priceMax.HasValue) url += $"&price_max={priceMax}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sort_by={sortBy}";

            // 3. ADIM — otelleri getir
            var hotelRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers =
        {
            { "x-rapidapi-key", "da3b83105cmsh002393b27e10bb3p100447jsn7c57f49c7365" },
            { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
        }
            };

            var hotelResponse = await _client.SendAsync(hotelRequest);
            var hotelBody = await hotelResponse.Content.ReadAsStringAsync();
            var hotelResult = System.Text.Json.JsonSerializer.Deserialize<HotelSearchResponseDto>(hotelBody, options);

            var hotels = hotelResult.Data.Hotels.Select(h => {
                h.Property.HotelId = h.HotelId;
                return h.Property;
            }).ToList();

            return View("Search", hotels);
        }
    }
}