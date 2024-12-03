using Iei.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Xml;

namespace Iei.Wrappers
{
    public class CsvWrapper
    {
        public List<ModeloCSV> ParseMonumentosCsv()
        {
            var modelos = new List<ModeloCSV>();

            string filePath = "FuentesDeDatos/bienes_inmuebles_interes_cultural.csv";

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader) // Skip the header line
                    {
                        isHeader = false;
                        continue;
                    }

                    var columns = line.Split(';');

                    // Ensure sufficient columns
                    if (columns.Length < 10)
                        continue;

                    modelos.Add(new ModeloCSV
                    {
                        Denominacion = columns[1],
                        Provincia = columns[2],
                        Municipio = columns[3],
                        UtmEste = double.TryParse(columns[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var este) ? este : (double?)null,
                        UtmNorte = double.TryParse(columns[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var norte) ? norte : (double?)null,
                        Categoria = columns[9],
                        Clasificacion = columns[7]
                    });
                }
            }

            return modelos;
        }
    }
}
