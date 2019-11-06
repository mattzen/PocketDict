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
    [Table("Words")]
    public class Word
    {
        public int wordId { get; set; }
        [PrimaryKey]
        public string word { get; set; }
        public string type { get; set; }
    }
}