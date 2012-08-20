/*
 * Matthew Grohman
 * Created with Microsoft Visual Studio 2005
 * SearchUtil - A pimped out search utility
 * searches for text strings in windows files
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SearchCommon;

namespace SearchUtil
{
	public partial class SearchForm : Form
	{

		#region constructors and fields

		public SearchForm()
		{
			InitializeComponent();

			//load combo boxes and settings
			LoadData();

			//refresh extension
			chxExt_CheckedChanged(null, null);
		}

		//user preferences
		Settings settings;

		//thread that does the actual searching
		Thread searchThread;

		//old strings for combo boxes and their associating dates
		Dictionary<string, DateTime> dirs = new Dictionary<string, DateTime>(),
			exts = new Dictionary<string, DateTime>(), searches = new Dictionary<string, DateTime>();

		//state variables
		string context = "";
		bool _searching = false;
		bool searching
		{
			get { return _searching; }
			set
			{
				_searching = value;
				//update the search button text
				if (_searching)
					SetText(this.btnSearch, "&Cancel");
				else
					SetText(this.btnSearch, "&Search");
			}
		}
		//rtf-formatting stuff to show results in a more friendly manner
		string resultText = "";
		string startResult;

		//used to keep list of files in memory for similar searches
		string lastSearchDir = "", lastSearchExt = "";
		bool lastSearchIsExt = true, lastSearchSubDir = true, lastSearchOnlyExts = true;
		string[] files = null;

		#endregion


		#region get file list methods

		private string[] GetFiles()
		{
			//max and value used to increment progress bar
			List<NumAvs> max = new List<NumAvs>();
			max.Add(new NumAvs());
			return GetFiles(context, max, 0, IsChecked(this.chxSubDir)).ToArray();
		}

		private List<string> GetFiles(string directory, List<NumAvs> max, int depth, bool subDirs)
		{
			//use a try-catch because there is no guarantee this will not put the progress bar's value above the maximum
			//as the maximum is just a caulculated value based on previous estimations
			try
			{
				//increment the progress bar
				IncProgress();
			}
			catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
			catch { }

			//resulting list of files
			List<string> result = new List<string>();

			bool error = false;
			try
			{
				//get files
				result.AddRange(Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly));
			}
			catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
			catch (Exception exc)
			{
				AddText("\\line *************\\line Could not obtain files in '" + directory
					+ "'\\line Error information:\\line " + exc.Message + "\\line *************\\line ");

				//only show one error per file
				error = true;
			}

			if (subDirs)
			{
				string[] dirs = new string[0];
				try
				{
					//get all subdirectories
					dirs = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
				}
				catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
				catch (Exception exc)
				{
					if (!error)
						AddText("\\line *************\\line Could not obtain subdirectories of '" + directory
							+ "'\\line Error information:\\line " + exc.Message + "\\line *************\\line ");
				}

				//add more depth when needed
				while (max.Count <= depth)
					max.Add(new NumAvs());

				//do stuff to approximate time needed, so we can properly set the progress bar
				max[depth].Add((double)dirs.Length);
				for (int val = max.Count - 2; val >= 0; val--)
					max[val].Multiple = max[val + 1].getAvg() + 1;

				//update the progress bar's 'Maximum' property
				ResetProgress(false, (int)Math.Ceiling(max[0].getAvg()), "");

				//get files and subdirectories of each subdirectory
				foreach (string dir in dirs)
					result.AddRange(GetFiles(dir, max, depth + 1, subDirs));
			}

			return result;
		}

		#endregion


		#region search methods

		private void AbortSearch()
		{
			//abort the search
			searchThread.Abort();

			searching = false;

			AddText("\\line *************\\line Search canceled.\\line *************\\line ");

			SetText(this.lblStatus, "Ready.");

			//makes it not crap out the first time you try to open a file in notepad
			FocusControl(this.rtbResults);
		}

		private void DoSearch()
		{
			//save the search criteria and add it to the comboboxes
			SaveOldTextData();

			//check for a valid search
			if (GetText(this.txtSearch).Trim() == "")
			{
				SetText("\\line *************\\line Search for something, dude!\\line *************\\line ");
				return;
			}

			//check if the directory exists
			if (Directory.Exists(GetText(this.txtDir)))
			{
				searching = true;

				searchThread = new Thread(SearchAll);
				//mark the thread so it doesn't keep the app alive when the main form thread exits
				searchThread.IsBackground = true;
				//give priority to most other threads
				searchThread.Priority = ThreadPriority.BelowNormal;
				//begin the search
				searchThread.Start();
			}
			else
				SetText("\\line *************\\line Invalid Directory.\\line *************\\line ");
		}

		private void SearchAll()
		{
			//get search text, context, and extensions
			string searchText = GetText(this.txtSearch);
			context = GetText(this.txtDir);
			string extensions = GetText(this.txtExt).Trim();

			bool checkExt = IsChecked(this.chxExt) && extensions != "";
			bool subDirs = IsChecked(this.chxSubDir);

			//fix case
			bool checkCase = IsChecked(this.chxCase);
			if (!checkCase)
				searchText = searchText.ToLower();

			//clear previous results
			SetText("");

			string[] extArray = new string[0];
			bool onlyExts = true;
			if (checkExt)
			{
				//check for special character pattern signifying the extensions should not be searched
				if ((extensions.Length > 2) && extensions[0] == '~' &&
					extensions[1] == '(' && extensions[extensions.Length - 1] == ')')
				{
					onlyExts = false;
					extensions = extensions.Substring(2, extensions.Length - 3);
				}

				extArray = extensions.Split(';');

				//trim each extension
				for (int i = 0; i < extArray.Length; i++)
					extArray[i] = extArray[i].Trim().Trim('.');
			}
			else
				onlyExts = false;

			ResetProgress(13, "Collecting files...");

			Stopwatch timer = new Stopwatch();
			bool checkTime = true;

			//check that the file list is not already in memory
			if (lastSearchDir != context || lastSearchIsExt != checkExt ||
				lastSearchSubDir != subDirs ||
				lastSearchExt != extensions || lastSearchOnlyExts != onlyExts)
			{
				timer.Start();

				files = null;

				//get the search option based on the checkbox
				SearchOption searchOption = SearchOption.TopDirectoryOnly;
				if (subDirs)
					searchOption = SearchOption.AllDirectories;

				try { files = Directory.GetFiles(context, "*", searchOption); }
				catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
				catch { files = GetFiles(); }

				if (checkExt)
					ResetProgress(files.Length, "Filtering extensions...");

				if (checkExt)
				{
					List<string> fileCollector = new List<string>();

					foreach (string newFile in files)
					{
						string[] parts = newFile.Split('.');

						bool add = !onlyExts;
						foreach (string validExt in extArray)
						{
							string[] validExtParts = validExt.Split('.');
							if (parts.Length - validExtParts.Length < 0)
								continue;

							//add the proper number of parts back together in case the 
							//extension has a . in it, such as Designer.cs
							string fileExt = "";
							for (int i = parts.Length - validExtParts.Length; i < parts.Length; i++)
								fileExt += parts[i] + ".";
							fileExt = fileExt.TrimEnd('.');

							//if the extension matches, either add the file or dont, as specified by the onlyExts option
							if (fileExt.ToUpper() == validExt.ToUpper())
							{
								add = onlyExts;
								break;
							}
						}
						if (add)
							fileCollector.Add(newFile);

						IncProgress();
					}

					files = fileCollector.ToArray();
				}

				//trim the context off of each file to save memory
				for (int i = 0; i < files.Length; i++)
					files[i] = files[i].Substring(context.Length);

				//update the lastSearch values 
				lastSearchDir = context;
				lastSearchIsExt = checkExt;
				lastSearchSubDir = subDirs;
				lastSearchExt = extensions;
				lastSearchOnlyExts = onlyExts;

				timer.Stop();
			}
			else
				checkTime = false;

			//setup progress bar
			ResetProgress(files.Length, "Searching files...");

			//count results
			int totResults = 0, totFiles = 0;

			//search each file
			foreach (string file in files)
			{
				//do the actual search
				int numResults = SearchFile(context + file, searchText, checkCase);
				if (numResults > 0)
					totFiles++;
				totResults += numResults;
				//increment progress bar
				IncProgress();
			}

			//compare the time taken to get the list of files to the size of the list
			//to determine if it's better to conserve memory
			if (checkTime && (timer.ElapsedMilliseconds * timer.ElapsedMilliseconds < files.Length * 5000))
			{
				lastSearchDir = "";
				files = null;
			}

			AddText(String.Format("\\line *************\\line Search finished successfully!  \\i "));

			//check if any results were found
			if (totFiles == 0)
				AddText("No results found...\\i0 \\line *************\\line ");
			else
				AddText(String.Format("{0} occurrence{2} found in {1} file{3}.\\i0 \\line *************\\line ",
					totResults, totFiles, totResults == 1 ? "" : "s", totFiles == 1 ? "" : "s"));

			SetText(this.lblStatus, "Ready.");

			searching = false;

			//makes it not crap out the first time you try to open a file in notepad
			FocusControl(this.rtbResults);
		}

		private int SearchFile(string file, string searchText, bool checkCase)
		{
			int numFound = 0;

			//get the usable name - the path relative to the search context
			int length = context.Length;
			if (context[length - 1] == '\\')
				length--;
			string usableName = file.Substring(length);

			StreamReader reader = null;

			try
			{
				//open reader
				reader = new StreamReader(file);
			}
			catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
			catch (Exception exc)
			{
				AddText("\\line *************\\line Unable to open reader for '" + usableName +
					"'\\line Error information:\\line " + exc.Message + "\\line *************\\line ");

				return 0;
			}

			////check if file contains string at all
			//string text = null;
			//text = reader.ReadToEnd();
			////fix case
			//if (!checkCase)
			//    text = text.ToLower();
			//if (!text.Contains(searchText))
			//    goto end;
			//reader.Close();

			try
			{
				//find specific lines
				string lineText = null;
				bool first = true, next = false;
				int count = 0, lineCount = 0;
				while (true)
				{
					lineCount++;
					string last = lineText;
					lineText = reader.ReadLine();
					//end of file
					if (lineText == null)
						break;
					string temp = lineText;
					//fix case
					if (!checkCase)
						temp = lineText.ToLower();

					//used to show the next line of code after a successful find
					if (next)
					{
						AddText(DoReplaces(lineText).Trim() + "\\line ");
						next = false;
					}
					//dont search the line immediately following a find
					else
					{
						//search for text
						int index = temp.IndexOf(searchText);
						if (index > -1)
						{
							//only show up to three finds in the same file
							if (++count > settings.ResultsPerFile)
								break;

							numFound++;

							//show the file name and relative location if it's the first find
							if (first)
								AddText("\\line \\b -------------" + DoReplaces(usableName) +
									"-------------\\b0  \\i line " + lineCount.ToString() + "\\i0 \\line ");
							else
								AddText("... \\i line " + lineCount.ToString() + "\\i0 \\line ");

							//show line before
							if (last != null)
								AddText(DoReplaces(last).Trim());
							AddText("\\line " + DoReplaces(lineText.Substring(0, index)).TrimStart() + "{\\cf1 " +
								DoReplaces(lineText.Substring(index, searchText.Length)) + "}" +
								DoReplaces(lineText.Substring(index + searchText.Length)).TrimEnd() + "\\line ");
							//tell it to show line after during the next loop
							next = true;

							first = false;
						}
					}
				}
				//show an empty line after if the text was found at the end of the file
				if (next)
					AddText("\\line ");

			}
			catch (ThreadAbortException) { Thread.CurrentThread.Abort(); }
			catch (Exception exc)
			{
				AddText("\\line *************\\line Unable to finish searching '" + usableName +
					"'\\line Error information:\\line " + exc.Message + "\\line *************\\line ");
			}

			reader.Close();
			reader.Dispose();

			return numFound;
		}

		private string DoReplaces(string input)
		{
			//add extra \'s to rtf-escape characters, alse get rid of the stupid \0's, whatever they are
			string res = input.Replace("\0", "").Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
			return res;
		}

		#endregion


		#region open notepad

		//used to open notepad
		[DllImport("kernel32.dll")]
		static extern uint WinExec(string cmdline, uint show);

		private void OpenWithNotepad(Point point)
		{
			/*
			 *  This shit would work fine if the stupid GetLineFromCharIndex() 
			 * method was consistant with the array from the Lines property...

			 //get the line number clicked on
			 int line = this.rtbResults.GetLineFromCharIndex(this.rtbResults.SelectionStart);

			 //start search with the clicked-on line and move up
			 for (int i = line; i > 0; i--)
			 {
				 string lText = this.rtbResults.Lines[i];
				 //search line text for '-------------', denoting a file name
				 if (lText.IndexOf("-------------") == 0)
				 {
					 //if found, get the full file name
					 string filename = context + "\\" + lText.Trim('-');

					 //if the file exists, open it, otherwise keep searching
					 if (File.Exists(filename))
					 {
						 try
						 {
							 WinExec("notepad \"" + filename + "\"", 1);
						 }
						 catch
						 {
							 MessageBox.Show("Unable to open file in notepad.\n'kernel32.dll' may not exist on your computer.",
								 "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						 }
						 return;
					 }
				 }
			 }
             
			 * So instead I have to do all this crap down here:
			 */

			//start search by character index of wherever you clicked
			for (int i = this.rtbResults.GetCharIndexFromPosition(point); i > 0; i--)
			{
				//set iterator to the first character of the line since we only have to check each line once
				i = this.rtbResults.GetFirstCharIndexFromLine(this.rtbResults.GetLineFromCharIndex(i));

				//check if this line starts with '-------------', denoting a file name
				bool all = true;
				for (int i2 = 0; i2 < 13; i2++)
					if (this.rtbResults.Text[i + i2] != '-')
					{
						all = false;
						break;
					}
				//if not a file name line, continue to the previous line
				if (!all)
					continue;

				//store the file name as you read it one by one - a real pain in the ass
				StringBuilder lText = new StringBuilder();

				int curIndex = i + 13;
				while (true)
				{
					//look for another string of '-------------', indicating the end of the file name
					all = true;
					for (int i2 = 0; i2 < 13; i2++)
						if (this.rtbResults.Text[curIndex + i2] != '-')
						{
							all = false;
							break;
						}
					//when it hits the end of the file name, try to open the file!
					if (all)
					{
						string filename = context + "\\" + lText;

						if (File.Exists(filename))
						{
							try
							{
								uint code = WinExec("notepad \"" + filename + "\"", 1);
								if (!(code > 31))
									throw new Exception("WinExec failed in kernel32.dll.  Return code: " + code);
							}
							catch (Exception exc)
							{
								MessageBox.Show("Unable to open file in notepad.  Error information: \n" + exc.Message,
									"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}

							//were done here
							return;
						}
						else
						{
							//try the next line
							break;
						}
					}

					try
					{
						//add this char to the text and move to the next char
						lText.Append(this.rtbResults.Text[curIndex++].ToString());
					}
					catch (Exception)
					{
						//try the next line
						break;
					}
				}
			}
		}

		#endregion


		#region thread safe control calls

		//callback delegates
		delegate void ModTextCallback(bool add, Control btnORrtb, string text);
		delegate string GetTextCallback(Control txtORrtb);
		delegate void ResetProgressCallback(bool setValue, int value, int maximum, string status);
		delegate void IncProgressCallback(int value);
		delegate void FocusCallback(Control c);
		delegate bool CheckedCallback(CheckBox chx);

		//get if a checkbox is checked
		private bool IsChecked(CheckBox chx)
		{
			if (chx.InvokeRequired)
			{
				CheckedCallback d = new CheckedCallback(IsChecked);
				return (bool)this.Invoke(d, new object[] { chx });
			}
			else
			{
				return chx.Checked;
			}
		}

		//focus a control
		private void FocusControl(Control c)
		{
			if (c.InvokeRequired)
			{
				FocusCallback d = new FocusCallback(FocusControl);
				this.Invoke(d, new object[] { c });
			}
			else
			{
				c.Focus();
			}
		}

		//reset the progress bar
		private void ResetProgress(int maximum, string status)
		{
			ResetProgress(true, 0, maximum, status);
		}
		private void ResetProgress(bool value, int maximum, string status)
		{
			ResetProgress(value, 0, maximum, status);
		}
		private void ResetProgress(bool setValue, int value, int maximum, string status)
		{
			if (this.pbSearch.InvokeRequired || this.lblStatus.InvokeRequired)
			{
				ResetProgressCallback d = new ResetProgressCallback(ResetProgress);
				this.Invoke(d, new object[] { setValue, value, maximum, status });
			}
			else
			{
				if (setValue)
				{
					this.pbSearch.Value = value;
					this.lblStatus.Text = status;
				}

				this.pbSearch.Maximum = maximum;
			}
		}

		//increment the progress bar
		private void IncProgress()
		{
			IncProgress(1);
		}
		private void IncProgress(int value)
		{
			if (this.pbSearch.InvokeRequired)
			{
				IncProgressCallback d = new IncProgressCallback(IncProgress);
				this.Invoke(d, new object[] { value });
			}
			else
			{
				this.pbSearch.Value += value;
			}
		}

		//modify the text of a button or rich text box
		private void SetText(string text)
		{
			SetText(this.rtbResults, text);
		}
		private void SetText(Control btnORrtb, string text)
		{
			ModText(false, btnORrtb, text);
		}
		private void AddText(string text)
		{
			AddText(this.rtbResults, text);
		}
		private void AddText(Control btnORrtb, string text)
		{
			ModText(true, btnORrtb, text);
		}
		private void ModText(bool add, Control btnORrtb, string text)
		{
			if (btnORrtb.InvokeRequired)
			{
				ModTextCallback d = new ModTextCallback(ModText);
				this.Invoke(d, new object[] { add, btnORrtb, text });
			}
			else
			{
				//check for proper casting and whether to set or add text
				if (btnORrtb is Button)
				{
					if (add)
						((Button)btnORrtb).Text += text;
					else
						((Button)btnORrtb).Text = text;
				}
				else if (btnORrtb is Label)
				{
					if (add)
						((Label)btnORrtb).Text += text;
					else
						((Label)btnORrtb).Text = text;
				}
				else if (btnORrtb is RichTextBox)
				{
					if (add)
						resultText += text;
					else
						resultText = text;

					RichTextBox rtb = ((RichTextBox)btnORrtb);
					if (!settings.AutoScrollResults && rtb == this.rtbResults && rtb.Focused)
						this.btnSearch.Focus();

					//int start = rtb.SelectionStart, length = rtb.SelectionLength;
					rtb.Rtf = startResult + resultText;
					//rtb.Select(start, length);
					//rtb.Focus();
					//this.rtbResults.Controls[0].Hide();
				}
				else
					throw new Exception();
			}
		}

		//get the text of a combo box or rich text box
		private string GetText(Control txtORrtb)
		{
			if (txtORrtb.InvokeRequired)
			{
				GetTextCallback d = new GetTextCallback(GetText);
				return (string)this.Invoke(d, new object[] { txtORrtb });
			}
			else
			{
				//check for proper casting
				if (txtORrtb is RichTextBox)
					return ((RichTextBox)txtORrtb).Text;
				else if (txtORrtb is ComboBox)
					return ((ComboBox)txtORrtb).Text;
				else
					throw new Exception();
			}
		}

		#endregion


		#region load, edit, and save preferences and form data

		private void Customize()
		{
			//event to fire when the apply button is clicked
			CustomizeForm.SettingsChangedEvent settingsChangedEvent = new CustomizeForm.SettingsChangedEvent(SettingsChanged);
			CustomizeForm.ShowCustomizeForm(settings, settingsChangedEvent);
		}

		private void SettingsChanged()
		{
			//get default settings if they are not defined
			if (settings == null)
				settings = new Settings();

			this.rtbResults.BackColor = settings.BackColor;

			ComboBox[] boxes = { this.txtDir, this.txtExt, this.txtSearch };
			//loop for each combo box and set number of drop down items
			for (int i = 0; i < boxes.Length; i++)
				boxes[i].MaxDropDownItems = settings.DropDownItems;

			//set proper text colors
			StringBuilder result = new StringBuilder("{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}{\\colortbl;\\red");
			result.Append(settings.HighlightColor.R);
			result.Append("\\green");
			result.Append(settings.HighlightColor.G);
			result.Append("\\blue");
			result.Append(settings.HighlightColor.B);
			result.Append(";\\red");
			result.Append(settings.TextColor.R);
			result.Append("\\green");
			result.Append(settings.TextColor.G);
			result.Append("\\blue");
			result.Append(settings.TextColor.B);
			result.Append(";}\r\n\\viewkind4\\uc1\\pard\\lang1033\\f0\\fs17\\cf2");
			startResult = result.ToString();

			//re-set the text to refresh the colors
			SetText(resultText);

			//OwnerSearch.SetSettings(settings);
		}

		private void GetAdvancedExtensionSettings()
		{
			AdvancedExts exts = new AdvancedExts(this.txtExt.Text);
			if (exts.ShowDialog() == DialogResult.OK)
				this.txtExt.Text = exts.Extensions;
		}

		//save the information for a search
		private void SaveOldTextData()
		{
			string[] texts = { this.txtDir.Text, this.txtExt.Text.Trim('.'), this.txtSearch.Text };
			Dictionary<string, DateTime>[] dicts = { dirs, exts, searches };
			//loop for each combo box
			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i].Trim() == "")
					continue;

				//add the new text string with the current time
				if (dicts[i].ContainsKey(texts[i]))
					dicts[i].Remove(texts[i]);
				dicts[i].Add(texts[i], DateTime.Now);
			}

			//re-fill the combo boxes
			LoadComboBoxes();
		}

		private void LoadComboBoxes()
		{
			ComboBox[] boxes = { this.txtDir, this.txtExt, this.txtSearch };
			Dictionary<string, DateTime>[] dicts = { dirs, exts, searches };
			//loop for each combo box
			for (int i = 0; i < boxes.Length; i++)
			{
				//clear combo box items
				boxes[i].Items.Clear();

				//add the items from the dictionaries and check for removal
				List<string> remove = new List<string>();
				foreach (string text in dicts[i].Keys)
				{
					//insert it into the combo box, even if it is being removed
					boxes[i].Items.Insert(0, (text));

					//check for removal
					if (DateTime.Now.Subtract(dicts[i][text]) > settings.Span)
						remove.Add(text);
				}

				//remove items that are too old
				foreach (string s in remove)
					dicts[i].Remove(s);

				//remove the oldest item while the count is higher than the max
				while (dicts[i].Count > settings.MaxHistItems)
				{
					string oldestKey = null;
					DateTime oldest = DateTime.Now;

					//find oldest item
					foreach (string text in dicts[i].Keys)
					{
						if (dicts[i][text] < oldest)
						{
							oldestKey = text;
							oldest = dicts[i][text];
						}
					}

					//remove the item
					if (oldestKey != null)
						dicts[i].Remove(oldestKey);
				}
			}
		}

		//name of data file
		const string datName = "SUtil.dat";

		private void SaveFormData()
		{
			FileStream fs = new FileStream(datName, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);

			//version number
			bw.Write("1.2.4.1");

			//save what is currently in the text fields and checkboxes
			bw.Write(this.txtDir.Text);
			bw.Write(this.txtExt.Text);
			bw.Write(this.txtSearch.Text);

			bw.Write(this.chxExt.Checked);
			bw.Write(this.chxSubDir.Checked);
			bw.Write(this.chxCase.Checked);

			//save old combo box items
			SaveDict(bw, dirs);
			SaveDict(bw, exts);
			SaveDict(bw, searches);

			//save settings
			settings.Save(bw);

			bw.Flush();
			bw.Close();
			fs.Close();
			fs.Dispose();
		}

		private void LoadData()
		{
			this.settings = null;

			if (File.Exists(datName))
			{
				FileStream fs = new FileStream(datName, FileMode.Open);
				BinaryReader br = new BinaryReader(fs);

				try
				{
					string version = br.ReadString();

					//check version
					if (version != "1.2.4.0" && version != "1.2.4.1")
						throw new Exception();

					//load the text fields and checkboxes
					this.txtDir.Text = br.ReadString();
					this.txtExt.Text = br.ReadString();
					this.txtSearch.Text = br.ReadString();

					this.chxExt.Checked = br.ReadBoolean();
					this.chxSubDir.Checked = br.ReadBoolean();
					this.chxCase.Checked = br.ReadBoolean();

					//load old combo box items
					dirs = LoadDict(br);
					exts = LoadDict(br);
					searches = LoadDict(br);

					//load settings
					this.settings = Settings.Load(br);
				}
				catch { }

				br.Close();
				fs.Close();
				fs.Dispose();
			}

			SettingsChanged();

			LoadComboBoxes();
		}

		//save a dictionary
		private void SaveDict(BinaryWriter bw, Dictionary<string, DateTime> dict)
		{
			//write number of entries
			bw.Write(dict.Count);
			foreach (string key in dict.Keys)
			{
				//write each entry
				bw.Write(key);
				bw.Write(dict[key].Ticks);
			}
		}

		//load a dictionary
		private Dictionary<string, DateTime> LoadDict(BinaryReader br)
		{
			Dictionary<string, DateTime> result = new Dictionary<string, DateTime>();

			//read the number of entries and read that amount of entries
			int count = br.ReadInt32();
			for (int i = 0; i < count; i++)
				result.Add(br.ReadString(), new DateTime(br.ReadInt64()));

			return result;
		}

		#endregion


		#region form events

		private void btnOwner_Click(object sender, EventArgs e)
		{
			//OwnerSearch.Show();
		}

		private void chxExt_CheckedChanged(object sender, EventArgs e)
		{
			//refresh extension textbox and button
			this.txtExt.Enabled = this.chxExt.Checked;
			this.btnAdvanced.Enabled = this.chxExt.Checked;
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			//start at either current directory or last directory
			if (Directory.Exists(this.txtDir.Text))
				this.folderBrowserDialog1.SelectedPath = this.txtDir.Text;
			else if (Directory.Exists(context))
				this.folderBrowserDialog1.SelectedPath = context;

			//show directory dialog and if OK set text
			if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				this.txtDir.Text = this.folderBrowserDialog1.SelectedPath;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			/*** debug stuff ***/
			//this.rtbResults.Text = resultText;
			//return;
			//this.rtbResults.Text = this.rtbResults.Rtf;
			//return;

			//check if the app is already performing a search
			if (searching)
				AbortSearch();
			else
				DoSearch();
		}

		private void rtbResults_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (settings.NotepadDoubleclick)
				OpenWithNotepad(e.Location);
		}

		private void rtbResults_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && settings.NotepadRightClick)
				OpenWithNotepad(e.Location);
		}

		private void rtbResults_TextChanged(object sender, EventArgs e)
		{
			if (this.rtbResults.Text.Length > 0)
			{
				if (settings.AutoScrollResults)
				{
					//only way I can find to programatically scroll the rtb...
					this.rtbResults.Select(this.rtbResults.Text.Length - 1, 0);
					this.rtbResults.ScrollToCaret();
				}
			}
		}

		private void btnAdvanced_Click(object sender, EventArgs e)
		{
			GetAdvancedExtensionSettings();
		}

		private void btnCustomize_Click(object sender, EventArgs e)
		{
			Customize();
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void SearchUtil_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveFormData();
		}

		private void txtDir_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				this.btnOpen_Click(null, null);
		}

		private void txtExt_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				this.btnAdvanced_Click(null, null);
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				this.btnSearch_Click(null, null);
		}

		private void checkbox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				((CheckBox)sender).Checked = !((CheckBox)sender).Checked;
		}

		#endregion

	}
}