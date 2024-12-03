namespace Iei.Models
{
    public class Localidad : Base<int>
    {
        public string Nombre { get; set; } = string.Empty;
        public int ProvinciaId { get; set; }
        public Provincia? Provincia { get; set; }
        public ICollection<Monumento> Monumentos { get; set; } = new List<Monumento>();
    }
}

