using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RapidApi_BookingProject.Controllers
{
    public class AIController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string? city, string? myCity, string? childirenCount, string? enctranceDate, string? exitDate, string? adultCount, string? type, string? notes)
        {
            var apikey = "OpenAI_Api_Key";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "Sen bir tur danışmanıısın ve kullanıcı sana bazı bilgiler göndericek bu bilgilere göre kullanıcının belirttiği gün sayısı kar ona seçmiş olduğu şehir ile tur tur gezi rotası oluşturucaksın, Bu gezi rotasında o şehirdeki en güzel ve en turistik yerleri önericeksin."
                    },
                    new
                    {
                        role = "user",
                        content = $"Kullanıcının seçmiş olduğu bilgiler: Kullanıcının seçmiş olduğu şehir, Tur buradan başlıcak={myCity}, Kullanıcının Gideği şehir: {city}, kullanıcının gidiş tarifi: {enctranceDate}, kullanının dönüş tarihi: {exitDate}, kullanıcının çoçuk sayısı: {childirenCount}, Yetişkin sayısı: {adultCount},  Turun fiyat ortalaması: {type} olsun. , kullanıcın notu: {notes}. Bu bilgilere göre bana Tur Oluştur."
                    }
                },
                temperature = 0.5
            };

            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var content = result.Choices[0].message.content;
                ViewBag.receipe = content;
            }
            else
            {
                ViewBag.receipe = "Bir Hata Oluştu: " + response.StatusCode;
            }

            return View();
        }
        public class OpenAIResponse
        {
            public List<Choices> Choices { get; set; }
        }
        public class Choices
        {
            public Message message { get; set; }
        }
        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }

    }
}
