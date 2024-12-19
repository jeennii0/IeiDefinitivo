using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Threading;
using Iei.Models;
using  Convertidor;
using Iei.Services;



namespace Iei.Extractors
{
    public class CVExtractor
    {
        public CVExtractor()
        {
        }
        private GeocodingService geocodingService = new GeocodingService();


        public async Task<List<Monumento>> ExtractData(List<ModeloCSVOriginal> monumentosCsv)
        {
            try
            {
                var monumentos = new List<Monumento>();
                var convertidor = new Convertidor.Convertidor();

                foreach (var monumento in monumentosCsv)
                {
                    // Validar que la denominación, las coordenadas y la provincia estén presentes
                    if (string.IsNullOrWhiteSpace(monumento.Denominacion) || 
                          string.IsNullOrWhiteSpace(monumento.Clasificacion) )
                    {
                        Console.WriteLine($"Datos incompletos para el monumento {monumento.Denominacion}. Saltando este monumento.");
                        continue; // Saltar al siguiente monumento si falta información clave
                    }

                    // Normalizar la provincia
                    var provinciaNormalizada = NormalizarProvincia(monumento.Provincia?.ToString() ?? "");

                    if (string.IsNullOrEmpty(provinciaNormalizada))
                    {
                        Console.WriteLine($"Provincia no válida para el monumento {monumento.Denominacion}. Saltando este monumento.");
                        continue; // Saltar al siguiente monumento si la provincia no es válida
                    }

                    var nuevoMonumento = new Monumento
                    {
                        Nombre = monumento.Denominacion?.ToString() ?? "",
                        Descripcion = monumento.Clasificacion?.ToString() ?? "",
                        Tipo = ConvertirTipoMonumento(monumento.Categoria?.ToString() ?? ""),
                        Localidad = new Localidad
                        {
                            Nombre = monumento.Municipio?.ToString() ?? "",
                            Provincia = new Provincia
                            {
                                Nombre = provinciaNormalizada
                            }
                        },
                    };

                    // Validar datos UTM y llamar al conversor
                    if (monumento.UtmEste != null && monumento.UtmNorte != null && !string.IsNullOrEmpty(monumento.Provincia))
                    {
                        try
                        {
                            // Verificar que las coordenadas sean números válidos
                            double utmEste = (double)monumento.UtmEste;
                            double utmNorte = (double)monumento.UtmNorte;

                            // Asegurarse de que las coordenadas no son NaN o valores fuera de rango
                            if (double.IsNaN(utmEste) || double.IsNaN(utmNorte) || utmEste == 0 || utmNorte == 0)
                            {
                                throw new ArgumentException("Las coordenadas UTM son inválidas.");
                            }

                            // Llamar al conversor para obtener las coordenadas en WGS84 (latitud, longitud)
                            var coordenadas = await convertidor.ConvertUTMToLatLong(utmEste.ToString(), utmNorte.ToString());

                            // Asignar las coordenadas obtenidas a las propiedades del monumento
                            nuevoMonumento.Latitud = coordenadas.latitud;
                            nuevoMonumento.Longitud = coordenadas.longitud;
                        }
                        catch (Exception ex)
                        {
                            // Si ocurre un error en la conversión, logueamos el error pero seguimos con el siguiente monumento.
                            Console.WriteLine($"Error al convertir UTM a lat/long para el monumento {monumento.Denominacion}: {ex.Message}");
                            continue; // Saltar al siguiente monumento
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Datos UTM inválidos para el monumento {monumento.Denominacion}. Asignando valores predeterminados.");
                        continue; // Saltar al siguiente monumento
                    }

                    // Si la dirección o el código postal no están definidos, realizamos una búsqueda de geocodificación
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) || string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal)
                       || string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) || string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                    {
                        var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                        if (string.IsNullOrEmpty(nuevoMonumento.Direccion)) nuevoMonumento.Direccion = address;
                        if (string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)) nuevoMonumento.CodigoPostal = postcode;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre)) nuevoMonumento.Localidad.Nombre = locality;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre)) nuevoMonumento.Localidad.Provincia.Nombre = province;
                    }

                    // Agregar el nuevo monumento a la lista
                    monumentos.Add(nuevoMonumento);
                }

                return monumentos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al extraer datos del archivo: {ex.Message}");
                return null;
            }
        }

        public string ConvertirTipoMonumento(string tipoMonumento)
        {
            var tipoMonumentoMap = new Dictionary<string, string>
            {
                { "Zona arqueológica", "Yacimiento arqueológico" },
                { "Monumento", "Edificio singular" },
                { "Individual (mueble)", "Edificio singular" },
                { "Conjunto histórico", "Edificio singular" },
                { "Fondo de museo (primera)", "Otros" },
                { "Zona paleontológica", "Yacimiento arqueológico" },
                { "Archivo", "Otros" },
                { "Espacio etnológico", "Otros" },
                { "Sitio histórico", "Edificio singular" },
                { "Jardín histórico", "Edificio singular" },
                { "Parque cultural", "Otros" },
                { "Monumento de interés local", "Edificio singular" }
            };

            return tipoMonumentoMap.ContainsKey(tipoMonumento)
                ? tipoMonumentoMap[tipoMonumento]
                : "Otros";
        }

        private string NormalizarProvincia(string provincia)
        {
            var provinciaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Alicante", "Alicante" },
                { "Castellón", "Castellón" },
                { "Valencia", "Valencia" }
            };

            return provinciaMap.ContainsKey(provincia) ? provinciaMap[provincia] : "";
        }
    }
}
