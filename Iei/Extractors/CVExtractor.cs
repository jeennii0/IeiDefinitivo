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

namespace Iei.Extractors
{
    public class CVExtractor
    {
        public CVExtractor()
        {

        }
        public async Task <List<Monumento>> ExtractData(List<ModeloCSVOriginal> monumentosCsv)
        {
            try
            {
                var monumentos = new List<Monumento>();
                foreach (var monumento in monumentosCsv)
                {
                    

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
            finally
            {
             
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