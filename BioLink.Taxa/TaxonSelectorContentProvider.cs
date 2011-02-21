﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxonSelectorContentProvider : IHierarchicalSelectorContentProvider {

        private TaxonExplorer _explorer;

        public TaxonSelectorContentProvider(User user, TaxonExplorer explorer) {
            this.User = user;
            _explorer = explorer;
        }

        public string Caption {
            get { return "Select a taxon"; }
        }

        public bool CanAddNewItem {
            get { return false; }
        }

        public bool CanDeleteItem {
            get { return false; }
        }

        public bool CanRenameItem {
            get { return true; }
        }

        public List<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent) {                        
            var service = new TaxaService(User);
            List<Taxon> model = null;
            if (parent == null) {
                model = service.GetTopLevelTaxa();
            } else {
                model = service.GetTaxaForParent((parent as TaxonViewModel).TaxaID.Value);
            }

            if (model != null) {
                var list = new List<HierarchicalViewModelBase>(model.ConvertAll((m) => {
                    return new TaxonViewModel(parent, m, _explorer.GenerateTaxonDisplayLabel);
                }));
                return list;
            }

            return null;            
        }

        public List<HierarchicalViewModelBase> Search(string searchTerm) {
            var service = new TaxaService(User);
            var list = service.FindTaxa(searchTerm);
            return new List<HierarchicalViewModelBase>(list.ConvertAll((m) => {
                return new TaxonViewModel(null, m, _explorer.GenerateTaxonDisplayLabel);
            }));
        }

        public bool CanSelectItem(HierarchicalViewModelBase candidate) {
            var taxon = candidate as TaxonViewModel;
            return taxon != null;
        }

        public SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem) {
            var taxon = selectedItem as TaxonViewModel;
            if (taxon != null) {
                var result = new SelectionResult();
                result.DataObject = taxon;
                result.ObjectID = taxon.TaxaID;
                result.Description = taxon.TaxaFullName;
                return result;
            }
            return null;
        }

        public Data.DatabaseAction AddNewItem(HierarchicalViewModelBase selectedItem) {
            throw new NotImplementedException();
        }

        public Data.DatabaseAction RenameItem(HierarchicalViewModelBase selectedItem, string newName) {
            var taxon = selectedItem as TaxonViewModel;
            if (taxon != null) {
                TaxonName name = TaxonNameParser.ParseName(taxon, newName);
                if (name != null) {
                    taxon.Author = name.Author;
                    taxon.Epithet = name.Epithet;
                    taxon.YearOfPub = name.Year;
                    taxon.ChgComb = name.ChangeCombination;
                    return new UpdateTaxonDatabaseAction(taxon.Taxon);                    
                } else {
                    ErrorMessage.Show("Please enter at least the epithet, with author and year where appropriate.");                    
                }
            }
            return null;
        }

        public Data.DatabaseAction DeleteItem(HierarchicalViewModelBase selectedItem) {
            throw new NotImplementedException();
        }

        public int? GetElementIDForViewModel(HierarchicalViewModelBase item) {
            var taxon = item as TaxonViewModel;
            if (taxon != null) {
                return taxon.TaxaID;
            }
            return -1;
        }

        #region Properties

        public User User { get; private set; }

        #endregion
    }
}
