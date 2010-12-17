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
using BioLink.Data;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalSectionDetails.xaml
    /// </summary>
    public partial class JournalSectionDetails : UserControl {

        public JournalSectionDetails() {
            InitializeComponent();
        }

        public JournalSectionDetails(User user) {
            InitializeComponent();
            txtJournal.BindUser(user, Extensibility.LookupType.Journal);
        }



    }
}