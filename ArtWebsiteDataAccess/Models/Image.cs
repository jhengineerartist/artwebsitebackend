// Create a model class for image entity
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtWebsiteDataAccess.Models
{
    [Table("Images")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = "Untitled";

        [Required]
        [MaxLength(1000)]
        public string Caption { get; set; } = "Uncaptioned";

        [Required]
        [MaxLength(255)]
        public string Class { get; set; } = "WorkInProgress";

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "int[]")]
        public List<int> Related { get; set; } = new List<int>();

        [Required]
        [Column(TypeName = "varchar[]")]
        public List<string> Tags { get; set; } = new List<string>();

        [MaxLength(2083)]
        public string Url { get; set; } = "";
    }
}