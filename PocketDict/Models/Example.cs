using System;
using System.Collections.Generic;
using SQLite;

namespace PocketDict.Models
{
    [Table("Examples")]
    public class Example
    {
        public int exampleId { get; set; }
        [PrimaryKey]
        public int definitionId { get; set; }
        public string example { get; set; }
    }
}