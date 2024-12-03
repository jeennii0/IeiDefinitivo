using Iei.Models;

public class Mapper_EUS : IMapper<Modelo_EUS, Monumento>
{
    public Monumento Map(Modelo_EUS source)
    {
        var monumento = new Monumento
        {
            Nombre = source.DocumentName,
            Descripcion = source.DocumentDescription, 
            Longitud = source.Lonwgs84,
            Latitud = source.Latwgs84,
            Direccion = source.Address,
            Localidad = new Localidad 
            { 
                Nombre = source.Municipality,
                Provincia = new Provincia {
                    Nombre = source.Territory
                } 
            },

        };

        return monumento;
    }
}
