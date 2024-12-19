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

        public static bool EsMonumentoInicialValido(string nombre, string tipo)
        {
            return !string.IsNullOrWhiteSpace(nombre) &&
                   !string.IsNullOrWhiteSpace(tipo);
        }

        public static bool EsCoordenadaUtmValida(double? utmEste, double? utmNorte)
        {
            return utmEste.HasValue && utmNorte.HasValue &&
                   !double.IsNaN(utmEste.Value) && !double.IsNaN(utmNorte.Value) &&
                   utmEste.Value != 0 && utmNorte.Value != 0;
        }

        public static bool EsLocalidadDatosValido(string nombreLocalidad, string nombreProvincia)
        {
            return !string.IsNullOrWhiteSpace(nombreLocalidad) && !string.IsNullOrWhiteSpace(nombreProvincia);
        }
    }

}