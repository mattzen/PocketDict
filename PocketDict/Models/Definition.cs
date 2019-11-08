using SQLite;

namespace PocketDict.Models
{
    [Table("Definitions")]
    public class Definition
    {
        public int definitionId { get; set; }
        public string definition { get; set; }
        [PrimaryKey]
        public int wordId { get; set; }
    }
}