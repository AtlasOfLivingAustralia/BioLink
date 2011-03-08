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
using System.Data;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for RowEditorWindow.xaml
    /// </summary>
    public partial class RowEditorWindow : Window {

        private Dictionary<string, string> _changeMap = new Dictionary<string, string>();

        public RowEditorWindow(ImportStagingService service, int rowId, List<ImportFieldMapping> mappings, DataRowView row) {
            InitializeComponent();
            this.Service = service;
            this.RowID = rowId;
            this.Row = row;
            this.Mappings = mappings;

            int rowIndex = 0;
            foreach (ImportFieldMapping mapping in mappings) {

                gridFields.RowDefinitions.Add(CreateRowDefinition());

                var lbl = new Label();
                lbl.SetValue(Grid.RowProperty, rowIndex);
                lbl.Content = mapping.SourceColumn;
                gridFields.Children.Add(lbl);

                var txt = new Extensibility.TextBox();
                txt.SetValue(Grid.ColumnProperty, 1);
                txt.SetValue(Grid.RowProperty, rowIndex);
                txt.Height = 23;
                var objValue = row[mapping.SourceColumn];
                var txtValue = (objValue == null ? "" : objValue.ToString());
                txt.Text = txtValue;

                var fieldMapping = mapping; // need to keep a copy of this mapping so its captured in the closure

                txt.TextChanged += new TextChangedEventHandler((source, e) => {
                    
                    var textbox = source as TextBox;                    
                    if (textbox.Text != txtValue) {
                        _changeMap[fieldMapping.SourceColumn] = textbox.Text;
                    } else {
                        if (_changeMap.ContainsKey(fieldMapping.SourceColumn)) {
                            _changeMap.Remove(fieldMapping.SourceColumn);
                        }
                    }
                });
                
                gridFields.Children.Add(txt);

                rowIndex++;
            }
        }

        private RowDefinition CreateRowDefinition() {
            var def = new RowDefinition();
            def.Height = new GridLength(28);

            return def;

        }

        public Dictionary<string, string> Changes { 
            get { return _changeMap; } 
        }

        protected int RowID { get; private set; }
        protected ImportStagingService Service { get; private set; }
        protected List<ImportFieldMapping> Mappings { get; private set; }
        protected DataRowView Row { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }


    }
}
