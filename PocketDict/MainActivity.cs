using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using SQLite;

namespace PocketDict
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private EditText searchBox;
        private TextView definitionsView;
        private TextView wordView;
        private TextView polishView;
        private TextView suggestionsView;
        private SQLiteConnection db;
        private List<string> wordListLocal;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            InitializeDb();
            wordListLocal = new List<string>();
            AssetManager assets = this.Assets;

            using (StreamReader reader = new StreamReader(assets.Open("words.txt")))
            {
                while (!reader.EndOfStream)
                {
                    wordListLocal.Add(reader.ReadLine());
                }
            }

            searchBox = FindViewById<EditText>(Resource.Id.searchBox);
            definitionsView = FindViewById<TextView>(Resource.Id.definitionsView);
            wordView = FindViewById<TextView>(Resource.Id.wordView);
            polishView = FindViewById<TextView>(Resource.Id.polishView);
            suggestionsView = FindViewById<TextView>(Resource.Id.suggestionsView);

            definitionsView.MovementMethod = new ScrollingMovementMethod();

            searchBox.TextChanged += SearchBox_TextChanged;

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        private void InitializeDb()
        {
            string dbPath = FileAccessHelper.GetLocalFilePath("wordsdbandroid.db");
            db = new SQLiteConnection(dbPath);
        }

        private void SearchBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (searchBox.Text.Length > 2)
                {
                    GetEstimate(suggestionsView, searchBox.Text, wordListLocal);

             
                    var word = db.Get<Word>(searchBox.Text);
                    var Definition = db.Get<Definitions>(word.wordId);
                    var definitions = GetDefinitionsAsync(db, word.wordId);
                    var polishWords = GetPolishWordsAsync(db, word.wordId);
                    var examples = GetExamplesAsync(db, definitions);
                    
                    wordView.Text = string.Empty;

                    wordView.Append($"{word.word} [{word.type}]");
                    
                    definitionsView.Text = string.Empty;
                    polishView.Text = string.Empty;

                    SetDefitnitionsView(definitionsView, definitions, examples);
                    SetPolishView(polishView, polishWords);

                    definitionsView.Invalidate();
                    wordView.Invalidate();
                    polishView.Invalidate();
                }
            }
            catch
            {

            }
        }


        public void GetEstimate(TextView google_textbox, string input, List<string> list)
        {
            string word;
            List<double> list_scores;
            List<string> list_now = new List<string>(list);
            word = input;

            if (word.Length > 0)
            {

                list_scores = new List<double>();
                int index;
                string val1;
                double max;
                google_textbox.Text = "";

                for (int i = 0; i < list.Count; i++)
                {

                    list_scores.Add(getScore(word, list_now[i]));

                }


                for (int i = 0; i < 10; i++)
                {
                    max = list_scores.Max();
                    index = list_scores.IndexOf(max);
                    val1 = list_now[index];
                    list_scores.RemoveAt(index);
                    list_now.RemoveAt(index);
                    google_textbox.Text += val1 + " " + max.ToString() + " | ";
                }
                google_textbox.Invalidate();
            }
        }


        public double getScore(string keyword, string testword)
        {
            int score = 0;
            int count = 0;

            for (int i = 0; i < keyword.Length; i++)
            {
                for (int j = count; j < testword.Length; j++)
                {
                    if (keyword[i] == testword[j])
                    {
                        score = score + 2;
                        count = j;
                        break;
                    }

                }

            }

            if (keyword.Length == testword.Length)
                score += 1;

            if (keyword[0] == testword[0] && keyword[keyword.Length - 1] == testword[testword.Length - 1])
                score += 1;
            else
                score -= 1;

            if (testword.Length > keyword.Length)
                score -= (testword.Length - keyword.Length) / 2;

            if (testword.Length < keyword.Length)
                score -= (keyword.Length - testword.Length) / 2;

            return score;
        }
        public List<Examples> GetExamplesAsync(SQLiteConnection db, List<Definitions> definitions)
        {
            return db.Query<Examples>($"select * from Examples where definitionId in ({string.Join(",", definitions.Select(x => x.definitionId.ToString()))})");
        }

        public List<PolishWord> GetPolishWordsAsync(SQLiteConnection db, int id)
        {
            return db.Query<PolishWord>($"select * from PolishWords where wordId = ?", id);
        }

        public List<Definitions> GetDefinitionsAsync(SQLiteConnection db, int id)
        {
            return db.Query<Definitions>($"select * from Definitions where wordId = ?", id);
        }

        public void SetDefitnitionsView(TextView definitionsView, List<Definitions> definitions, List<Examples> examples)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                definitionsView.Append($"{i + 1}. {definitions[i].definition}");
                var example = examples.FirstOrDefault(x => x.definitionId == definitions[i].definitionId);
                if (example != null)
                {
                    definitionsView.Append(System.Environment.NewLine);
                    definitionsView.Append($"  -{example.example}");
                }
                definitionsView.Append(System.Environment.NewLine);

            }
        }

        public void SetPolishView(TextView polishView, List<PolishWord> polishWords)
        {
            polishView.Append("Polish: ");
            polishView.Append(System.Environment.NewLine);

            StringBuilder str = new StringBuilder();
            foreach (var polword in polishWords)
            {
                str.Append($"{polword.word} [{polword.type}] ");
            }
            polishView.Append(str.ToString());
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public static class FileAccessHelper
        {
            public static string GetLocalFilePath(string filename)
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string dbPath = Path.Combine(path, filename);

                CopyDatabaseIfNotExists(dbPath);

                return dbPath;
            }

            private static void CopyDatabaseIfNotExists(string dbPath)
            {
                if (!File.Exists(dbPath))
                {
                    using (var br = new BinaryReader(Application.Context.Assets.Open("wordsdbandroid.db")))
                    {
                        using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bw.Write(buffer, 0, length);
                            }
                        }
                    }
                }
            }
        }


    }
}

