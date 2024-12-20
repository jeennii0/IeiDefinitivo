namespace Iei.Extractors.ValidacionMonumentos
{
    public static class ValidacionesMonumentos
    {
        // Valida los datos iniciales del monumento
        public static bool EsMonumentoInicialValido(string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("Se descarta el monumento: no tiene nombre.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': la descripci�n no es v�lida.");
                return false;
            }

            return true;
        }

        // Valida los datos relacionados con la direcci�n y el c�digo postal del monumento
        public static bool SonDatosDireccionValidos(string nombre, string codigoPostal, string direccion, string localidad, string provincia)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': el c�digo postal est� vac�o.");
                return false;
            }

            if (!EsCodigoPostalValido(codigoPostal))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': el c�digo postal '{codigoPostal}' no es v�lido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': la provincia est� vac�a.");
                return false;
            }


            if (string.IsNullOrWhiteSpace(direccion))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': la direcci�n est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(localidad))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': la localidad est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                Console.WriteLine($"Se descarta el monumento '{nombre}': la provincia est� vac�a.");
                return false;
            }

            return true;
        }

        // Completa un c�digo postal de 4 d�gitos con un 0 al inicio
        public static string CompletarCodigoPostal(string codigoPostal)
        {
            codigoPostal = codigoPostal.Trim();
            if (codigoPostal.Length == 4)
            {
                codigoPostal = "0" + codigoPostal;
                Console.WriteLine($"El c�digo postal se complet� a 5 d�gitos: '{codigoPostal}'.");
            }
            return codigoPostal;
        }

        // Valida si un c�digo postal tiene el formato correcto
        public static bool EsCodigoPostalValido(string codigoPostal)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                Console.WriteLine("El c�digo postal est� vac�o o solo contiene espacios en blanco.");
                return false;
            }

            codigoPostal = codigoPostal.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                Console.WriteLine($"El c�digo postal '{codigoPostal}' no tiene un formato v�lido.");
                return false;
            }


            return true;
        }

        // Verifica si un c�digo postal es correcto para una regi�n espec�fica
        public static bool EsCodigoPostalCorrectoParaRegion(string codigoPostal, string fuente)
        {
            switch (fuente)
            {
                case "CV":
                    return codigoPostal.StartsWith("03") || codigoPostal.StartsWith("12") || codigoPostal.StartsWith("46");

                case "CLE":
                    return codigoPostal.StartsWith("05") || codigoPostal.StartsWith("09") || codigoPostal.StartsWith("24") ||
                           codigoPostal.StartsWith("34") || codigoPostal.StartsWith("37") || codigoPostal.StartsWith("40") ||
                           codigoPostal.StartsWith("42") || codigoPostal.StartsWith("47") || codigoPostal.StartsWith("49");

                case "EUS":
                    return codigoPostal.StartsWith("01") || codigoPostal.StartsWith("20") || codigoPostal.StartsWith("48");

                default:
                    Console.WriteLine($"No hay validaci�n espec�fica para la provincia '{fuente}'.");
                    return true;
            }
        }

        // Valida las coordenadas UTM del monumento
        public static bool EsCoordenadaUtmValida(double? utmEste, double? utmNorte)
        {
            if (!utmEste.HasValue || !utmNorte.HasValue)
            {
                Console.WriteLine("Las coordenadas UTM est�n incompletas: faltan valores de Este o Norte.");
                return false;
            }

            if (double.IsNaN(utmEste.Value) || double.IsNaN(utmNorte.Value))
            {
                Console.WriteLine("Las coordenadas UTM contienen valores no num�ricos.");
                return false;
            }

            if (utmEste.Value == 0 || utmNorte.Value == 0)
            {
                Console.WriteLine("Las coordenadas UTM no pueden ser cero.");
                return false;
            }

            return true;
        }

        // Valida si las coordenadas geogr�ficas est�n en un rango aceptable
        public static bool ValidarCoordenadas(double latitud, double longitud)
        {
            if (latitud < -90 || latitud > 90)
            {
                Console.WriteLine($"La latitud '{latitud}' est� fuera del rango permitido (-90 a 90).");
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                Console.WriteLine($"La longitud '{longitud}' est� fuera del rango permitido (-180 a 180).");
                return false;
            }

            return true;
        }
    }
}
