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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalDetails.xaml
    /// </summary>
    public partial class JournalDetails : DatabaseActionControl {

        private JournalViewModel _viewModel;

        private TabItem _traits;
        private TabItem _notes;
        

        #region Designer Ctor
        public JournalDetails() {
            InitializeComponent();
        }
        #endregion

        public JournalDetails(User user, int journalID) : base(user, "Journal:" + journalID) {
            InitializeComponent();
            Journal model = null;

            if (journalID >= 0) {
                var service = new SupportService(user);
                model = service.GetJournal(journalID);
            } else {
                model = new Journal();
                model.JournalID = -1;
                model.FullName = "<New Journal>";
            }

            _traits = tabJournal.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.Journal, journalID));
            _notes = tabJournal.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.Journal, journalID));
            tabJournal.AddTabItem("Ownership", new OwnershipDetails(model));

            _viewModel = new JournalViewModel(model);
            if (_viewModel.JournalID >= 0) {
                _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
            } else {
                Loaded += new RoutedEventHandler(JournalDetails_Loaded);
                _traits.IsEnabled = false;
                _notes.IsEnabled = false;
            }

            ChangesCommitted += new PendingChangesCommittedHandler(JournalDetails_ChangesCommitted);

            this.DataContext = _viewModel;
        }

        void JournalDetails_ChangesCommitted(object sender) {
            _traits.IsEnabled = true;
            _notes.IsEnabled = true;
        }

        void JournalDetails_Loaded(object sender, RoutedEventArgs e) {
            RegisterPendingChange(new InsertJournalAction(_viewModel.Model));
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateJournalAction((viewmodel as JournalViewModel).Model));
        }

    }
}
