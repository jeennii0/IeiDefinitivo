using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Iei.Modelos_Fuentes;
using Iei.Models;
using Iei.Services;
using Iei.Wrappers;
using Newtonsoft.Json;
using Iei.Extractors.ValidacionMonumentos;

namespace Iei.Extractors
{
    public class EUSExtractor
    {
        public EUSWrapper jsonWrapper = new EUSWrapper();
        private GeocodingService geocodingService = new GeocodingService();

        public async Task<List<Monumento>> ExtractDataAsync(List<ModeloJSONOriginal> monumentosJson)
        {
            try
            {
                var monumentos = new List<Monumento>();
                foreach (ModeloJSONOriginal monumento in monumentosJson)
                {
                    // Validar datos iniciales
                    if (!ValidarDatosIniciales(monumento)) continue;

                    // Crear el nuevo objeto Monumento
                    var nuevoMonumento = new Monumento
                    {
                        Nombre = monumento.DocumentName?.ToString() ?? "",
                        Direccion = monumento.Address?.ToString() ?? "",
                        CodigoPostal = monumento.PostalCode?.ToString() ?? "",
                        Descripcion = monumento.DocumentDescription?.ToString() ?? "",
                        Latitud = monumento.Latwgs84,
                        Longitud = monumento.Lonwgs84,
                        Tipo = ConvertirTipoMonumento(monumento.DocumentName),
                        Localidad = new Localidad
                        {
                            Nombre = monumento.Municipality?.ToString() ?? "",
                            Provincia = new Provincia { Nombre = monumento.Territory?.ToString() ?? "" }
                        }
                    };

                    // Si los campos dirección, código postal, localidad o provincia están vacíos, se realiza una geocodificación
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                    {
                        var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                        // Asignar los valores obtenidos de la geocodificación, si no están vacíos
                        if (string.IsNullOrEmpty(nuevoMonumento.Direccion)) nuevoMonumento.Direccion = address;
                        if (string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)) nuevoMonumento.CodigoPostal = postcode;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre)) nuevoMonumento.Localidad.Nombre = locality;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre)) nuevoMonumento.Localidad.Provincia.Nombre = province;
                    }

                    // Validar código postal
                    if (!ValidacionesMonumentos.EsCodigoPostalValido(nuevoMonumento.CodigoPostal))
                    {
                        Console.WriteLine($"No hay código postal válido para el monumento {monumento.DocumentName}. Saltando este monumento.");
                        continue;
                    }

                    // Validar dirección y localidad
                    if (!ValidacionesMonumentos.EsMonumentoDireccionValido(nuevoMonumento.Direccion, nuevoMonumento.Localidad.Nombre, nuevoMonumento.Localidad.Provincia.Nombre))
                    {
                        Console.WriteLine($"Datos inválidos para el monumento {monumento.DocumentName}. Saltando este monumento.");
                        continue;
                    }

                    // Agregar el monumento a la lista si pasa las validaciones
                    monumentos.Add(nuevoMonumento);
                }  // Cierra el ciclo foreach

                return monumentos; // Debe estar fuera del bucle foreach
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al extraer datos del archivo: {ex.Message}");
                return null;
            }
        }


        private bool ValidarDatosIniciales(ModeloJSONOriginal monumento)
        {
            if (!ValidacionesMonumentos.EsMonumentoInicialValido(monumento.DocumentName.ToString(), monumento.DocumentDescription?.ToString())
                || !ValidacionesMonumentos.ValidarCoordenadas(monumento.Latwgs84, monumento.Lonwgs84)
                )
            {
                Console.WriteLine($"Datos incompletos para el monumento {monumento.DocumentName}. Saltando este monumento.");
                return false;
            }

           

            return true;
        }

        public string ConvertirTipoMonumento(string tipoMonumento)
        {
            var tipoMonumentoMap = new Dictionary<string, string>
            {
                { "Castillo", "Castillo-Fortaleza-Torre" },
                { "Ermita", "Iglesia-Ermita" },
                { "Monasterio", "Monasterio-Convento" },
                { "Torre", "Castillo-Fortaleza-Torre" },
                { "Palacio", "Edificio singular" },
                { "Catedral", "Iglesia-Ermita" },
                { "Puente", "Puente" },
                { "Iglesia", "Iglesia-Ermita" },
                { "Basílica", "Iglesia-Ermita" },
                { "Ayuntamiento", "Edificio singular" },
                { "Casa-Torre", "Castillo-Fortaleza-Torre" },
                { "Convento", "Monasterio-Convento" },
                { "Muralla", "Castillo-Fortaleza-Torre" },
                { "Parroquia", "Iglesia-Ermita" },
                { "Santuario", "Iglesia-Ermita" },
                { "Teatro", "Edificio singular" },
                { "Torre-Palacio", "Castillo-Fortaleza-Torre" },
            };

            foreach (var key in tipoMonumentoMap.Keys)
            {
                if (!string.IsNullOrEmpty(tipoMonumento) && tipoMonumento.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return tipoMonumentoMap[key];
                }
            }
            return "Otros";
        }
    }
}