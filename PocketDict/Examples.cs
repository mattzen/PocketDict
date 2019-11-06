using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace PocketDict
{
    [Table("Examples")]
    public class Examples
    {
        public int exampleId { get; set; }
        [PrimaryKey]
        public int definitionId { get; set; }
        public string example { get; set; }
    }
}