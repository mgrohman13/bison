//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.Windows.Forms;
//using SearchCommon;

//namespace SearchUtil
//{
//    public partial class OwnerSearch : Form
//    {

//        #region static

//        static OwnerSearch form;
//        //Settings
		
//        internal static void SetSettings(Settings settings)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        internal new static void Show()
//        {
//            if (form == null)
//                form = new OwnerSearch();
//        }A

//        #endregion


//        #region constructors and fields

//        private OwnerSearch()
//        {
//            InitializeComponent();
//        }

//        private void InitializeComponent()
//        {
//            this.SuspendLayout();
//            // 
//            // OwnerSearch
//            // 
//            this.ClientSize = new System.Drawing.Size(292, 273);
//            this.Name = "OwnerSearch";
//            this.Text = "Search By Owner";
//            this.ResumeLayout(false);

//        }

//        #endregion

//    }
//}