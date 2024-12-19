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
using Convertidor;
using Iei.Services;
using Iei.Extractors.ValidacionMonumentos;

namespace Iei.Extractors
{
    public class CVExtractor
    {
        public CVExtractor() { }

        private GeocodingService geocodingService = new GeocodingService();

        public async Task<List<Monumento>> ExtractData(List<ModeloCSVOriginal> monumentosCsv)
        {
            var monumentos = new List<Monumento>();
            var convertidor = new Convertidor.Convertidor();

            foreach (var monumento in monumentosCsv)
            {
                if (!ValidarDatosIniciales(monumento)) continue;

                var nuevoMonumento = new Monumento
                {
                    Nombre = monumento.Denominacion,
                    Descripcion = monumento.Clasificacion,
                    Tipo = ConvertirTipoMonumento(monumento.Categoria),
                    Localidad = new Localidad
                    {
                        Nombre = monumento.Municipio,
                        Provincia = new Provincia
                        {
                            Nombre = NormalizarProvincia(monumento.Provincia)
                        }
                    }
                };

                if (!await AsignarCoordenadasAsync(nuevoMonumento, monumento, convertidor)) continue;

                // Si la dirección, código postal, localidad o provincia están vacíos o nulos, intenta asignarlos mediante geocodificación
                if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) || string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)
                    || string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre) || string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    await AsignarGeocodificacionAsync(nuevoMonumento);
                }

                // Validar código postal
                if (!ValidacionesMonumentos.EsCodigoPostalValido(nuevoMonumento.CodigoPostal))
                {
                    Console.WriteLine($"Código postal no válido para el monumento {nuevoMonumento.Nombre}. Saltando este monumento.");
                    continue; // Si el código postal es inválido, saltamos este monumento
                }

                // Validar la localidad
                if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre))
                {
                    Console.WriteLine($"Datos incompletos para la localidad del monumento {nuevoMonumento.Nombre}. Saltando este monumento.");
                    continue; // Si la localidad está vacía, saltamos este monumento
                }

                // Validar la provincia
                if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    Console.WriteLine($"Datos incompletos para la provincia del monumento {nuevoMonumento.Nombre}. Saltando este monumento.");
                    continue; // Si la provincia está vacía, saltamos este monumento
                }

                // Si todo está validado correctamente, agregamos el monumento a la lista
                monumentos.Add(nuevoMonumento);
            }

            return monumentos; 
        }

        private bool ValidarDatosIniciales(ModeloCSVOriginal monumento)
        {
            if (!ValidacionesMonumentos.EsMonumentoInicialValido(monumento.Denominacion, monumento.Clasificacion))
            {
                Console.WriteLine($"Datos incompletos para el monumento {monumento.Denominacion}. Saltando este monumento.");
                return false;
            }

            var provinciaNormalizada = NormalizarProvincia(monumento.Provincia?.ToString() ?? "");
            if (string.IsNullOrEmpty(provinciaNormalizada))
            {
                Console.WriteLine($"Provincia no válida para el monumento {monumento.Denominacion}. Saltando este monumento.");
                return false;
            }

            return true;
        }

        private async Task<bool> AsignarCoordenadasAsync(Monumento nuevoMonumento, ModeloCSVOriginal monumento, Convertidor.Convertidor convertidor)
        {
            try
            {
                if (!ValidacionesMonumentos.EsCoordenadaUtmValida(monumento.UtmEste, monumento.UtmNorte))
                {
                    Console.WriteLine($"Datos UTM inválidos para el monumento {monumento.Denominacion}.");
                    return false;
                }

                var coordenadas = await convertidor.ConvertUTMToLatLong(monumento.UtmEste.ToString(), monumento.UtmNorte.ToString());
                nuevoMonumento.Latitud = coordenadas.latitud;
                nuevoMonumento.Longitud = coordenadas.longitud;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al convertir UTM a lat/long para el monumento {monumento.Denominacion}: {ex.Message}");
                return false;
            }
        }

        private async Task AsignarGeocodificacionAsync(Monumento nuevoMonumento)
        {
            try
            {
                var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(postcode))
                {
                    Console.WriteLine("La API de geocodificación no ha devuelto una dirección válida.");
                }

                nuevoMonumento.Direccion = address ?? "";
                nuevoMonumento.CodigoPostal = postcode ?? "";
                if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre) && !string.IsNullOrEmpty(locality))
                {
                    nuevoMonumento.Localidad.Nombre = locality;
                }

                if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre) && !string.IsNullOrEmpty(province))
                {
                    nuevoMonumento.Localidad.Provincia.Nombre = province;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la geocodificación: {ex.Message}");
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
                { "Valencia", "Valencia" },
                { "Alacant", "Alicante" },
                { "Castellon", "Castellón" },
                { "València ", "Valencia" }
            };

            return provinciaMap.ContainsKey(provincia) ? provinciaMap[provincia] : "";
        }
    }
}
