using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Globalization;
namespace Convertidor
{
    public class Convertidor
    {
        private IWebDriver driver;

        // Inicializa y configura el ChromeDriver
        public Convertidor()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless"); // Ejecutar en modo sin cabeza (sin ventana visible)
            options.AddArgument("--disable-web-security");
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--incognito");
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options); // Inicializa el ChromeDriver
        }

        // Método para convertir coordenadas UTM a Latitud y Longitud
        public async Task<(double latitud, double longitud)> ConvertUTMToLatLong(string utmX, string utmY)
        {
            try
            {
                Console.WriteLine("Navegando a la página para la conversión de coordenadas...");
                driver.Navigate().GoToUrl("https://www.ign.es/web/calculadora-geodesica");

                // Esperar hasta que la página cargue completamente
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.Id("combo_tipo")));

                // Seleccionar el radio button para UTM
                IWebElement radioButton = driver.FindElement(By.Id("utm"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", radioButton);
                Actions actions1 = new Actions(driver);
                actions1.MoveToElement(radioButton).Click().Perform();

                // Introducir las coordenadas UTM
                driver.FindElement(By.Id("datacoord1")).SendKeys(utmY);  // Coordenada Y (Norte)
                driver.FindElement(By.Id("datacoord2")).SendKeys(utmX);  // Coordenada X (Este)

                // Esperar hasta que el botón de conversión esté disponible
                var waitt = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                waitt.Until(d => d.FindElement(By.Id("trd_calc")));

                // Hacer clic en el botón para calcular
                IWebElement trdCalcButton = driver.FindElement(By.Id("trd_calc"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", trdCalcButton);

                // Esperar hasta que los valores de latitud y longitud estén disponibles
                wait.Until(d => d.FindElement(By.Id("txt_etrs89_longd")).GetAttribute("value") != "");

                // Obtener las coordenadas de latitud y longitud
                var latitudStr = driver.FindElement(By.Id("txt_etrs89_latgd")).GetAttribute("value");
                var longitudStr = driver.FindElement(By.Id("txt_etrs89_longd")).GetAttribute("value");

                // Verificar si las coordenadas fueron extraídas correctamente
                if (string.IsNullOrEmpty(latitudStr) || string.IsNullOrEmpty(longitudStr))
                {
                    throw new Exception("No se pudo extraer la latitud o longitud correctamente.");
                }

                // Convertir las coordenadas a formato double
                double latitud = Convert.ToDouble(latitudStr, CultureInfo.InvariantCulture);
                double longitud = Convert.ToDouble(longitudStr, CultureInfo.InvariantCulture);

                Console.WriteLine($"Latitud: {latitud}");
                Console.WriteLine($"Longitud: {longitud}");

                return (latitud, longitud);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la conversión de coordenadas UTM: {ex.Message}");
                throw;
            }
        }

        // Método para cerrar el driver después de que todas las conversiones se hayan realizado
        public void CloseDriver()
        {
            driver?.Quit(); // Cerrar el navegador al final de todo
        }
    }
}
