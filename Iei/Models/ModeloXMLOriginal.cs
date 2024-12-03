namespace Iei.Models
{
    public class ModeloXMLOriginal
    {
            public string Nombre { get; set; }
            public string TipoMonumento { get; set; }
            public string Calle { get; set; }
            public string CodigoPostal { get; set; }
            public string Descripcion { get; set; }

            public PoblacionXML Poblacion { get; set; }
            
            public CoordenadasXml Coordenadas { get; set; }
        }

       
    

    }

