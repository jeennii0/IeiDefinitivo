using Iei.Extractors;
using Iei.Models;
using Iei.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace Iei.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargarDatosController:ControllerBase
    { 
            [HttpGet("convertir-xml-a-json")]
            public IActionResult ConvertirXmlAJson()
            {
                // Crear una instancia de XmlWrapper
                XmlWrapper xmlWrapper = new XmlWrapper();

                // Llamar al método para convertir XML a JSON
                string json = xmlWrapper.ConvertXmlToJson();

                Console.WriteLine(json);

                ExtractorXml extractorXml = new ExtractorXml();
                List<MonumentoModificado> lista = extractorXml.ExtractData();

                // Retornar el JSON como respuesta
                return Ok(lista);
            }
        }
    }



