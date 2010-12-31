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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RegionDetails.xaml
    /// </summary>
    public partial class RegionDetails : DatabaseActionControl {

        private RegionViewModel _viewModel;

        #region Designer Constructor
        public RegionDetails() {
            InitializeComponent();
        }
        #endregion

        public RegionDetails(User user, int regionId) : base(user, "Region:" + regionId) {
            InitializeComponent();
            this.RegionID = RegionID;

            var service = new MaterialService(user);
            _viewModel = new RegionViewModel(service.GetRegion(regionId));
            this.DataContext = _viewModel;

            _viewModel.DataChanged += new DataChangedHandler(model_DataChanged);

            txtRegionType.BindUser(User, PickListType.Phrase, "Region Rank", TraitCategoryType.Region);

            tabRegion.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Region, ViewModel.PoliticalRegionID));
            tabRegion.AddTabItem("Ownership", new OwnershipDetails(_viewModel.Model));

        }

        void model_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateRegionAction(_viewModel.Model));
        }

        #region Properties

        public RegionViewModel ViewModel {
            get { return _viewModel; }
        }

        public int RegionID { get; set; }

        #endregion
    }

    public class UpdateRegionAction : GenericDatabaseAction<Region> {

        public UpdateRegionAction(Region model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateRegion(Model.PoliticalRegionID, Model.Name, Model.Rank);
        }

    }

    public class RegionViewModel : GenericViewModelBase<Region> {

        public RegionViewModel(Region model)
            : base(model) {
        }

        public int PoliticalRegionID { 
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }
        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Rank {
            get { return Model.Rank; }
            set { SetProperty(() => Model.Rank, value); }
        }

        public string Parentage {
            get { return Model.Parentage; }
            set { SetProperty(() => Model.Parentage, value); }
        }

    }

}
