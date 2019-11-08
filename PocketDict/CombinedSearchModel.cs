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

namespace PocketDict
{
    public class CombinedSearchModel
    {
        public List<Definitions> definitions { get; set; }
        public List<Examples> examples { get; set; }
        public PolishWord polishWord { get; set; }
        public List<Word> words { get; set; }

    }

    public class CombinedSearchModelEnglish
    {
        public List<Definitions> definitions { get; set; }
        public List<Examples> examples { get; set; }
        public List<PolishWord> polishWords { get; set; }
        public Word word { get; set; }
    }

}