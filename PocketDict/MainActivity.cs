using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
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
        private SQLiteConnection db;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            InitializeDb();

            searchBox = FindViewById<EditText>(Resource.Id.searchBox);
            definitionsView = FindViewById<TextView>(Resource.Id.definitionsView);
            wordView = FindViewById<TextView>(Resource.Id.wordView);
            polishView = FindViewById<TextView>(Resource.Id.polishView);

            definitionsView.MovementMethod = new ScrollingMovementMethod();

            searchBox.TextChanged += SearchBox_TextChanged;

            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
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
                    definitionsView.Text = string.Empty;
                    wordView.Text = string.Empty;
                    polishView.Text = string.Empty;
                    var word = db.Get<Word>(searchBox.Text);
                    var Definition = db.Get<Definitions>(word.wordId);
                    var definitions = GetDefinitionsAsync(db, word.wordId);
                    var polishWords = GetPolishWordsAsync(db, word.wordId);
                    var examples = GetExamplesAsync(db, definitions);

                    wordView.Append($"{word.word} [{word.type}]");

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


        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.menu_main, menu);
        //    return true;
        //}

        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
        //    int id = item.ItemId;
        //    if (id == Resource.Id.action_settings)
        //    {
        //        return true;
        //    }

        //    return base.OnOptionsItemSelected(item);
        //}

        //private void FabOnClick(object sender, EventArgs eventArgs)
        //{
        //    View view = (View) sender;
        //    Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
        //        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        //}
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}


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

