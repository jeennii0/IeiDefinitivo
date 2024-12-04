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
using UTMtoLatLongScraper;

namespace Iei.Extractors
{
    public class CVExtractor
    {
        public CVExtractor()
        {

        }

        public async Task<List<Monumento>> ExtractData(List<ModeloCSVOriginal> monumentosCsv)
        {
            try
            {
                var monumentos = new List<Monumento>();

                // Crear una instancia de UTMConverter para la conversión
                UTMConverter converter = new UTMConverter();

                foreach (var monumento in monumentosCsv)
                {
                    // Obtener las coordenadas UTM (asegúrate de tenerlas en tu CSV o ajusta según tus datos)
                    double utmEste = (double)monumento.UtmEste;  // Asegúrate de que estas propiedades estén en tu modelo
                    double utmNorte = (double)monumento.UtmNorte;
                    string zonaUTM = monumento.Provincia; // Asegúrate de tener la zona UTM

                    // Llamar a ConvertirUTMtoLatLong para obtener la latitud y longitud
                    var coordenadas = converter.ConvertirUTMtoLatLong(utmEste, utmNorte, zonaUTM);

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
                                Nombre = monumento.Provincia?.ToString() ?? ""
                            }
                        },
                        // Asignar las coordenadas obtenidas
                        Latitud = coordenadas.latitud,
                        Longitud = coordenadas.longitud
                    };

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
            finally
            {
                // Cualquier código de limpieza si es necesario
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
    }
}
