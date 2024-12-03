namespace Iei.Models
{
    public class Provincia : Base <int>
    {
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Localidad> Localidades { get; set; } = new List<Localidad>();
    }
}
