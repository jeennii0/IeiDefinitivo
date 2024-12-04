using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace UTMtoLatLongScraper
{
    public class Scrapper
    {
        public (double latitud, double longitud) ConvertirUTMtoLatLong(double utmEste, double utmNorte, string zonaUTM)
        {
            // Ruta al chromedriver (asegúrate de poner la ruta correcta)
            string driverPath = @"C:\drivers\chromedriver\chromedriver.exe";

            // Inicializar el navegador Chrome
            ChromeOptions options = new ChromeOptions();
            using (IWebDriver driver = new ChromeDriver(driverPath, options))
            {
                // Navegar al sitio web de LatLong.net
                driver.Navigate().GoToUrl("https://www.latlong.net/convert-utm-to-lat-long.html");

                // Esperar a que la página cargue completamente
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.Name("utm_e")));

                // Localizar los campos de entrada para las coordenadas UTM
                IWebElement utmEsteInput = driver.FindElement(By.Name("utm_e"));
                IWebElement utmNorteInput = driver.FindElement(By.Name("utm_n"));
                IWebElement zonaInput = driver.FindElement(By.Name("utm_zone"));

                // Ingresar coordenadas UTM en los campos
                utmEsteInput.Clear();
                utmEsteInput.SendKeys(utmEste.ToString());
                utmNorteInput.Clear();
                utmNorteInput.SendKeys(utmNorte.ToString());
                zonaInput.Clear();
                zonaInput.SendKeys(zonaUTM);

                // Esperar a que los resultados se actualicen
                wait.Until(d => d.FindElement(By.Id("lat")));

                // Localizar el campo donde se muestra la latitud y longitud
                IWebElement latitudElement = driver.FindElement(By.Id("lat"));
                IWebElement longitudElement = driver.FindElement(By.Id("lon"));

                // Obtener los valores de latitud y longitud
                double latitud = Convert.ToDouble(latitudElement.Text);
                double longitud = Convert.ToDouble(longitudElement.Text);

                // Retornar las coordenadas como una tupla
                return (latitud, longitud);
            }
        }
    }
}
