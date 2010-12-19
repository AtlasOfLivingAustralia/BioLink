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
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Interaction logic for HierarchicalSelector.xaml
    /// </summary>
    public partial class HierarchicalSelector : ChangeContainer {

        private IHierarchicalSelectorContentProvider _content;

        private ObservableCollection<HierarchicalViewModelBase> _model;

        #region Designer Constructor

        public HierarchicalSelector() {
            InitializeComponent();
        }

        #endregion

        public HierarchicalSelector(User user, IHierarchicalSelectorContentProvider contentProvider, Action<SelectionResult> selectAction) {
            InitializeComponent();
            this.User = user;
            _content = contentProvider;
            if (_content != null) {
                this.Title = _content.Caption;
            }
            this.SelectedAction = selectAction;
            LoadTopLevel();
            Initialized += new EventHandler((s, e) => {
                txtFind.Focus();
            });

            this.ChangeRegistered += new PendingChangedRegisteredHandler(HierarchicalSelector_ChangeRegistered);
            this.ChangesCommitted += new PendingChangesCommittedHandler(HierarchicalSelector_ChangesCommitted);

            if (_content.CanRenameItem || _content.CanDeleteItem || _content.CanAddNewItem) {
                btnApply.Visibility = System.Windows.Visibility.Visible;
                btnApply.IsEnabled = false;
            } else {
                btnApply.Visibility = System.Windows.Visibility.Collapsed;
            }

            tvwExplorer.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(tvw_SelectedItemChanged);
            tvwSearchResults.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(tvw_SelectedItemChanged);

            btnSelect.IsEnabled = false;
        }

        void tvw_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            ValidateSelection();                        
        }

        private void ValidateSelection() {
            HierarchicalViewModelBase selected = null;
            if (tvwExplorer.IsVisible) {
                if (tvwExplorer.SelectedItem != null) {
                    selected = tvwExplorer.SelectedItem as HierarchicalViewModelBase;
                }
            } else if (tvwSearchResults.IsVisible) {
                if (tvwSearchResults.SelectedItem != null) {
                    selected = tvwSearchResults.SelectedItem as HierarchicalViewModelBase;
                }
            }

            btnSelect.IsEnabled = false;
            if (selected != null) {
                btnSelect.IsEnabled = _content.CanSelectItem(selected);
            }

        }

        public List<string> GetExpandedParentages(ObservableCollection<HierarchicalViewModelBase> model) {
            List<string> list = new List<string>();
            ProcessList(model, list);
            return list;
        }

        private string GetParentage(HierarchicalViewModelBase item) {            
            if (item != null) {
                return GetParentage(item.Parent) + "/" + _content.GetElementIDForViewModel(item);
            }
            return "";
        }

        private void ProcessList(ObservableCollection<HierarchicalViewModelBase> model, List<string> list) {
            foreach (HierarchicalViewModelBase vm in model) {
                if (vm.IsExpanded) {                    
                    list.Add(GetParentage(vm));
                    if (vm.Children != null && vm.Children.Count > 0) {
                        ProcessList(vm.Children, list);
                    }
                }
            }
        }

        public void ExpandParentages(ObservableCollection<HierarchicalViewModelBase> model, List<string> expanded) {
            if (expanded != null && expanded.Count > 0) {
                var todo = new Stack<HierarchicalViewModelBase>(model);
                while (todo.Count > 0) {
                    var vm = todo.Pop();
                    string parentage = GetParentage(vm);
                    if (expanded.Contains(parentage)) {
                        vm.IsExpanded = true;
                        expanded.Remove(parentage);
                        vm.Children.ForEach(child => todo.Push(child));
                    }
                }
            }
        }


        void HierarchicalSelector_ChangesCommitted(object sender) {
            btnApply.IsEnabled = false;
            // Save the current expanded hierarchy....
            var expanded = GetExpandedParentages(_model);
            // reload the model...
            LoadTopLevel();
            // expand out the saved expanded hierarchy
            ExpandParentages(_model, expanded);
        }

        void HierarchicalSelector_ChangeRegistered(object sender, object action) {
            btnApply.IsEnabled = true;
        }

        private void LoadTopLevel() {
            _model = LoadModel(null);
            tvwExplorer.ItemsSource = _model;
        }

        private ObservableCollection<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent) {
            var model = new ObservableCollection<HierarchicalViewModelBase>();
            if (_content != null) {
                var list = _content.LoadModel(parent);
                foreach (HierarchicalViewModelBase vm in list) {
                    vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                    vm.LazyLoadChildren += new HierarchicalViewModelAction((p) => {
                        p.Children.Clear();
                        var children = LoadModel(p);
                        foreach (HierarchicalViewModelBase child in children) {
                            p.Children.Add(child);
                        }
                    });
                }
                model = new ObservableCollection<HierarchicalViewModelBase>(list);
            }
            return model;
        }

        private void tvw_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {

            HierarchicalViewModelBase selected = null;

            if (tvwExplorer.IsVisible) {
                selected = tvwExplorer.SelectedItem as HierarchicalViewModelBase;
            } else {
                selected = tvwSearchResults.SelectedItem as HierarchicalViewModelBase;
            }

            if (_content.CanRenameItem && selected != null) {
                var action = _content.RenameItem(selected, text);
                if (action != null) {
                    RegisterUniquePendingChange(action, this);
                }
            }
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }

        private void txtFind_TypingPaused(string text) {

            if (_content == null) {
                return;
            }

            if (string.IsNullOrEmpty(txtFind.Text)) {
                tvwExplorer.Visibility = System.Windows.Visibility.Visible;
                tvwSearchResults.Visibility = System.Windows.Visibility.Collapsed;
            } else {
                tvwExplorer.Visibility = System.Windows.Visibility.Collapsed;
                tvwSearchResults.Visibility = System.Windows.Visibility.Visible;
            }

            if (text.Length > 1) {
                var list = _content.Search(txtFind.Text);
                var model = new ObservableCollection<HierarchicalViewModelBase>(list);
                foreach (HierarchicalViewModelBase vm in list) {
                    vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                    vm.LazyLoadChildren += new HierarchicalViewModelAction((p) => {
                        p.Children.Clear();
                        var children = LoadModel(p);
                        foreach (HierarchicalViewModelBase child in children) {
                            p.Children.Add(child);
                        }
                    });
                }
                tvwSearchResults.ItemsSource = model;
            } else {
                tvwSearchResults.ItemsSource = null;
            }

        }

        private void txtFind_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                if (tvwSearchResults.IsVisible) {
                    
                    if (tvwSearchResults.SelectedItem != null) {
                        TreeViewItem item = tvwSearchResults.ItemContainerGenerator.ContainerFromItem(tvwSearchResults.SelectedItem) as TreeViewItem;
                        item.Focus();
                    }
                } else {
                    tvwExplorer.Focus();
                }
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            HierarchicalViewModelBase selected = null;
            if (tvwExplorer.IsVisible) {
                if (tvwExplorer.SelectedItem != null) {
                    selected = tvwExplorer.SelectedItem as HierarchicalViewModelBase;
                }
            } else if (tvwSearchResults.IsVisible) {
                if (tvwSearchResults.SelectedItem != null) {
                    selected = tvwSearchResults.SelectedItem as HierarchicalViewModelBase;
                }
            }

            if (selected != null && Select(selected)) {
                this.DialogResult = true;
                this.Close();
            }

        }

        private bool Select(HierarchicalViewModelBase selected) {

            if (_content != null) {
                var result = _content.CreateSelectionResult(selected);
                if (SelectedAction != null) {
                    SelectedAction(result);
                    return true;
                }
            }

            return false;
        }

        private void tvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            var tvw = sender as TreeView;

            var item = tvw.SelectedItem as HierarchicalViewModelBase;
            if (item != null) {
                ShowContextMenu(item, tvw);
            }
        }

        private void ShowContextMenu(HierarchicalViewModelBase selected, FrameworkElement control) {
            ContextMenuBuilder builder = new ContextMenuBuilder(null);
            if (_content.CanAddNewItem) {
                builder.New("Add new").Handler(() => { AddNewItem(selected); }).End();
            }

            if (_content.CanRenameItem) {
                builder.Separator();
                builder.New("Rename").Handler(() => { RenameItem(selected); }).End();
            }

            if (_content.CanDeleteItem) {
                builder.Separator();
                builder.New("Delete").Handler(() => { DeleteItem(selected); }).End();
            }

            if (builder.HasItems) {
                control.ContextMenu = builder.ContextMenu;
            }
        }

        private void AddNewItem(HierarchicalViewModelBase parent) {
            var action = _content.AddNewItem(parent);
            if (action != null) {
                RegisterPendingChange(action, this);
            }
        }

        private void RenameItem(HierarchicalViewModelBase item) {
            if (item != null) {
                // This starts the rename, and the change is registered when the edit is complete. See EditableTextBlock_EditingComplete
                item.IsRenaming = true;                
            }
        }

        private void DeleteItem(HierarchicalViewModelBase item) {
            var action = _content.DeleteItem(item);
            if (action != null) {                
                RegisterPendingChange(action, this);
                item.IsDeleted = true;
            }
        }

        #region Properties

        internal Action<SelectionResult> SelectedAction { get; private set; }

        #endregion

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            CommitPendingChanges();
        }

        private void ChangeContainer_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (HasPendingChanges) {
                if (!this.Question("You have unsaved changes. Are you sure you want to discard those changes?", "Discard changes?")) {
                    e.Cancel = true;
                }
            }
        }

    }

    public interface IHierarchicalSelectorContentProvider {

        string Caption { get; }

        bool CanAddNewItem { get; }

        bool CanDeleteItem { get; }

        bool CanRenameItem { get; }

        List<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent);

        List<HierarchicalViewModelBase> Search(string searchTerm);

        bool CanSelectItem(HierarchicalViewModelBase candidate);

        SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem);

        DatabaseAction AddNewItem(HierarchicalViewModelBase selectedItem);

        DatabaseAction RenameItem(HierarchicalViewModelBase selectedItem, string newName);

        DatabaseAction DeleteItem(HierarchicalViewModelBase selectedItem);
        

        int? GetElementIDForViewModel(HierarchicalViewModelBase item);

    }
}
