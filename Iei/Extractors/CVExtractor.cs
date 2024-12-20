using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iei.Models;
using Iei.Extractors.ValidacionMonumentos;
using Convertidor;
using Iei.Services;

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
                if (!ValidacionesMonumentos.EsMonumentoInicialValido(monumento.Denominacion, monumento.Clasificacion)) continue;

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

                // Validación de la provincia
                if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    Console.WriteLine($"Se descarta el monumento '{nuevoMonumento.Nombre}': la provincia está vacía.");
                    continue;
                }

                // Asignar coordenadas si están disponibles
                if (!await AsignarCoordenadasAsync(nuevoMonumento, monumento, convertidor)) continue;

                // Intentar asignar los datos de dirección mediante geocodificación si están vacíos
                if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) || string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)
                    || string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre) || string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    await AsignarGeocodificacionAsync(nuevoMonumento);
                }

                string provincia = nuevoMonumento.Localidad.Provincia.Nombre;
                string localidad = nuevoMonumento.Localidad.Nombre;

                // Validar la dirección y código postal
                if (!ValidacionesMonumentos.SonDatosDireccionValidos(nuevoMonumento.Nombre, nuevoMonumento.CodigoPostal, nuevoMonumento.Direccion, localidad, provincia)) continue;

                // Validar si el código postal es correcto para la Comunidad Valenciana
                if (!ValidacionesMonumentos.EsCodigoPostalCorrectoParaRegion(nuevoMonumento.CodigoPostal, "CV"))
                {
                    Console.WriteLine($"Se descarta el monumento '{nuevoMonumento.Nombre}': el código postal '{nuevoMonumento.CodigoPostal}' no es válido para la Comunidad Valenciana.");
                    continue;
                }

                // Si todo está validado correctamente, agregamos el monumento a la lista
                monumentos.Add(nuevoMonumento);
            }

            return monumentos;
        }

        private async Task<bool> AsignarCoordenadasAsync(Monumento nuevoMonumento, ModeloCSVOriginal monumento, Convertidor.Convertidor convertidor)
        {
            try
            {
                if (!ValidacionesMonumentos.EsCoordenadaUtmValida(monumento.UtmEste, monumento.UtmNorte))
                {
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
                    Console.WriteLine($"La direccion es {address} y el codigo postal {postcode}");
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
                    nuevoMonumento.Localidad.Provincia.Nombre = NormalizarProvincia(province);
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
