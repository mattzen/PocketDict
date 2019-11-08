using SQLite;

namespace PocketDict.Models
{
    [Table("Words")]
    public class Word
    {
        public int wordId { get; set; }
        [PrimaryKey]
        public string word { get; set; }
        public string type { get; set; }
    }
}