using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Iei
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ruta al chromedriver (asegúrate de poner la ruta correcta de tu instalación)
            string driverPath = @"C:\path\to\chromedriver";

            // Inicializar el navegador Chrome
            ChromeOptions options = new ChromeOptions();
            IWebDriver driver = new ChromeDriver(driverPath, options);

            // Navegar al sitio web de LatLong.net
            driver.Navigate().GoToUrl("https://www.latlong.net/convert-utm-to-lat-long.html");

            // Esperar a que la página cargue completamente
            System.Threading.Thread.Sleep(2000);  // Usar WebDriverWait para mejor control

            // Localizar los campos de entrada para las coordenadas UTM
            IWebElement utmEsteInput = driver.FindElement(By.Name("utm_e"));
            IWebElement utmNorteInput = driver.FindElement(By.Name("utm_n"));
            IWebElement zonaInput = driver.FindElement(By.Name("utm_zone"));

            // Ingresar coordenadas UTM en los campos (modifica estos valores según tus datos)
            utmEsteInput.SendKeys("500000");  // Reemplaza con tu valor de UTM Este
            utmNorteInput.SendKeys("4649776");  // Reemplaza con tu valor de UTM Norte
            zonaInput.SendKeys("30T");  // Reemplaza con la zona UTM adecuada

            // Esperar un momento para que se actualicen los resultados
            System.Threading.Thread.Sleep(2000);  // Usar WebDriverWait para mejor control

            // Localizar el campo donde se muestra la latitud y longitud
            IWebElement latitudElement = driver.FindElement(By.Id("lat"));
            IWebElement longitudElement = driver.FindElement(By.Id("lon"));

            // Obtener los valores de latitud y longitud
            string latitud = latitudElement.Text;
            string longitud = longitudElement.Text;

            // Mostrar los resultados
            Console.WriteLine($"Latitud: {latitud}");
            Console.WriteLine($"Longitud: {longitud}");

            // Cerrar el navegador
            driver.Quit();
        }
    }
}

