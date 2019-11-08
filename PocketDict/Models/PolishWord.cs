using System;
using SQLite;

namespace PocketDict.Models
{
    [Table("PolishWords")]
    public class PolishWord
    {
        public int wordId { get; set; }
        public int polwordId { get; set; }
        [PrimaryKey]
        public string word { get; set; }
        public string type { get; set; }  
    }
}