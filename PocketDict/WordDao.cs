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
    public class WordDao
    {
        private SQLiteConnection db;

        public WordDao()
        {
            string dbPath = FileAccessHelper.GetLocalFilePath("wordsdbandroid.db");
            db = new SQLiteConnection(dbPath);
        }

        public CombinedSearchModel GetCombinedSearchModel(string keyword)
        {
            List<Definitions> definitions = new List<Definitions>();
            List<Examples> examples = new List<Examples>();
            try
            {
                var word = GetWordsFromPolishSearchAsync(keyword);
                if (word != null)
                {
                    var words = GetWordBasedOnPolishAsync(word);

                    if (words.Count > 0)
                    {
                        //definitions = GetDefinitionsAsync(db, words[0].wordId);
                        //examples = GetExamplesAsync(db, definitions);
                    }

                    return new CombinedSearchModel
                    {
                        //definitions = definitions,
                        //examples = examples,
                        words = words,
                        polishWord = word[0]
                    };

                }
            }
            catch
            {

            }

            return null;
        }

        public CombinedSearchModelEnglish GetCombinedSearchModelEnglish(string keyword)
        {
            List<Definitions> definitions = new List<Definitions>();
            List<Examples> examples = new List<Examples>();
            List<PolishWord> polishWords = new List<PolishWord>();
            try
            {
                var word = db.Get<Word>(keyword);

                if (word != null && word.word != null)
                {
                    definitions = GetDefinitionsAsync(word.wordId);
                    polishWords = GetPolishWordsAsync(word.wordId);
                    examples = GetExamplesAsync(definitions);

                    return new CombinedSearchModelEnglish
                    {
                        definitions = definitions,
                        examples = examples,
                        polishWords = polishWords,
                        word = word

                    };

                }
            }
            catch
            {

            }
            return null;
        }


        private List<PolishWord> GetWordsFromPolishSearchAsync(string polishWord)
        {
            return db.Query<PolishWord>($"SELECT * FROM PolishWords WHERE word == ?", polishWord);
        }

        private List<Word> GetWordBasedOnPolishAsync(List<PolishWord> polishWord)
        {
            return db.Query<Word>($"select * from Words where wordId in ({string.Join(",", polishWord.Select(x => x.wordId.ToString()))})");
        }

        private List<Examples> GetExamplesAsync(List<Definitions> definitions)
        {
            return db.Query<Examples>($"select * from Examples where definitionId in ({string.Join(",", definitions.Select(x => x.definitionId.ToString()))})");
        }

        private List<PolishWord> GetPolishWordsAsync(int id)
        {
            return db.Query<PolishWord>($"select * from PolishWords where wordId = ?", id);
        }

        private List<Definitions> GetDefinitionsAsync(int id)
        {
            return db.Query<Definitions>($"select * from Definitions where wordId = ?", id);
        }
    }
}