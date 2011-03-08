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
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : WizardPage, IProgressObserver {

        private ObservableCollection<ImportStatusMessage> _messages;
        private ImportProcessor _importProcessor;

        public ImportPage() {
            InitializeComponent();
        }

        public override string PageTitle {
            get {
                return "Import data";
            }
        }

        private void btnSaveTemplate_Click(object sender, RoutedEventArgs e) {
            SaveTemplate();
        }

        private void SaveTemplate() {

            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Template files (*.bip)|*.bip|All files (*.*)|*.*";
            if (dlg.ShowDialog().GetValueOrDefault()) {
                var inifile = new IniFile();
                inifile.SetValue("Import", "ProfileStr", ImportContext.Importer.CreateProfileString());
                inifile.SetValue("Import", "ImportFilter", ImportContext.Importer.Name);
                inifile.SetValue("Import", "FieldCount", ImportContext.FieldMappings.Count());

                int i = 0;
                foreach (ImportFieldMapping mapping in ImportContext.FieldMappings) {
                    var ep = new EntryPoint(mapping.SourceColumn);
                    ep.AddParameter("Mapping", mapping.TargetColumn);
                    ep.AddParameter("Default", mapping.DefaultValue);
                    ep.AddParameter("IsFixed", mapping.IsFixed.ToString());

                    inifile.SetValue("Mappings", string.Format("Field{0}", i++), ep.ToString());
                }

                inifile.Save(dlg.FileName);
            }
        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartImportAsync();
        }

        private void StartImportAsync() {

            JobExecutor.QueueJob(() => {
                StartImport();
            });
        }

        private void StartImport() {
            _messages = new ObservableCollection<ImportStatusMessage>();

            this.InvokeIfRequired(() => {
                lvw.ItemsSource = _messages;
            });

            StatusMsg(ImportStatusLevel.Info, "Import started");

            _importProcessor = new ImportProcessor(this.FindParentWindow(), ImportContext.Importer, ImportContext.FieldMappings, this, StatusMsg);
            _importProcessor.Import();

            StatusMsg(ImportStatusLevel.Info, "Import finished");
        }

        public void StatusMsg(ImportStatusLevel level, string message) {
            this.InvokeIfRequired(() => {
                var msg = new ImportStatusMessage { Timestamp = DateTime.Now, Message = message, Level = level };
                _messages.Add(msg);
                lvw.SelectedItem = msg;
                lvw.ScrollIntoView(msg);
            });
        }


        public void ProgressStart(string message, bool indeterminate = false) {
            this.InvokeIfRequired(() => {
                progressBar.IsIndeterminate = indeterminate;
                progressBar.Minimum = 0;
                progressBar.Maximum = 100;
                progressBar.Value = 0;
                lblStatus.Content = message;
            });
        }

        public void ProgressMessage(string message, double? percentComplete = null) {
            this.InvokeIfRequired(() => {
                if (percentComplete.HasValue) {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = percentComplete.Value;
                }
                lblStatus.Content = message;
            });
            
        }

        public void ProgressEnd(string message) {
            this.InvokeIfRequired(() => {
                progressBar.IsIndeterminate = false;
                progressBar.Value = progressBar.Maximum;
                lblStatus.Content = message;
            });            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            if (_importProcessor != null) {
                _importProcessor.Cancelled = true;
            }
        }
    }

    public enum ImportStatusLevel {
        Info,
        Warning,
        Error
    }

    class ImportStatusMessage {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public ImportStatusLevel Level { get; set; }
    }

}
