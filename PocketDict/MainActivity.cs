using System;
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
        TextView textMessage;
        EditText searchBox;
        TextView definitionsView;
        TextView wordView;
        TextView polishView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            searchBox = FindViewById<EditText>(Resource.Id.searchBox);
            definitionsView = FindViewById<TextView>(Resource.Id.definitionsView);
            wordView = FindViewById<TextView>(Resource.Id.wordView);
            polishView = FindViewById<TextView>(Resource.Id.polishView);

            definitionsView.MovementMethod = new ScrollingMovementMethod();

            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;

            string dbPath = FileAccessHelper.GetLocalFilePath("wordsdbandroid.db");
            var db = new SQLiteConnection(dbPath);

            //definitionsView.MovementMethod.Initialize(new ScrollingMovementMethod());

            searchBox.TextChanged += delegate
            {
                try
                {
                    definitionsView.Text = string.Empty ;
                    wordView.Text = string.Empty;
                    polishView.Text = string.Empty;

                    var stock = db.Get<Word>(searchBox.Text);
                    var Definition = db.Get<Definitions>(stock.wordId);
                    var Definitions = db.Query<Definitions>($"select * from Definitions where wordId = ?", stock.wordId);
                    var PolishWords = db.Query<PolishWord>($"select * from PolishWords where wordId = ?", stock.wordId);
                    var examples = db.Query<Examples>($"select * from Examples where definitionId in ({string.Join(",", Definitions.Select(x => x.definitionId.ToString()))})");

                    wordView.Append($"{stock.word} [{stock.type}]");

                    for (int i = 0; i < Definitions.Count; i++)
                    {
                        definitionsView.Append($"{i+1}. {Definitions[i].definition}");
                        var example = examples.FirstOrDefault(x => x.definitionId == Definitions[i].definitionId);
                        if (example != null)
                        {
                            definitionsView.Append(System.Environment.NewLine);
                            definitionsView.Append($"  -{example.example}");
                        }
                        definitionsView.Append(System.Environment.NewLine);

                    }
                    polishView.Append("Polish: ");
                    polishView.Append(System.Environment.NewLine);

                    StringBuilder str = new StringBuilder();
                    foreach (var polword in PolishWords)
                    {
                        str.Append($"{polword.word} [{polword.type}] ");
                    }

                    polishView.Append(str.ToString());

                    definitionsView.Invalidate();
                    wordView.Invalidate();
                    polishView.Invalidate();
                }
                catch
                {

                }
            };


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

