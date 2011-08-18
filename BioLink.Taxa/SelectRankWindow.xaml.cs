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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    /// <summary>
    /// Interaction logic for SelectRankWindow.xaml
    /// </summary>
    public partial class SelectRankWindow : Window {

        private Dictionary<string, TaxonRank> _map;

        public SelectRankWindow() {

            InitializeComponent();

            var service = new TaxaService(PluginManager.Instance.User);
            _map = service.GetTaxonRankMap();

            var kingdoms = service.GetKingdomList().Where((kingdom) => {
                return !string.IsNullOrWhiteSpace(kingdom.KingdomCode);
            });

            cmbKingdom.SelectionChanged += new SelectionChangedEventHandler(cmbKingdom_SelectionChanged);

            cmbKingdom.ItemsSource = kingdoms;
            if (kingdoms.Count() > 0) {
                cmbKingdom.SelectedIndex = 0;
            }

            this.DataContext = this;

        }

        public bool CanSelect {
            get { return SelectedRank != null; }
        }

        void cmbKingdom_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_map != null) {
                var kingdom = cmbKingdom.SelectedItem as Kingdom;
                if (kingdom != null) {
                    var list = _map.Values.Where((rank) => {
                        return rank.KingdomCode == kingdom.KingdomCode;
                    });

                    cmbRank.ItemsSource = list;

                    if (list.Count() > 0) {
                        cmbRank.SelectedIndex = 0;
                    }
                }
            }
        }

        public TaxonRank SelectedRank {
            get { return cmbRank.SelectedItem as TaxonRank;  }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (SelectedRank != null) {
                this.DialogResult = true;
                Close();
            }
        }
    }
}