using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MattUtil.RealTimeGame
{
    class HighScores
    {
        public List<HighScoreEntry> highScores;
        public decimal total;
        public int games;

        HighScores(List<HighScoreEntry> highScores, decimal total, int games)
        {
            this.highScores = highScores;
            this.total = total;
            this.games = games;
        }

        HighScores()
        {
            this.highScores = new List<HighScoreEntry>();
            this.total = 0;
            this.games = 0;
        }

        public static void SaveScore(bool showScores, decimal newScore)
        {
            lock (typeof(HighScores))
            {
                HighScores scores = ReadScores();
                List<HighScoreEntry> highScores = scores.highScores;

                if (highScores.Count < 10 || newScore > highScores[9].score)
                {
                    //made a high score, show the form
                    NewHighScoreForm form = new NewHighScoreForm();
                    form.ShowDialog();
                    form.Dispose();
                    showScores = true;

                    //add high score
                    highScores.Add(new HighScoreEntry(form.ScoreName, newScore));
                    //sort by score
                    highScores.Sort(new Comparison<HighScoreEntry>(CompareScoreEntries));
                    if (highScores.Count == 11)
                    {
                        //add the last score to the total and bump it off the list
                        scores.total += highScores[10].score;
                        ++( scores.games );
                        highScores.RemoveAt(10);
                    }
                }
                else
                {
                    scores.total += newScore;
                    ++( scores.games );
                }

                //write to the file
                FileStream stream = new FileStream(GameForm.Game.ScoreFile, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(scores.total);
                writer.Write(scores.games);
                //hash for security so the user cant just change the scores by editing the file
                writer.Write(Hash(scores));
                foreach (HighScoreEntry entry in scores.highScores)
                {
                    writer.Write(ChangeString(true, entry.name));
                    writer.Write(entry.score);
                }

                writer.Flush();
                writer.Close();
                stream.Close();
                stream.Dispose();
            }

            if (showScores)
                ShowScores();
        }
        static HighScores ReadScores()
        {
            FileStream fs = new FileStream(GameForm.Game.ScoreFile, FileMode.OpenOrCreate);
            BinaryReader reader = new BinaryReader(fs);

            int hash;
            decimal total;
            int games;
            try
            {
                //read initial values
                total = reader.ReadDecimal();
                games = reader.ReadInt32();
                hash = reader.ReadInt32();
            }
            catch
            {
                reader.Close();
                fs.Close();
                fs.Dispose();
                return new HighScores();
            }

            List<HighScoreEntry> highScores = new List<HighScoreEntry>();
            try
            {
                //read each high score until the end of the stream
                for (int i = 0 ; i < 10 ; ++i)
                    highScores.Add(new HighScoreEntry(ChangeString(false, reader.ReadString()), reader.ReadDecimal()));
            }
            catch { }

            reader.Close();
            fs.Close();
            fs.Dispose();

            HighScores retVal = new HighScores(highScores, total, games);

            //check security hash to validate score file
            if (Hash(retVal) == hash)
                return retVal;
            else
                return new HighScores();
        }
        static int CompareScoreEntries(HighScoreEntry p1, HighScoreEntry p2)
        {
            decimal dif = p2.score - p1.score;
            return dif > 0 ? 1 : dif < 0 ? -1 : 0;
        }
        static int Hash(HighScores scores)
        {
            //create a string to hash
            string hashVal = scores.total.ToString();
            foreach (HighScoreEntry se in scores.highScores)
                hashVal += se.ToString();
            hashVal += scores.games.ToString();
            hashVal += scores.total.ToString();
            //hash the string with my custom hash function
            return MattUtil.Hashing.Hash(hashVal);
        }
        static string ChangeString(bool dir, string value)
        {
            //shift strings for high score names so they arent obvious if someone opens the score file with a text editor
            const int mult = 13;
            const int add = 666;
            string retval = "";
            foreach (char c in value)
                retval += Convert.ToChar(dir ? Convert.ToInt32(c) * mult + add : ( Convert.ToInt32(c) - add ) / mult);
            return retval;
        }

        public static void ShowScores()
        {
            ScoresForm form = new ScoresForm(ReadScores());
            form.ShowDialog();
            form.Dispose();
        }
        public static void ClearScores()
        {
            FileStream fs = new FileStream(GameForm.Game.ScoreFile, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);

            //write a bunch of random data to the file so its size doesnt change too much
            //hash check will cause it to be thrown out so you wont get a bunch of random score data
            Byte[] randBytes = new byte[Game.Random.GaussianCappedInt(210, .1, 130)];
            Game.Random.NextBytes(randBytes);
            writer.Write(randBytes);

            writer.Flush();
            writer.Close();
            fs.Close();
            fs.Dispose();

            ShowScores();
        }
    }
    struct HighScoreEntry
    {
        public string name;
        public decimal score;
        public HighScoreEntry(string name, decimal score)
        {
            this.name = name;
            this.score = score;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", name, score);
        }
    }
}
