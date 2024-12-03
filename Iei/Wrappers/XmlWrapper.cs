using Iei.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Iei.Wrappers
{
    public class XmlWrapper
    {
        public string ConvertXmlToJson()
        {
                // Obtener la ruta de la raíz del proyecto
                string projectRoot = Directory.GetCurrentDirectory();

                // Construir la ruta al archivo XML
                string filePath = "FuentesDeDatos/monumentos.xml";
               
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("El archivo XML no se encontró.", filePath);
                }

                // Cargar el archivo XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                // Convertir XML a un objeto de tipo ModeloXMLOriginal
                List<ModeloXMLOriginal> monumentos = ParseMonumentosXml(xmlDoc);

                // Convertir la lista de objetos a JSON
                string json = JsonConvert.SerializeObject(monumentos, Newtonsoft.Json.Formatting.Indented);

                // Devolver el JSON
                return json;
        
        }



        private List<ModeloXMLOriginal> ParseMonumentosXml(XmlDocument xmlDoc)
        {
            List<ModeloXMLOriginal> monumentos = new List<ModeloXMLOriginal>();

            // Navegar a los elementos <monumento>
            XmlNodeList monumentosNodes = xmlDoc.GetElementsByTagName("monumento");

            foreach (XmlNode monumentoNode in monumentosNodes)
            {
                ModeloXMLOriginal monumento = new ModeloXMLOriginal();

                // Parsear los datos de cada monumento
                monumento.Nombre = monumentoNode["nombre"]?.InnerText;
                monumento.TipoMonumento = monumentoNode["tipoMonumento"]?.InnerText;
                monumento.Calle = monumentoNode["calle"]?.InnerText;
                monumento.CodigoPostal = monumentoNode["codigoPostal"]?.InnerText;
                monumento.Descripcion = monumentoNode["Descripcion"]?.InnerText;

                // Parsear los datos de la población (verificar que el nodo exista)
                XmlNode poblacionNode = monumentoNode["poblacion"];
                if (poblacionNode != null)
                {
                    PoblacionXML poblacion = new PoblacionXML
                    {
                        Provincia = poblacionNode["provincia"]?.InnerText,
                        Municipio = poblacionNode["municipio"]?.InnerText,
                        Localidad = poblacionNode["localidad"]?.InnerText
                    };
                    monumento.Poblacion = poblacion;
                }

                // Parsear las coordenadas (verificar que el nodo exista)
                XmlNode coordenadasNode = monumentoNode["coordenadas"];
                if (coordenadasNode != null)
                {
                    CoordenadasXml coordenadas = new CoordenadasXml
                    {
                        // Verificar que los valores de latitud y longitud sean válidos
                        Latitud = double.TryParse(coordenadasNode["latitud"]?.InnerText, out double latitud) ? latitud : 0.0,
                        Longitud = double.TryParse(coordenadasNode["longitud"]?.InnerText, out double longitud) ? longitud : 0.0
                    };
                    monumento.Coordenadas = coordenadas;
                }

                // Añadir el monumento a la lista
                monumentos.Add(monumento);
            }

            return monumentos;
        }

    }
}
