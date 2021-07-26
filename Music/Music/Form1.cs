using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using MattUtil;

namespace music
{
    public partial class Form1 : Form
    {
        private MTRandom Random = new MTRandom();

        private Dialog d = new Dialog();

        private string dir;
        private int numSize = 5;

        private int min, max;

        SortedDictionary<int?, SongFile> files;
        private IDictionary<string, Song> songs;

        public Form1()
        {
            Random.StartTick();

            InitializeComponent();
            this.MouseWheel += Form1_MouseWheel;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.ReadOnly = false;
            dataGridView2.ReadOnly = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.textBox1.Focus();
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = SystemColors.Window;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            if (Directory.Exists(path))
            {
                dir = path.Trim('/', '\\') + Path.DirectorySeparatorChar;
                files = new SortedDictionary<int?, SongFile>();
                songs = new SortedDictionary<string, Song>();

                string[] paths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                Regex r = new Regex("\\d+_ .*");
                foreach (string p in paths)
                {
                    SongFile sf = new SongFile();
                    sf.path = p;

                    string name = Path.GetFileNameWithoutExtension(p);
                    if (r.IsMatch(name))
                    {
                        string[] split = GetParts(sf, out sf.num);
                        name = split[1];
                        numSize = Math.Max(numSize, split[0].Length);
                    }

                    files.Add(sf.num, sf);

                    Song s;
                    songs.TryGetValue(name, out s);
                    if (s == null)
                    {
                        s = new Song();
                        s.form = this;
                        songs.Add(name, s);
                        s.name = name;
                    }

                    sf.song = s;

                    s.copies++;
                    s.files.Add(sf);
                }

                min = files.Keys.Min().Value;
                max = files.Keys.Max().Value;

                Refresh1();
                dataGridView1.Columns[0].ReadOnly = false;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;

                Refresh2();
            }
            else
            {
                textBox1.BackColor = Color.IndianRed;
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int num = (int)dataGridView2.Rows[e.RowIndex].Cells[0].Value;
            d.textBox1.Text = num.ToString();
            if (d.ShowDialog() == DialogResult.OK)
            {
                int newVal;
                if (int.TryParse(d.textBox1.Text, out newVal))
                {
                    if (num != newVal)
                    {
                        foreach (Song s in songs.Values.Where(s => s.copies == num))
                            s.copies = newVal;
                        Refresh2();
                    }
                }
                else
                {
                    d.textBox1.BackColor = Color.IndianRed;
                    dataGridView2_CellClick(sender, e);
                }
            }
            d.textBox1.BackColor = SystemColors.Window;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = songs.Values.Sum(s => Math.Abs(s.files.Count - s.copies));
            this.progressBar1.Visible = true;
            this.progressBar1.Refresh();

            foreach (Song s in Random.Iterate(songs.Values))
            {
                int cur = s.files.Count;
                int tar = s.copies;

                while (cur > tar)
                {
                    SongFile sf = Random.SelectValue(s.files);
                    File.Delete(sf.path);

                    s.files.Remove(sf);
                    files.Remove(sf.num);
                    cur--;

                    this.progressBar1.Value++;
                    this.progressBar1.Refresh();
                }

                while (cur < tar)
                {
                    int val = Random.RangeInt(Math.Max(0, min - 1), max + 1);
                    if (files.ContainsKey(val))
                    {
                        SongFile move = files[val];

                        ++max;
                        string newPath = NewPath(move, max);
                        File.Move(move.path, newPath);
                        move.num = max;
                        move.path = newPath;

                        files.Remove(val);
                        files.Add(max, move);
                    }

                    SongFile sf = Random.SelectValue(s.files);
                    string np = NewPath(sf, val);
                    File.Copy(sf.path, np);

                    SongFile copy = new SongFile();
                    copy.path = np;
                    copy.num = val;
                    copy.song = s;

                    s.files.Add(copy);
                    files.Add(val, copy);
                    cur++;

                    min = Math.Min(min, val);
                    max = Math.Max(max, val);

                    this.progressBar1.Value++;
                    this.progressBar1.Refresh();
                }
            }

            button1_Click(sender, e);

            this.progressBar1.Visible = false;
        }

        private static string[] GetParts(SongFile sf, out int? num)
        {
            string[] parts = Path.GetFileNameWithoutExtension(sf.path).Split(new String[] { "_ " }, StringSplitOptions.None);
            num = int.Parse(parts[0]);
            return parts;
        }
        private string NewPath(SongFile sf, int newNum)
        {
            return this.dir + FormatNum(newNum) + "_ " + sf.song.name + Path.GetExtension(sf.path);
        }
        private string FormatNum(int num)
        {
            return num.ToString(new string('0', this.numSize));
        }

        private void Refresh1()
        {
            dataGridView1.DataSource = Random.Iterate(songs.Values).OrderByDescending(s => s.copies).ToList();
        }
        private void Refresh2()
        {
            dataGridView2.DataSource = songs.Values.GroupBy(s => s.Copies).Select(g => new Tuple<int, int, int>(g.Key, g.Count(), g.Key * g.Count())).OrderBy(t => t.Item1).ToList();
            dataGridView2.Columns[0].HeaderText = "Group";
            dataGridView2.Columns[1].HeaderText = "Songs";
            dataGridView2.Columns[2].HeaderText = "Total";
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            int currentIndex = this.dataGridView1.FirstDisplayedScrollingRowIndex;
            int scrollLines = SystemInformation.MouseWheelScrollLines;
            if (e.Delta > 0)
            {
                this.dataGridView1.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollLines);
            }
            else if (e.Delta < 0)
            {
                this.dataGridView1.FirstDisplayedScrollingRowIndex = currentIndex + scrollLines;
            }
        }

        public class SongFile
        {
            public string path = null;
            public int? num = null;
            public Song song = null;

            public override string ToString()
            {
                return string.Format("{0} {1}", num, path);
            }
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Refresh1();
        }

        public class Song
        {
            public Form1 form;

            public string name = null;
            public int copies = 0;
            public List<SongFile> files = new List<SongFile>();

            public int Copies
            {
                get
                {
                    return copies;
                }
                set
                {
                    copies = value;
                    form.Refresh2();
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string Numbers
            {
                get
                {
                    return files.Select(sf => sf.num).OrderBy(n => n).Select(n => form.FormatNum(n.Value)).Aggregate((s1, s2) => s1 + " " + s2);
                }
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", copies, name);
            }
        }
    }
}
