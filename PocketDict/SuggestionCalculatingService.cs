using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PocketDict
{
    public class SuggestionCalculatingService
    {
        private List<string> wordListLocal;
        public SuggestionCalculatingService(AssetManager assets)
        {
            wordListLocal = new List<string>();

            using (StreamReader reader = new StreamReader(assets.Open("words.txt")))
            {
                while (!reader.EndOfStream)
                {
                    wordListLocal.Add(reader.ReadLine());
                }
            }
        }

        public void GetEstimate(TextView view, string input)
        {
            List<double> list_scores;
            List<string> list_now = new List<string>(wordListLocal);

            if (input.Length > 0)
            {
                list_scores = new List<double>();
                int index;
                string val1;
                double max;
                view.Text = "";

                for (int i = 0; i < wordListLocal.Count; i++)
                {

                    list_scores.Add(GetScore(input, list_now[i]));

                }

                for (int i = 0; i < 10; i++)
                {
                    max = list_scores.Max();
                    index = list_scores.IndexOf(max);
                    val1 = list_now[index];
                    list_scores.RemoveAt(index);
                    list_now.RemoveAt(index);
                    view.Text += $"{val1} {max.ToString()} | ";

                }
                view.Invalidate();
            }
        }

        public double GetScore(string keyword, string testword)
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

    }
}