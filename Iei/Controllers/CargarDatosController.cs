using Iei.Extractors;
using Iei.Modelos_Fuentes;
using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Iei.Services;
using Iei.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace Iei.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargarDatosController:ControllerBase
    {
        private readonly MonumentoService _monumentoService;
        public CargarDatosController(MonumentoService monumentoService)
        {
            _monumentoService = monumentoService;
        }
        [HttpGet("convertir-datos")]
        public async Task<IActionResult> ConvertirYInsertarDatos([FromQuery] string source)
        {
            try
            {
                List<Monumento> monumentos = new List<Monumento>();

                // Dependiendo de la fuente, obtenemos los monumentos
                if (source.Equals("xml", StringComparison.OrdinalIgnoreCase))
                {
                    CLEWrapper xmlWrapper = new CLEWrapper();
                    List<ModeloXMLOriginal> xmlData = xmlWrapper.ConvertXmlToJson();
                    CLEExtractor extractorXml = new CLEExtractor();
                    monumentos = await extractorXml.ExtractData(xmlData);
                }
                else if (source.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    EUSWrapper jsonWrapper = new EUSWrapper();
                    List<ModeloJSONOriginal> jsonEus = jsonWrapper.GenerateProcessedJson();
                    EUSExtractor extractorJson = new EUSExtractor();
                    monumentos = await extractorJson.ExtractDataAsync(jsonEus);
                }
                else if (source.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    CVWrapper csvWrapper = new CVWrapper();
                    List<ModeloCSVOriginal> csvData = csvWrapper.ParseMonumentosCsv();
                    CVExtractor extractorCsv = new CVExtractor();
                    monumentos = await extractorCsv.ExtractData(csvData);
                }
                else
                {
                    return BadRequest("Parámetro 'source' no válido. Use 'xml', 'json' o 'csv'.");
                }

                // Insertamos los monumentos en la base de datos
                await _monumentoService.InsertarMonumento(monumentos);

                // Retornamos una respuesta exitosa
                return Ok(new { message = $"{monumentos.Count} monumentos insertados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
        }
    



