using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Iei.Modelos_Fuentes;

public class EUSWrapper
{
    // Método principal que devuelve un JSON procesado sin duplicados
    public List<ModeloJSONOriginal> GenerateProcessedJson()
    {
        // Cargar el archivo JSON y unificar las propiedades duplicadas
        var jsonContent = LoadAndMergeJsonProperties();

        // Deserializar el JSON modificado
        List<ModeloJSONOriginal> monumentosJson = JsonConvert.DeserializeObject<List<ModeloJSONOriginal>>(jsonContent);
        return monumentosJson;

    }

    // Método para cargar y unir las propiedades duplicadas en el JSON
    // Método para cargar y unir las propiedades duplicadas en el JSON
    private string LoadAndMergeJsonProperties()
    {
        // Ruta del archivo JSON original
        string filePath = "FuentesDeDatos/edificios.json";

        // Leer el contenido del archivo JSON
        string jsonContent = File.ReadAllText(filePath);

        // Usar JsonTextReader para procesar el JSON de forma más controlada
        using (var reader = new JsonTextReader(new StringReader(jsonContent)))
        {
            var jsonObjects = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var currentObject = new Dictionary<string, object>();

                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            string propertyName = reader.Value.ToString();
                            reader.Read();

                            // Leer el valor
                            var currentValue = reader.Value?.ToString();


                            // Leer y unir las propiedades duplicadas
                            if (propertyName.Equals("address", StringComparison.OrdinalIgnoreCase) ||
                                propertyName.Equals("postalCode", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(currentValue)) // Agregar solo si el valor no está vacío o nulo
                                {
                                    if (currentObject.ContainsKey(propertyName))
                                    {
                                        // Si ya existe, unir los valores
                                        currentObject[propertyName] = currentObject[propertyName]?.ToString() + " " + currentValue;
                                    }
                                    else
                                    {
                                        // Si no existe, agregar la propiedad
                                        currentObject[propertyName] = currentValue;
                                    }
                                }
                            }
                            else
                            {
                                // Agregar las propiedades que no son address ni postalCode
                                currentObject[propertyName] = currentValue;
                            }
                        }
                    }

                    jsonObjects.Add(currentObject);
                }
            }

            // Serializar la lista de objetos combinados nuevamente a un JSON
            return JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
        }
    }

    // Método para convertir una lista de objetos en un JSON
    private string ConvertToJson(List<ModeloJSONOriginal> data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }
}