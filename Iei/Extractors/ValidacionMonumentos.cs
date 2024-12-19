namespace Iei.Extractors.ValidacionMonumentos
{
    public static class ValidacionesMonumentos
    {
        public static bool EsCodigoPostalValido(string codigoPostal)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal)) return false;
            codigoPostal = codigoPostal.Trim();
            return System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$");
        }

        public static bool EsMonumentoInicialValido(string nombre, string descripcion)
        {
            return !string.IsNullOrWhiteSpace(nombre) &&
                   !string.IsNullOrWhiteSpace(descripcion);
        }

        public static bool EsCoordenadaUtmValida(double? utmEste, double? utmNorte)
        {
            return utmEste.HasValue && utmNorte.HasValue &&
                   !double.IsNaN(utmEste.Value) && !double.IsNaN(utmNorte.Value) &&
                   utmEste.Value != 0 && utmNorte.Value != 0;
        }

        
            // Cambia el método a estático
            public static bool ValidarCoordenadas(double latitud, double longitud)
            {
                // Lógica de validación de coordenadas
                return latitud >= -90 && latitud <= 90 && longitud >= -180 && longitud <= 180;
            }
        


        public static bool EsMonumentoDireccionValido(string direccion, string localidad, string provincia)
        {
            return !string.IsNullOrWhiteSpace(direccion) &&
                   !string.IsNullOrWhiteSpace(localidad) &&
                   !string.IsNullOrWhiteSpace(provincia);
        }

    }
    }