using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Iei.Models;
using Iei.Wrappers;
using Newtonsoft.Json;
namespace Iei.Extractors
{
    public class ExtractorXml
    {
        public XmlWrapper xmlWrapper = new XmlWrapper();
        public ExtractorXml()
        {

        }
        public List<MonumentoModificado> ExtractData()
        {
            try
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(xmlWrapper.ConvertXmlToJson());
                var monumentos = new List<MonumentoModificado>();
                foreach (var monumento in data)
                {
                    var coordenadas = monumento["Coordenadas"] as Dictionary<string, object>;
                    var poblacion = monumento["Poblacion"] as Dictionary<string, object>;
                    Console.WriteLine(poblacion);
                    var nuevoMonumento = new MonumentoModificado
                    {
                        Nombre = monumento["Nombre"]?.ToString() ?? "",
                        Direccion = monumento["Calle"]?.ToString() ?? "",
                        CodigoPostal = monumento["CodigoPostal"]?.ToString() ?? "",
                        Descripcion = monumento["Descripcion"]?.ToString() ?? "",
                        Latitud = coordenadas != null && coordenadas.ContainsKey("Latitud") ? Convert.ToDouble(coordenadas["Latitud"]) : 0.0,
                        Longitud = coordenadas != null && coordenadas.ContainsKey("Longitud") ? Convert.ToDouble(coordenadas["Longitud"]) : 0.0,
                        Provincia = poblacion != null && poblacion.ContainsKey("Localidad") ? poblacion["Localidad"]?.ToString() : "",
                        Localidad = poblacion != null && poblacion.ContainsKey("Localidad") ? poblacion["Localidad"]?.ToString() : "",
                    };
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
    }
}