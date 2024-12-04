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
    public class ExtractorCsv
    {
        private IWebDriver _driver;

        public ExtractorCsv()
        {
            InitializeDriver();
        }

        public async Task<List<Monumento>> ExtractData(List<ModeloCSV> monumentosCsv)
        {
            try
            {
                var monumentos = new List<Monumento>();
                foreach (var monumento in monumentosCsv)
                {
                    // Convertir coordenadas UTM a WGS84 si están disponibles
                    (double latitud, double longitud) = monumento.UtmEste != null && monumento.UtmNorte != null
                        ? await ScrapeUTMtoWGS84(monumento.UtmEste.ToString(), monumento.UtmNorte.ToString())
                        : (0.0, 0.0);

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
                        Latitud = latitud,
                        Longitud = longitud
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
                _driver?.Quit();
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

        private void InitializeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--incognito");
            _driver = new ChromeDriver(options);
        }

        public async Task<(double latitud, double longitud)> ScrapeUTMtoWGS84(string utmX, string utmY)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.ign.es/web/calculadora-geodesica");

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));  // Aumenta el tiempo de espera
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("combo_tipo")));

                // Seleccionar UTM
                var utmButton = _driver.FindElement(By.Id("utm"));
                utmButton.Click();

                // Ingresar coordenadas UTM
                _driver.FindElement(By.Id("datacoord1")).SendKeys(utmY);
                _driver.FindElement(By.Id("datacoord2")).SendKeys(utmX);

                // Calcular
                _driver.FindElement(By.Id("trd_calc")).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("txt_etrs89_latgd")));

                // Obtener resultados
                var latitudStr = _driver.FindElement(By.Id("txt_etrs89_latgd")).GetAttribute("value");
                var longitudStr = _driver.FindElement(By.Id("txt_etrs89_longd")).GetAttribute("value");

                // Imprimir los valores obtenidos para depurar
                Console.WriteLine($"Latitud: {latitudStr}, Longitud: {longitudStr}");

                if (!double.TryParse(latitudStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitud))
                    throw new Exception("No se pudo obtener la latitud.");
                if (!double.TryParse(longitudStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitud))
                    throw new Exception("No se pudo obtener la longitud.");

                return (latitud, longitud);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Selenium: {ex.Message}");
                return (0.0, 0.0);
            }
        }

    }
}