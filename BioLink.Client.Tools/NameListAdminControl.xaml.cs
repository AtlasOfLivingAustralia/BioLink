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
using BioLink.Data.Model;
using System.Collections.ObjectModel;


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for RefLinkTypesControl.xaml
    /// </summary>
    public partial class NameListAdminControl : AdministrationComponent {

        private List<TypeData> _typeData;
        private List<string> _categories;
        private ObservableCollection<TypeDataViewModel> _currentCategoryModel;

        public NameListAdminControl(User user, string nameListType) : base(user) {
            InitializeComponent();
            this.Type = nameListType;
            lstTypeData.MouseRightButtonUp += new MouseButtonEventHandler(lstTypeData_MouseRightButtonUp);
        }

        void lstTypeData_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            if (selected != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                builder.New("Rename").Handler(() => { RenameTraitType(selected); }).End();
                builder.Separator();
                builder.New("Add new {0} type", Type).Handler(() => { AddNewTypeData(); }).End();
                builder.Separator();
                builder.New("Delete {0} type", Type).Handler(() => { DeleteTypeData(selected); }).End();
                lstTypeData.ContextMenu = builder.ContextMenu;
            }

        }

        public override void Populate() {
            _typeData = Service.GetTypeInfo(Type);
            var map = new Dictionary<string, string>();
            foreach (TypeData info in _typeData) {
                if (!map.ContainsKey(info.Category)) {
                    map[info.Category] = info.Category;
                }
            }
            _categories = new List<string>(map.Values);
            cmbCategory.ItemsSource = _categories;
            cmbCategory.SelectedItem = _categories[0];

            IsPopulated = true;
        }

        protected String Type { get; private set; }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string category = cmbCategory.SelectedItem as string;
            if (category != null) {
                var list = _typeData.FindAll((info) => info.Category.Equals(category));
                _currentCategoryModel = new ObservableCollection<TypeDataViewModel>(list.Select((m) => {
                    var vm = new TypeDataViewModel(m);
                    return vm;
                }));
                lstTypeData.ItemsSource = _currentCategoryModel;
            }            

        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewTypeData();
        }

        private void AddNewTypeData() {
            var model = new TypeData();
            model.Category = cmbCategory.SelectedItem as string;
            model.Description = string.Format("<New {0} type>", Type);
            model.ID = -1;
            var viewModel = new TypeDataViewModel(model);
            _currentCategoryModel.Add(viewModel);
            lstTypeData.SelectedItem = viewModel;

            lstTypeData.ScrollIntoView(viewModel);

            viewModel.IsRenaming = true;
            _typeData.Add(model);
            RegisterPendingChange(new InsertTypeDataAction(model, Type));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            DeleteTypeData(selected);
        }

        private void DeleteTypeData(TypeDataViewModel selected) {

            if (selected != null) {
                _currentCategoryModel.Remove(selected);
                _typeData.Remove(selected.Model);
                RegisterPendingChange(new DeleteTypeDataAction(selected.Model, Type));
            }

        }

        private void RenameTraitType(TypeDataViewModel selected) {
            selected.IsRenaming = true;
        }

        private void txtTraitType_EditingComplete(object sender, string text) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            if (selected != null) {
                selected.Description = text;
                if (selected.ID >= 0) {
                    RegisterUniquePendingChange(new UpdateTypeDataAction(selected.Model, Type));
                }
            }
        }



    }
}
