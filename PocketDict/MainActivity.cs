using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        SuggestionCalculatingService suggestionService;
        WordDao dao;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            suggestionService = new SuggestionCalculatingService(this.Assets);
            dao = new WordDao();

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

        private void SearchBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (searchBox.Text.Length > 2)
                {
                    suggestionService.GetEstimate(suggestionsView, searchBox.Text);

                    var taskOne = Task.Run(() => dao.GetCombinedSearchModel(searchBox.Text));
                    var taskTwo = Task.Run(() => dao.GetCombinedSearchModelEnglish(searchBox.Text));

                    taskOne.Wait();
                    taskTwo.Wait();

                    if (taskOne?.Result != null && taskOne.Result.words.Count > 0)
                    {
                        wordView.Text = string.Empty;
                        wordView.Append($"{taskOne.Result.polishWord?.word} [{taskOne.Result.polishWord?.type}]");
                        definitionsView.Text = string.Empty;
                        polishView.Text = string.Empty;
                       //SetDefitnitionsView(definitionsView, taskOne.Result.definitions, taskOne.Result.examples);
                        SetPolishView(polishView, taskOne.Result.words);
                    }
                    else if (taskTwo?.Result != null && taskTwo.Result.polishWords.Count > 0)
                    {
                        wordView.Text = string.Empty;
                        wordView.Append($"{taskTwo.Result.word?.word} [{taskTwo.Result.word?.type}]");
                        definitionsView.Text = string.Empty;
                        polishView.Text = string.Empty;
                        SetDefitnitionsView(definitionsView, taskTwo.Result.definitions, taskTwo.Result.examples);
                        SetPolishView(polishView, taskTwo.Result.polishWords);
                    }

                    definitionsView.Invalidate();
                    wordView.Invalidate();
                    polishView.Invalidate();
                }
            }
            catch(Exception ex)
            {
                var ee = ex;
            }
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

        public void SetPolishView(TextView polishView, List<Word> polishWords)
        {
            polishView.Append("English: ");
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

      

    }
}

