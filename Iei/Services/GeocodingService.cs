using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iei.Services
{
    public class GeocodingService
    {
        private readonly string _apiKey = "35672dd4e7574171aa6a196aa108482a";

        public async Task<(string Address, string Postcode, string Province, string Locality)> GetGeocodingDetails(double latitud, double longitud)
        {
            string url = $"https://api.opencagedata.com/geocode/v1/json?q={latitud}%2C{longitud}&key={_apiKey}";
            Console.WriteLine($"URL de solicitud: {url}");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(responseBody);

                        var result = jsonResponse["results"]?[0];

                        if (result == null)
                        {
                            throw new Exception("No se encontraron resultados para la ubicación.");
                        }

                        string locality = result["components"]?["town"]?.ToString() ?? "";
                        string postcode = result["components"]?["postcode"]?.ToString() ?? "";
                        string province = result["components"]?["province"]?.ToString() ?? "";
                        string city = result["components"]?["city"]?.ToString() ?? locality;
                        string calle = result["components"]?["road"]?.ToString() ?? "";

                        string address = $"{calle}";

                        return (address, postcode, province, locality);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error en la solicitud: {response.StatusCode}");
                        Console.WriteLine($"Mensaje de error: {errorMessage}");
                        throw new Exception($"Error en la solicitud: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}