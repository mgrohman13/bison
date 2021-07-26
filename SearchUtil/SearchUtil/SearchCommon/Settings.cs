using System;
using System.Drawing;
using System.IO;

namespace SearchCommon
{
    /// <summary>
    /// Stores user preferences.
    /// </summary>
    public class Settings
    {
        private int resultsPerFile = 3;
        public int ResultsPerFile
        {
            get { return resultsPerFile; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("ResultsPerFile", value, "ResultsPerFile must be greater than 0.");
                else if (value > 999)
                    throw new ArgumentOutOfRangeException("ResultsPerFile", value, "ResultsPerFile must be less than or equal to 999.");

                resultsPerFile = value;
            }
        }

        private int maxHistItems = 30;
        public int MaxHistItems
        {
            get { return maxHistItems; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("MaxHistItems", value, "MaxHistItems must be greater than or equal to 0.");
                else if (value > 999)
                    throw new ArgumentOutOfRangeException("MaxHistItems", value, "MaxHistItems must be less than or equal to 999.");

                maxHistItems = value;
            }
        }

        private int dropDownItems = 13;
        public int DropDownItems
        {
            get { return dropDownItems; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("DropDownItems", value, "DropDownItems must be greater than 0.");
                else if (value > 100)
                    throw new ArgumentOutOfRangeException("DropDownItems", value, "DropDownItems must be less than or equal to 100.");

                dropDownItems = value;
            }
        }

        TimeSpan span = new TimeSpan(7, 0, 0, 0);
        public TimeSpan Span
        {
            get { return span; }
            set
            {
                if (value.TotalDays < 0)
                    throw new ArgumentOutOfRangeException("Span", value, "Span must be greater than 0 days.");
                else if (value.TotalDays > 1000)
                    throw new ArgumentOutOfRangeException("Span", value, "Span must be less than or equal to 999.9 days.");
                else if (value.TotalDays > 999.9)
                    value = new TimeSpan(0, 0, 1439856, 0);

                span = value;
            }
        }

		Color highlightColor = Color.Blue;
        public Color HighlightColor
        {
            get { return highlightColor; }
            set { highlightColor = value; }
        }

        Color textColor = Color.Black;
        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        Color backColor = Color.White;
        public Color BackColor
        {
            get { return backColor; }
            set { backColor = value; }
        }

        private bool notepadDoubleclick = true;
        public bool NotepadDoubleclick
        {
            get { return notepadDoubleclick; }
            set { notepadDoubleclick = value; }
        }

        private bool notepadRightClick = true;
        public bool NotepadRightClick
        {
            get { return notepadRightClick; }
            set { notepadRightClick = value; }
        }

        private bool autoScrollResults = true;
        public bool AutoScrollResults
        {
            get { return autoScrollResults; }
            set { autoScrollResults = value; }
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(ResultsPerFile);
            bw.Write(DropDownItems);
            bw.Write(MaxHistItems);

            bw.Write(NotepadDoubleclick);
            bw.Write(NotepadRightClick);
            bw.Write(AutoScrollResults);

            SaveColor(bw, HighlightColor);
            SaveColor(bw, TextColor);
            SaveColor(bw, BackColor);

            bw.Write(Span.Ticks);
        }

        public static Settings Load(BinaryReader br)
        {
            Settings result = new Settings();

            try
            {
                result.ResultsPerFile = br.ReadInt32();
                result.DropDownItems = br.ReadInt32();
                result.MaxHistItems = br.ReadInt32();

                result.NotepadDoubleclick = br.ReadBoolean();
                result.NotepadRightClick = br.ReadBoolean();
                result.AutoScrollResults = br.ReadBoolean();

                result.HighlightColor = LoadColor(br);
                result.TextColor = LoadColor(br);
                result.BackColor = LoadColor(br);

                result.Span = new TimeSpan(br.ReadInt64());
            }
            catch { }

            return result;
        }

        private void SaveColor(BinaryWriter bw, Color color)
        {
            bw.Write(color.R);
            bw.Write(color.G);
            bw.Write(color.B);
        }

        private static Color LoadColor(BinaryReader br)
        {
            return Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
        }
    }
}