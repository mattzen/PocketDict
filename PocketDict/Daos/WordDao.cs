using System.Collections.Generic;
using System.Linq;
using PocketDict.Helpers;
using PocketDict.Models;
using SQLite;

namespace PocketDict.Daos
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
            List<Definition> definitions = new List<Definition>();
            List<Example> examples = new List<Example>();
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
            List<Definition> definitions = new List<Definition>();
            List<Example> examples = new List<Example>();
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

        private List<Example> GetExamplesAsync(List<Definition> definitions)
        {
            return db.Query<Example>($"select * from Examples where definitionId in ({string.Join(",", definitions.Select(x => x.definitionId.ToString()))})");
        }

        private List<PolishWord> GetPolishWordsAsync(int id)
        {
            return db.Query<PolishWord>($"select * from PolishWords where wordId = ?", id);
        }

        private List<Definition> GetDefinitionsAsync(int id)
        {
            return db.Query<Definition>($"select * from Definitions where wordId = ?", id);
        }
    }
}