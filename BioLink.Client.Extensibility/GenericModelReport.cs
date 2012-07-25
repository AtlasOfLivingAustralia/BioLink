﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// A generic report model that accepts a ViewModel collection, and provides a convenient way to export a listview model as a report (and therefore to any other exportable format)
    /// 
    /// Column names are expected to be either the name of a Model member, or in the format 'memberName[display name]'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericModelReport<T> : ReportBase where T : ViewModelBase {

        private string _reportName;

        public GenericModelReport(User user, string reportName, ICollection<T> model, params String[] columns) : base(user) {
            RegisterViewer(new TabularDataViewerSource());
            _reportName = reportName;
            Model = model;
            Columns = columns;
        }

        public override string Name {
            get { return _reportName; }
        }

        public override Data.DataMatrix ExtractReportData(Utilities.IProgressObserver progress) {
            DataMatrix matrix = new DataMatrix();
            var displayNameRegex = new Regex(@"^(\w+)\[(.+)\]$");
            // Cache the property getter
            Dictionary<String, MethodInfo> getters = new Dictionary<String, MethodInfo>();            
            foreach (String colName in Columns) {
                string displayName = colName;
                string memberName = colName;

                var m = displayNameRegex.Match(colName);
                if (m.Success) {
                    memberName = m.Groups[1].Value;
                    displayName = m.Groups[2].Value;
                }

                matrix.Columns.Add(new MatrixColumn { Name = displayName, IsHidden = false });
                var p = typeof(T).GetProperty(memberName);
                if (p != null) {
                    getters[colName] = p.GetGetMethod();
                }
                
            }

            foreach (T vm in Model) {
                var row = matrix.AddRow();
                int i = 0;
                foreach (String colName in Columns) {
                    Object value = null;
                    if (getters.ContainsKey(colName)) {
                        value = getters[colName].Invoke(vm, null);
                    }
                    row[i++] = value;
                }
            }

            return matrix;
        }

        protected ICollection<T> Model { get; private set; }

        protected String[] Columns { get; private set; }

    }
}
