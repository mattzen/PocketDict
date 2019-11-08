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

        public CombinedSearchModelPolish GetCombinedSearchModelPolish(string keyword)
        {
            var definitions = new List<Definition>();
            var examples = new List<Example>();
            try
            {
                var word = GetPolishWords(keyword);
                if (word != null)
                {
                    var words = GetEnglishTransaltionFromPolish(word);

                    return new CombinedSearchModelPolish
                    {
                        words = words,
                        polishWord = word[0]
                    };
                }
            }
            catch
            {
                //ignore for now
            }

            return null;
        }

        public CombinedSearchModelEnglish GetCombinedSearchModelEnglish(string keyword)
        {
            var definitions = new List<Definition>();
            var examples = new List<Example>();
            var polishWords = new List<PolishWord>();
            try
            {
                var word = db.Get<Word>(keyword);

                if (word != null && word.word != null)
                {
                    definitions = GetEnglishDefinitionsFromEnglishWordId(word.wordId);
                    polishWords = GetPolishTransaltionBasedONWordId(word.wordId);
                    examples = GetExamples(definitions);

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
                //ignore for now
            }
            return null;
        }


        private List<PolishWord> GetPolishWords(string polishWord)
        {
            return db.Query<PolishWord>($"SELECT * FROM PolishWords WHERE word == ?", polishWord);
        }

        private List<Word> GetEnglishTransaltionFromPolish(List<PolishWord> polishWord)
        {
            return db.Query<Word>($"select * from Words where wordId in ({string.Join(",", polishWord.Select(x => x.wordId.ToString()))})");
        }

        private List<Example> GetExamples(List<Definition> definitions)
        {
            return db.Query<Example>($"select * from Examples where definitionId in ({string.Join(",", definitions.Select(x => x.definitionId.ToString()))})");
        }

        private List<PolishWord> GetPolishTransaltionBasedONWordId(int id)
        {
            return db.Query<PolishWord>($"select * from PolishWords where wordId = ?", id);
        }

        private List<Definition> GetEnglishDefinitionsFromEnglishWordId(int id)
        {
            return db.Query<Definition>($"select * from Definitions where wordId = ?", id);
        }
    }
}