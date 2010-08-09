﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BioLink.Client.Extensibility;
using System.ComponentModel;

namespace BioLink.Client.Taxa {

    public class SortedTaxonConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            IEnumerable<HierarchicalViewModelBase> taxa = value as IEnumerable<HierarchicalViewModelBase>;
            ListCollectionView lcv = (ListCollectionView) CollectionViewSource.GetDefaultView(taxa);

            // Available and Literature Names are always at the top...
            lcv.SortDescriptions.Add(new SortDescription("IsAvailableOrLiteratureName", ListSortDirection.Descending)); // bool            

            // We are the sort mode is manual, then the order will take precedence over the name
            if (TaxonExplorer.IsManualSort) {
                lcv.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending)); // int
            }

            // And finally the name
            lcv.SortDescriptions.Add(new SortDescription("Epithet", ListSortDirection.Ascending));

            return lcv;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
