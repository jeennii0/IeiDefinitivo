using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Iei.ModelosFuentesOriginales;
using System.Text.RegularExpressions;
using System.Web;
using Iei.Models;
using Iei.Services;
using Iei.Wrappers;
using Newtonsoft.Json;
using OpenQA.Selenium.DevTools.V129.Network;
using Iei.Extractors.ValidacionMonumentos;

namespace Iei.Extractors
{
    public class CLEExtractor
    {
        private GeocodingService geocodingService = new GeocodingService();

        public async Task<List<Monumento>> ExtractData(List<ModeloXMLOriginal> monumentosXml)
        {
            try
            {
                var monumentos = new List<Monumento>();
                foreach (ModeloXMLOriginal monumento in monumentosXml)
                {
                    var nuevoMonumento = new Monumento
                    {
                        Nombre = monumento.Nombre?.ToString() ?? "",
                        Direccion = monumento.Calle?.ToString() ?? "",
                        CodigoPostal = monumento.CodigoPostal?.ToString() ?? "",
                        Descripcion = ProcesarDescripcion(monumento.Descripcion?.ToString() ?? ""),
                        Latitud = (double)(monumento.Coordenadas?.Latitud),
                        Longitud = (double)(monumento.Coordenadas?.Longitud),
                        Tipo = ConvertirTipoMonumento(monumento.TipoMonumento),
                        Localidad = new Localidad
                        {
                            Nombre = monumento.Poblacion.Localidad?.ToString() ?? "",
                            Provincia = new Provincia
                            {
                                Nombre = monumento.Poblacion.Provincia?.ToString() ?? ""
                            }
                        }
                    };

                    // Validar nombre y descripción
                    if (!ValidacionesMonumentos.EsMonumentoInicialValido(nuevoMonumento.Nombre, nuevoMonumento.Descripcion)) continue;

                    // Validar coordenadas geográficas
                    if (!ValidacionesMonumentos.ValidarCoordenadas(nuevoMonumento.Latitud, nuevoMonumento.Longitud)) continue;

                    // Completar datos faltantes con geocodificación
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                    {
                        var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                        if (string.IsNullOrEmpty(nuevoMonumento.Direccion)) nuevoMonumento.Direccion = address;
                        if (string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)) nuevoMonumento.CodigoPostal = postcode;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre)) nuevoMonumento.Localidad.Nombre = locality;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre)) nuevoMonumento.Localidad.Provincia.Nombre = province;
                    }

                    // Corregir el código postal si es de 4 dígitos
                    nuevoMonumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(nuevoMonumento.CodigoPostal);

                    // Validar dirección y código postal
                    string localidad = nuevoMonumento.Localidad.Nombre;
                    string provincia = nuevoMonumento.Localidad.Provincia.Nombre;
                    if (!ValidacionesMonumentos.SonDatosDireccionValidos(nuevoMonumento.Nombre, nuevoMonumento.CodigoPostal, nuevoMonumento.Direccion, localidad, provincia)) continue;

                    // Validar códigos postales específicos de Castilla y León
                    if (!ValidacionesMonumentos.EsCodigoPostalCorrectoParaRegion(nuevoMonumento.CodigoPostal, "CLE")) {
                        Console.WriteLine($"Se descarta el monumento '{nuevoMonumento.Nombre}': el código postal '{nuevoMonumento.CodigoPostal}' no es válido para Castilla y León."); 
                        continue; }

                    // Si todas las validaciones pasan, añadir el monumento a la lista final
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
                { "Yacimientos arqueológico", "Yacimientos arqueológicos" },
                { "Casa", "Edificio singular" },
                { "Casas Nobles", "Edificio singular" },
                { "Ermitas", "Iglesia-Ermita" },
                { "Iglesias", "Iglesia-Ermita" },
                { "Catedral", "Monasterio-Convento" },
                { "Torre", "Castillo-Fortaleza-Torre" },
                { "Muralla", "Castillo-Fortaleza-Torre" },
                { "Castillos", "Castillo-Fortaleza-Torre" },
                { "Puerta", "Castillo-Fortaleza-Torre" },
                { "Palacios", "Edificio singular" },
                { "Puentes", "Puente" },
                { "Santuario", "Monasterio-Convento" },
                { "Monasterios", "Monasterio-Convento" }
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

        private string ProcesarDescripcion(string descripcionHtml)
        {

            if (string.IsNullOrWhiteSpace(descripcionHtml))
                return "Desconocida";

            var textoDecodificado = HttpUtility.HtmlDecode(descripcionHtml);
            var textoLimpio = Regex.Replace(textoDecodificado, "<.*?>", string.Empty);

            
            return textoLimpio.Trim();
        }
    }
}