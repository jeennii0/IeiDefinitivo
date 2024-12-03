using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Iei.Models;

class Program2
{
    static void Main(string[] args)
    {
        // Ruta del archivo JSON
        string filePath = "FuentesDeDatos/edificios.json";

        // Leer todo el contenido del archivo JSON
        var json = File.ReadAllText(filePath);

        // Deserializar el JSON en una lista de objetos Modelo_EUS
        List<Modelo_EUS> data = JsonConvert.DeserializeObject<List<Modelo_EUS>>(json);

        // Crear el mapper
        IMapper<Modelo_EUS, Monumento> mapper = new Mapper_EUS();

        // Crear una lista para almacenar los monumentos mapeados
        List<Monumento> monumentos = new List<Monumento>();

        // Mapear los datos a los modelos comunes y agregarlos a la lista
        foreach (var item in data)
        {
            var monumento = mapper.Map(item);
            monumentos.Add(monumento);  // Guardar el objeto mapeado en la lista
        }

        // Serializar la lista de monumentos a JSON
        string jsonResult = JsonConvert.SerializeObject(monumentos, Formatting.Indented);

        // Imprimir el JSON en consola o guardarlo en un archivo
        Console.WriteLine(jsonResult);

        // Si deseas guardarlo en un archivo
        File.WriteAllText("Monumentos.json", jsonResult);
    }
}
