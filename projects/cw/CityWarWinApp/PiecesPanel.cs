using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CityWar;
using System.Drawing;

namespace CityWarWinApp
{
	class PiecesPanel : Panel
	{
		private static Font font = new Font("Arial", 13);
		Pen framePen = new Pen(Color.Black, 3);

		public enum DrawFlags
		{
			Frame = 0x1,
			Background = 0x2,
			Text = 0x4,
		}

		private System.Windows.Forms.VScrollBar sbPieces;

		public delegate Piece[] GetPiecesDelegate();
		public delegate MattUtil.EnumFlags<DrawFlags> GetDrawFlagsDelegate(Piece piece);

		public delegate string GetTextDelegate(Piece piece);
		public delegate Brush GetTextBrushDelegate();

		private GetPiecesDelegate GetPieces;
		private GetDrawFlagsDelegate GetDrawFlags;
		private GetTextDelegate GetText;
		private GetTextBrushDelegate GetTextBrush;

		public PiecesPanel()
		{
			InitializeComponent();
			//this line is the whole reason for this class
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

			this.GetPieces = delegate() { return new Piece[0]; };
		}

		public void Initialize(GetPiecesDelegate GetPieces, GetDrawFlagsDelegate GetDrawFlags,
			GetTextDelegate GetText, GetTextBrushDelegate GetTextBrush)
		{
			this.GetPieces = GetPieces;
			this.GetDrawFlags = GetDrawFlags;
			this.GetText = GetText;
			this.GetTextBrush = GetTextBrush;
		}

		public bool ScrollBarEnabled
		{
			get { return sbPieces.Enabled; }
		}

		void sbPieces_Scroll(object sender, ScrollEventArgs e)
		{
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			Piece[] pieces = GetPieces();
			int numPieces = pieces.Length;

			int numColumns = GetNumColumns();
			if (numColumns < 1)
				throw new Exception();

			//set the proper scroll bar height
			RefreshScrollBar(numPieces, numColumns);

			int end = (getYIndex(this.Height) + 1) * numColumns;
			if (end > numPieces)
				end = numPieces;
			for (int a = getYIndex(0) * numColumns; a < end; ++a)
			{
				int x = 13 + 113 * (a % numColumns);
				int y = 13 - sbPieces.Value + (a / numColumns) * 126;

				Piece currentPiece = pieces[a];

				MattUtil.EnumFlags<DrawFlags> drawFlags = GetDrawFlags(currentPiece);

				if (drawFlags.Contains(DrawFlags.Background))
					g.FillRectangle(Brushes.LightGray, x, y, 100, 100);

				//draw the piece
				g.DrawImage(currentPiece.Owner.GetConstPic(currentPiece.Name), x, y);

				Unit currentUnit = currentPiece as Unit;
				if (currentUnit != null)
				{
					//draw the health bar
					g.FillRectangle(Brushes.Black, x - 1, y + 101, 103, 5);
					g.FillRectangle(currentUnit.HealthBrush, x, y + 102,
						(int)Math.Round(currentUnit.GetHealthPct() * 101), 3);
				}

				if (drawFlags.Contains(DrawFlags.Frame))
					g.DrawRectangle(framePen, x, y, 100f, 100f);
				if (drawFlags.Contains(DrawFlags.Text))
					g.DrawString(GetText(currentPiece), font, GetTextBrush(), x, y + 103);
			}
			base.OnPaint(e);
		}

		private int GetNumColumns()
		{
			return (this.Width - this.sbPieces.Width - 13) / 113;
		}

		private void RefreshScrollBar(int numPieces, int numColumns)
		{
			numPieces = (int)Math.Ceiling((double)numPieces / numColumns);
			int needed = 126 * numPieces + 13;
			int max = 0;
			if (needed > Height)
			{
				max = needed - Height + 1;
				sbPieces.Enabled = true;
			}
			else
			{
				sbPieces.Value = 0;
				sbPieces.Enabled = false;
			}

			max += sbPieces.LargeChange - 2;

			if (sbPieces.Value > max)
				sbPieces.Value = 0;
			if (sbPieces.LargeChange > max)
				max = sbPieces.LargeChange;
			sbPieces.Maximum = max;

			sbPieces.Invalidate();
		}

		public Piece GetClickedPiece(MouseEventArgs e)
		{
			int index = GetPieceIndex(e.X, e.Y);
			Piece[] pieces = GetPieces();
			if (index < pieces.Length)
				return pieces[index];
			else
				return null;
		}
		int GetPieceIndex(int x, int y)
		{
			int numColumns = GetNumColumns();
			int xIndex = 0;
			if (numColumns > 1)
			{
				xIndex = (x - 7) / 113;
				if (xIndex < 0)
					xIndex = 0;
				if (xIndex >= numColumns)
					xIndex = numColumns - 1;
			}
			return getYIndex(y) * numColumns + xIndex;
		}
		private int getYIndex(int y)
		{
			int yIndex = (y + sbPieces.Value - 13) / 126;
			return yIndex < 0 ? 0 : yIndex;
		}

		public void PiecesPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			UseMouseWheel(sbPieces, e, sbPieces_Scroll);
			//this.Invalidate();
		}

		internal void ScrollToSelected()
		{
			Piece[] pieces = GetPieces();
			int length = pieces.Length;
			int firstIndex = -1, lastIndex = -1;
			for (int i = 0; i < length; ++i)
			{
				MattUtil.EnumFlags<DrawFlags> flags = GetDrawFlags(pieces[i]);
				if (flags.Contains(DrawFlags.Frame))
				{
					if (firstIndex == -1)
						firstIndex = i;
					lastIndex = i;
				}
			}

			int numColumns = GetNumColumns();
			int newValue = (lastIndex / numColumns) * 126 - this.Height + 139;

			int maxValue = (firstIndex / numColumns) * 126;
			if (newValue > maxValue)
				newValue = maxValue;

			RefreshScrollBar(length, numColumns);

			if (newValue < 0)
				newValue = 0;
			else
			{
				maxValue = GetRealScrollMax();
				if (newValue > maxValue)
					newValue = maxValue;
			}

			int oldValue = sbPieces.Value;
			this.sbPieces.Value = newValue;

			sbPieces_Scroll(sbPieces, new ScrollEventArgs(ScrollEventType.EndScroll, oldValue, newValue));
		}

		public static void UseMouseWheel(ScrollBar scrollBar, MouseEventArgs e, ScrollEventHandler scrollEvent)
		{
			if (!scrollBar.Enabled)
				return;

			int newValue = scrollBar.Value - Math.Sign(e.Delta) * scrollBar.SmallChange;

			if (newValue < 0)
				newValue = 0;
			else
			{
				int max = GetRealScrollMax(scrollBar);
				if (newValue > max)
					newValue = max;
			}

			int oldValue = scrollBar.Value;
			scrollBar.Value = newValue;
			if (scrollEvent != null)
				scrollEvent(scrollBar, new ScrollEventArgs(ScrollEventType.EndScroll, oldValue, newValue));
		}

		private int GetRealScrollMax()
		{
			return GetRealScrollMax(sbPieces);
		}
		private static int GetRealScrollMax(ScrollBar scrollBar)
		{
			return scrollBar.Maximum - scrollBar.LargeChange + 1;
		}

		private void InitializeComponent()
		{
			this.sbPieces = new System.Windows.Forms.VScrollBar();
			this.SuspendLayout();
			// 
			// sbPieces
			// 
			this.sbPieces.Dock = System.Windows.Forms.DockStyle.Right;
			this.sbPieces.LargeChange = 126;
			this.sbPieces.Location = new System.Drawing.Point(184, 0);
			this.sbPieces.Maximum = 126;
			this.sbPieces.Name = "sbPieces";
			this.sbPieces.Size = new System.Drawing.Size(16, 100);
			this.sbPieces.SmallChange = 42;
			this.sbPieces.TabIndex = 0;
			this.sbPieces.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbPieces_Scroll);
			// 
			// PiecesPanel
			// 
			this.Controls.Add(this.sbPieces);
			this.ResumeLayout(false);
		}
	}
}
