using System.ComponentModel.DataAnnotations;

namespace Ratio.LogWork.Entity
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
