using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Iei.Wrappers
{
    public class CLEWrapper
    {
        public List<ModeloXMLOriginal> ConvertXmlToJson()
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
           
                return monumentos;
        
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
                    CoordenadasXML coordenadas = new CoordenadasXML
                    {
                        // Verificar que los valores de latitud y longitud sean válidos
                        Latitud = ParseCoordenada(coordenadasNode["latitud"]?.InnerText),
                        Longitud =ParseCoordenada(coordenadasNode["longitud"]?.InnerText)
                    };
                    monumento.Coordenadas = coordenadas;
                }

                // Añadir el monumento a la lista
                monumentos.Add(monumento);
            }

            return monumentos;
        }
        private double ParseCoordenada(string coordenada)
        {
            if (string.IsNullOrWhiteSpace(coordenada))
                return 0.0;

            coordenada = new string(coordenada.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());

            coordenada = coordenada.Replace(',', '.');

            if (double.TryParse(coordenada, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            Console.WriteLine($"Coordenada inválida: {coordenada}");
            return 0.0;
        }

    }
}
