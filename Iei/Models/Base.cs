using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Iei.Models
{
    public abstract class Base<TId>
    {
        [Required]
        public Guid Guid { get; set; } = Guid.NewGuid();

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TId Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedTimeUtc { get; set; }

        [ConcurrencyCheck]
        public DateTime LastUpdateUtc { get; set; }
    }

}