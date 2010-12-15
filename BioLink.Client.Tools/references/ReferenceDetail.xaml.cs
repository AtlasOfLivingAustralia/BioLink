﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using BioLink.Data;
using System.IO;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ReferenceDetail.xaml
    /// </summary>
    public partial class ReferenceDetail : DatabaseActionControl {

        private ReferenceViewModel _viewModel;

        #region Designer Constructor

        public ReferenceDetail() {
            InitializeComponent();
        }

        #endregion

        private TabItem _notesTab;
        private TabItem _traitsTab;
        private TabItem _mmTab;

        public ReferenceDetail(User user, int referenceID)
            : base(user, "Reference:" + referenceID) {

            InitializeComponent();

            var refTypeList = new List<RefTypeMapping>();
            foreach (string reftypecode in SupportService.RefTypeMap.Keys) {
                refTypeList.Add(SupportService.RefTypeMap[reftypecode]);
            }

            Reference model = null;
            if (referenceID >= 0) {
                var service = new SupportService(user);
                model = service.GetReference(referenceID);
            } else {
                model = new Reference();
                model.RefID = -1;
                txtRefCode.IsReadOnly = false;
                Loaded += new RoutedEventHandler(ReferenceDetail_Loaded);
                model.RefType = refTypeList[0].RefTypeCode;
            }

            _viewModel = new ReferenceViewModel(model);

            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            this.DataContext = _viewModel;

            cmbRefType.ItemsSource = refTypeList;
            cmbRefType.DisplayMemberPath = "RefTypeName";

            txtPossess.BindUser(User, PickListType.Phrase, "Reference Possess", TraitCategoryType.Reference);
            txtSource.BindUser(User, PickListType.Phrase, "Reference Source", TraitCategoryType.Reference);

            _traitsTab = tabRef.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Reference, _viewModel.RefID));
            _notesTab = tabRef.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Reference, _viewModel.RefID));
            _mmTab = tabRef.AddTabItem("Multimedia", new MultimediaControl(User, TraitCategoryType.Reference, _viewModel.RefID));

            tabRef.AddTabItem("Ownership", new OwnershipDetails(_viewModel.Model));

            if (model.RefID < 0) {
                _traitsTab.IsEnabled = false;
                _notesTab.IsEnabled = false;
                _mmTab.IsEnabled = false;
            }

            cmbRefType.SelectionChanged += new SelectionChangedEventHandler(cmbRefType_SelectionChanged);

            this.ChangesCommitted += new PendingChangesCommittedHandler(ReferenceDetail_ChangesCommitted);

        }

        void cmbRefType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var refTypeMapping = cmbRefType.SelectedItem as RefTypeMapping;
            gridSpecific.Children.Clear();
            if (refTypeMapping != null) {
                FrameworkElement control = null;
                switch (refTypeMapping.RefTypeCode) {
                    case "J":
                        control = new JournalDetails(User);
                        break;
                    case "JS":
                        control = new JournalSectionDetails(User);
                        break;
                    case "B":
                        control = new BookDetails();
                        break;
                    case "BS":
                        control = new BookSectionDetails();
                        break;
                    case "M":
                        control = new MiscDetails();
                        break;
                    case "U":
                        control = new InternetURLDetails();
                        break;
                }

                if (control != null) {
                    control.DataContext = _viewModel;
                    gridSpecific.Children.Add(control);
                }
            }
        }

        void ReferenceDetail_ChangesCommitted(object sender) {
            _traitsTab.IsEnabled = true;
            _notesTab.IsEnabled = true;
            _mmTab.IsEnabled = true;
        }

        void ReferenceDetail_Loaded(object sender, RoutedEventArgs e) {
            if (_viewModel != null && _viewModel.RefID < 0) {
                RegisterUniquePendingChange(new InsertReferenceAction(_viewModel.Model));
                txtRefCode.Focus();
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var r = viewmodel as ReferenceViewModel;
            if (r != null) {
                RegisterUniquePendingChange(new UpdateReferenceAction(r.Model));
            }

            DisplayRTFPreview(txtPreview, BuildRefRTF(r.Model));
        }

        public void DisplayRTFPreview(RichTextBox control, string rtf) {
            if (!rtf.StartsWith(@"{{\rtf")) {
                rtf = string.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deftab720 {{\fonttbl{{\f1\fswiss Arial;}}}} \plain\f1\fs16 {0} }}", rtf);
            }

            var doc = control.Document;
            if (string.IsNullOrEmpty(rtf)) {
                doc.Blocks.Clear();
            } else {
                using (var t = new CodeTimer("Loading RTF")) {
                    using (var stream = new MemoryStream((new ASCIIEncoding()).GetBytes(rtf))) {
                        var text = new TextRange(doc.ContentStart, doc.ContentEnd);
                        text.Load(stream, DataFormats.Rtf);
                    }
                }
            }
        }

        private string AtEnd(string pstrMain, string pstrLookFor) {
            //'
            //' If the main string doesn't have the find string at the end, then put it there.
            //'
            if (!pstrMain.EndsWith(pstrLookFor)) {
                return pstrMain + pstrLookFor;
            }

            return pstrMain;
        }

        private string ProcRefPages(string pstrPages, PagesType pagesType) {

            String strPages = pstrPages.Trim();

            //  Replace dashes with NRules.
            strPages = strPages.Replace('-', (char)150);

            // Ensure there is no dash(NRules) without a closing number.
            if (strPages[0] == 150) {
                strPages = strPages + " *** Closing page expected ***";
            }

            // Install the pp for pages of a book.
            switch (pagesType) {
                case PagesType.REF_SECTION_PAGES:
                    if (!strPages.Contains((char)150)) {
                        if (!strPages.Contains("p")) {
                            strPages = "p. " + strPages;
                        }
                    } else {
                        if (!strPages.Contains("pp")) {
                            strPages = "pp. " + strPages;
                        }
                    }
                    break;
                case PagesType.ALWAYS_RANGE:
                    if (!strPages.Contains((char)150)) {
                        if (!strPages.Contains("pp")) {
                            strPages = strPages + "pp.";
                        }
                    } else {
                        if (!strPages.Contains("pp")) {
                            strPages = "pp. " + strPages;
                        }
                    }
                    break;
            }

            return strPages;
        }

        private String ProcessEdition(String edition) {

            if (edition == "11") {
                edition += "th";
            } else {
                char ch = edition[edition.Length - 1];
                if (Char.IsNumber(ch)) {
                    switch (ch) {
                        case '1':
                            edition += "st";
                            break;
                        case '2':
                            edition += "nd";
                            break;
                        case '3':
                            edition += "rd";
                            break;
                        default:
                            edition += "th";
                            break;
                    }
                }
            }

            if (!edition.ToUpper().Contains("EDITION") || !edition.ToUpper().Contains("EDN")) {
                edition += " Edn.";
            }

            return edition;
        }


        //    ProcRefPages = strPages

        //End Function

        public string BuildRefRTF(Reference model) {
            // This routine builds the reference depening on its type.


            // extract the reference type
            string strRefType = model.RefType;
            // write the author
            StringBuilder strRTF = new StringBuilder();

            strRTF.Append(model.Author);

            // add the year
            strRTF.Append(" (").Append(model.YearOfPub).Append(").");
            // add the title.

            String strTitle = model.Title;

            if (strTitle.EndsWith("}")) {
                strRTF.Append(" ").Append(AtEnd(strTitle.Substring(0, strTitle.Length - 1), ".")).Append("}");
            } else {
                strRTF.Append(" ").Append(AtEnd(strTitle, "."));
            }

            // Perform the type specific markup...
            switch (strRefType) {
                case "J":
                    strRTF.AppendFormat(@"\i {0} \i0 ", AtEnd(model.JournalName, "."));
                    // Write the series if not blank.
                    if (!String.IsNullOrEmpty(model.Series)) {
                        strRTF.AppendFormat(@"\i ({0}) \i0 ", model.Series);
                    }

                    // Write the volume.
                    strRTF.AppendFormat(@"\b {0} \b0 ", model.Volume);
                    // Write the part number
                    if (!String.IsNullOrEmpty(model.PartNo)) {
                        strRTF.AppendFormat(@" ({0}):", model.Volume);
                    } else {
                        strRTF.Append(" :");
                    }
                    // Write the pages.
                    strRTF.AppendFormat(" {0}", ProcRefPages(model.Pages, PagesType.NO_PP));
                    break;
                case "JS":
                    strRTF.AppendFormat(@" {0} \i in \i0 {1} {2} \i {3} \i0 ", ProcRefPages(model.Pages, PagesType.REF_SECTION_PAGES), AtEnd(model.Editor, "."), AtEnd(model.BookTitle, "."), AtEnd(model.JournalName, "."));

                    //Write the series if not blank.
                    if (!string.IsNullOrEmpty(model.Series)) {
                        strRTF.AppendFormat(@"\i {0} \i0 ", model.Series);
                    }
                    // Write the volume.
                    strRTF.AppendFormat("\b  {0} \b0 ", model.Volume);
                    // Write the part number
                    if (!string.IsNullOrEmpty(model.PartNo)) {
                        strRTF.AppendFormat(" ({0}):", model.PartNo);
                    } else {
                        strRTF.Append(" :");
                    }
                    // Write the pages.
                    strRTF.AppendFormat(" {0}", ProcRefPages(model.TotalPages, PagesType.NO_PP));
                    break;
                case "B":
                    // Write the place of publication
                    strRTF.AppendFormat(" {0} : {1}", model.Place, AtEnd(model.Publisher, "."));

                    if (!String.IsNullOrEmpty(model.Volume)) {
                        strRTF.AppendFormat(" Vol. {0}", model.Volume);
                    }
                    // Write the edition
                    if (!String.IsNullOrEmpty(model.Edition)) {
                        strRTF.AppendFormat(" {0}", ProcessEdition(model.Edition));
                    }
                    // Write the Total pages
                    if (!String.IsNullOrEmpty(model.TotalPages)) {
                        strRTF.AppendFormat(" {0}", AtEnd(ProcRefPages(model.TotalPages, PagesType.ALWAYS_RANGE), "."));

                    }
                    break;
                case "BS":
                    // Write the pages...
                    if (!string.IsNullOrEmpty(model.Pages)) {
                        strRTF.AppendFormat(@" {0}\i  in \i0 ", ProcRefPages(model.Pages, PagesType.REF_SECTION_PAGES));
                    } else {
                        strRTF.AppendFormat(@"\i  In \i0 ");
                    }


                    //Write the editors.
                    strRTF.AppendFormat(@" {0} \i  {1}\i0 {2} : {3}", model.Editor, AtEnd(model.BookTitle, "."), model.Place, model.Publisher);
                    // Write the volume
                    if (!String.IsNullOrEmpty(model.Volume)) {
                        strRTF.AppendFormat(" Vol. {0}", model.Volume);
                    }
                    // Write the edition
                    if (!String.IsNullOrEmpty(model.Edition)) {
                        strRTF.AppendFormat(" {0}", ProcessEdition(model.Edition));
                    }
                    // Write the Total pages
                    if (!String.IsNullOrEmpty(model.TotalPages)) {
                        strRTF.AppendFormat(" {0}", AtEnd(ProcRefPages(model.TotalPages, PagesType.ALWAYS_RANGE), "."));
                    }
                    break;
                case "M":
                case "U":
                    // Write the book/journal/misc details.
                    if (!String.IsNullOrEmpty(model.BookTitle)) {
                        strRTF.AppendFormat(" {0}", AtEnd(model.BookTitle, "."));
                    }
                    // Write the Total pages
                    if (!String.IsNullOrEmpty(model.Pages)) {
                        strRTF.AppendFormat(" {0}", AtEnd(ProcRefPages(model.Pages, PagesType.ALWAYS_RANGE), "."));
                    }
                    break;
            }

            return strRTF.ToString();

        }

    }

    public class RefTypeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string reftype = value as string;
            if (reftype != null) {
                if (SupportService.RefTypeMap.ContainsKey(reftype)) {
                    return SupportService.RefTypeMap[reftype];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var pair = value as RefTypeMapping;
            if (pair != null) {
                return pair.RefTypeCode;
            }
            return null;
        }

    }

    enum PagesType {
        NO_PP = 0,
        REF_SECTION_PAGES = 1,
        ALWAYS_RANGE = 2,
    }
}
