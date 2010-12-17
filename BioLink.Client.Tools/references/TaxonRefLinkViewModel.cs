﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class TaxonRefLinkViewModel : GenericViewModelBase<TaxonRefLink> {

        public TaxonRefLinkViewModel(TaxonRefLink model)
            : base(model) {
        }

        public int RefLinkID {
            get { return Model.RefLinkID; }
            set { SetProperty(() => Model.RefLinkID, value); }
        }

        public string RefLink {
            get { return Model.RefLink; }
            set { SetProperty(() => Model.RefLink, value); }
        }

        public int? BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string FullName {
            get { return Model.FullName; }
            set { SetProperty(() => Model.FullName, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public bool? UseInReports {
            get { return Model.UseInReports; }
            set { SetProperty(() => Model.UseInReports, value); }
        }
        
    }
}
