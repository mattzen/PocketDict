using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using PocketDict.Daos;
using PocketDict.Models;
using PocketDict.Services;
using SQLite;
using Xamarin.Essentials;

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
        private Button speakButton;
        private string currentWord = string.Empty;
        private LinearLayout polishTranslationLayout;

        private TextView tx;
        private TextView tx1;
        private TextView tx2;



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
            speakButton = FindViewById<Button>(Resource.Id.speakButton);
            polishTranslationLayout = FindViewById<LinearLayout>(Resource.Id.polishTranslationLayout);

       
            speakButton.Visibility = ViewStates.Invisible;
            definitionsView.MovementMethod = new ScrollingMovementMethod();

            searchBox.TextChanged += SearchBox_TextChanged;

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            speakButton.Touch += SpeakButton_Touch;
        }

        private void SpeakButton_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                TextToSpeech.SpeakAsync(currentWord);
            }
        }

        private void SearchBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (searchBox.Text.Length > 2)
                {
                    suggestionService.GetEstimate(suggestionsView, searchBox.Text);

                    var polishTask = Task.Run(() => dao.GetCombinedSearchModelPolish(searchBox.Text));
                    var englishTask = Task.Run(() => dao.GetCombinedSearchModelEnglish(searchBox.Text));

                    Task.WaitAll(polishTask, englishTask);

                    if (polishTask?.Result != null && polishTask.Result.words.Count > 0)
                    {
                        speakButton.Visibility = ViewStates.Invisible;
                        wordView.Text = string.Empty;
                        wordView.Append($"{polishTask.Result.polishWord?.word} [{polishTask.Result.polishWord?.type}]");
                        definitionsView.Text = string.Empty;
                        polishView.Text = string.Empty;
                        SetEnglishTransaltionView(polishView, polishTask.Result.words, polishTask.Result.polishWord.word);
                    }
                    if (englishTask?.Result != null && englishTask.Result.polishWords.Count > 0)
                    { 
                        wordView.Text = string.Empty;
                        wordView.Append($"{englishTask.Result.word?.word} [{englishTask.Result.word?.type}]");

                        if (englishTask.Result.word?.word != null)
                        {
                            currentWord = englishTask.Result.word.word;
                            speakButton.Visibility = ViewStates.Visible;
                        }

                        definitionsView.Text = string.Empty;
                        polishView.Text = string.Empty;
                        SetDefitnitionsView(definitionsView, englishTask.Result.definitions, englishTask.Result.examples);
                        SetPolishTransaltionsView(polishView, englishTask.Result.polishWords);
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


        public void SetDefitnitionsView(TextView definitionsView, List<Definition> definitions, List<Example> examples)
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

        public void SetPolishTransaltionsView(TextView polishView, List<PolishWord> polishWords)
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



        public void SetEnglishTransaltionView(TextView polishView, List<Word> polishWords, string keyword)
        {
            UnsubscribeAll();

           // polishView.Append("English: ");
            //polishView.Append(System.Environment.NewLine);
            //StringBuilder str = new StringBuilder();
            //foreach (var polword in polishWords)
            //{
            //    str.Append($"{polword.word} [{polword.type}] ");
            //}
            //polishView.Append(str.ToString());

            var label = new TextView(this);
            label.Text = $"English translation of '{ keyword }': ";
            polishTranslationLayout.AddView(label);

            tx = new TextView(this);
            tx.SetTextColor(Color.DarkRed);
            tx.SetTextSize(ComplexUnitType.Pt, 12);
            tx.SetTypeface(null, TypefaceStyle.Bold);
            tx.Text = polishWords[0].word;
            tx.Touch += Tx_Touch;
            polishTranslationLayout.AddView(tx);

            if (polishWords.Count > 1)
            {
                tx1 = new TextView(this);
                tx1.SetTextColor(Color.DarkRed);
                tx1.Text = polishWords[1].word;
                tx1.SetTextSize(ComplexUnitType.Pt, 12);
                tx1.SetTypeface(null, TypefaceStyle.Bold);
                tx1.Touch += Tx1_Touch;
                polishTranslationLayout.AddView(tx1);
            }

            if (polishWords.Count > 2)
            {
                tx2 = new TextView(this);
                tx2.SetTextColor(Color.DarkRed);
                tx2.Text = polishWords[2].word;
                tx2.SetTextSize(ComplexUnitType.Pt, 12);
                tx2.SetTypeface(null, TypefaceStyle.Bold);
                tx2.Touch += Tx2_Touch;
                polishTranslationLayout.AddView(tx2);
            }

            polishTranslationLayout.Invalidate();

        }

        private void UnsubscribeAll()
        {
            if (tx != null)
            {
                tx.Touch -= Tx_Touch;
                tx = null;
            }

            if (tx1 != null)
            {
                tx1.Touch -= Tx1_Touch;
                tx1 = null;
            }

            if (tx2 != null)
            {
                tx2.Touch -= Tx2_Touch;
                tx2 = null;
            }
            polishTranslationLayout.RemoveAllViews();
        }


        private void Tx2_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                searchBox.Text = ((TextView)sender).Text;
                UnsubscribeAll();
            }
        }

        private void Tx_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                searchBox.Text = ((TextView)sender).Text;
                UnsubscribeAll();
            }
        }

        private void Tx1_Touch(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                searchBox.Text = ((TextView)sender).Text;
                UnsubscribeAll();
            }
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

