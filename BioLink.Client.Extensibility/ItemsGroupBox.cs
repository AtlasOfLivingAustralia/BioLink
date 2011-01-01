﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ItemsGroupBox : ContentControl {

        private ObservableCollection<ViewModelBase> _items;
        private ViewModelBase _selectedItem;

        static ItemsGroupBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsGroupBox), new FrameworkPropertyMetadata(typeof(ItemsGroupBox)));
            SelectPrevious = new RoutedCommand("SelectPreviousCommand", typeof(ItemsGroupBox));
            SelectNext = new RoutedCommand("SelectNextCommand", typeof(ItemsGroupBox));
            Unlock = new RoutedCommand("UnlockCommand", typeof(ItemsGroupBox));
            AddNew = new RoutedCommand("AddNewCommand", typeof(ItemsGroupBox));    
        }

        public ItemsGroupBox() {            
            this.CommandBindings.Add(new CommandBinding(SelectPrevious, ExecutedSelectedPrevious, CanExecuteSelectPrevious));
            this.CommandBindings.Add(new CommandBinding(SelectNext, ExecutedSelectedNext, CanExecuteSelectNext));
            this.CommandBindings.Add(new CommandBinding(Unlock, ExecutedUnlock, CanExecuteUnlock));
            this.CommandBindings.Add(new CommandBinding(AddNew, ExecutedAddNew, CanExecuteAddNew));

            this.LockIcon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Locked.png");
        }


        protected virtual void OnSelectedItemChanged(ViewModelBase old, ViewModelBase @new) {
            if (SelectedItemChanged != null) {
                SelectedItemChanged(old, @new);
            }
        }

        private void OnNextClicked(RoutedEventArgs e) {

            if (_items != null) {
                if (SelectedIndex < _items.Count - 1) {
                    SelectedItem = _items[SelectedIndex + 1];
                }
            }

            if (NextClicked != null) {
                NextClicked(this, e);
            }
        }

        protected virtual void OnPrevClicked(RoutedEventArgs e) {

            if (_items != null) {
                if (SelectedIndex > 0) {                    
                    SelectedItem = _items[SelectedIndex - 1];
                }
            }

            if (PrevClicked != null) {
                PrevClicked(this, e);
            }
        }

        protected virtual void OnUnlock(RoutedEventArgs e) {
            IsUnlocked = true;
            if (EditClicked != null) {                
                EditClicked(this, e);
            }            

        }

        protected virtual void OnAddNew(RoutedEventArgs e) {
            if (AddNewClicked != null) {
                AddNewClicked(this, e);
            }
        }

        public ObservableCollection<ViewModelBase> Items {
            get { return _items; }
            set {
                _items = value;
                var old = this.SelectedItem;
                if (value != null && value.Count > 0) {                    
                    this.SelectedItem = value[0];
                } else {
                    SelectedItem = null;
                }
            }
        }

        public ViewModelBase SelectedItem {
            get { return _selectedItem; }

            set {
                var oldValue = _selectedItem;

                if (value != null) {
                    SelectedIndex = _items.IndexOf(value);
                    if (SelectedIndex >= 0) {
                        _selectedItem = value;
                        CurrentPosition = string.Format("{0} of {1}", SelectedIndex + 1, _items.Count);
                    } else {
                        _selectedItem = null;
                    }
                } else {
                    _selectedItem = null;
                    SelectedIndex = -1;
                }

                if (_selectedItem != oldValue) {
                    OnSelectedItemChanged(oldValue, _selectedItem);
                }

                if (_selectedItem == null) {
                    CurrentPosition = "???";
                }

                this.DataContext = _selectedItem;
                
            }
        }

        public int SelectedIndex { get; private set; }

        #region Dependency Properties

        public static readonly DependencyProperty EditButtonVisibilityProperty = DependencyProperty.Register("EditButtonVisibility", typeof(Visibility), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(Visibility.Visible,  new PropertyChangedCallback(OnEditVisibilityChanged)));

        public Visibility EditButtonVisibility {
            get { return (Visibility)GetValue(EditButtonVisibilityProperty); }
            set { SetValue(EditButtonVisibilityProperty, value); }
        }

        private static void OnEditVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }

        public static readonly DependencyProperty AddNewButtonVisibilityProperty = DependencyProperty.Register("AddNewButtonVisibility", typeof(Visibility), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnAddNewVisibilityChanged)));

        public Visibility AddNewButtonVisibility {
            get { return (Visibility)GetValue(AddNewButtonVisibilityProperty); }
            set { SetValue(AddNewButtonVisibilityProperty, value); }
        }

        private static void OnAddNewVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }

        public static readonly DependencyProperty CurrentPositionProperty = DependencyProperty.Register("CurrentPosition", typeof(string), typeof(ItemsGroupBox), new FrameworkPropertyMetadata("0 of 0"));

        public string CurrentPosition {
            get { return (string)GetValue(CurrentPositionProperty); }
            set { SetValue(CurrentPositionProperty, value); }
        }

        public static readonly DependencyProperty LockIconProperty = DependencyProperty.Register("LockIcon", typeof(BitmapSource), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(null));

        public BitmapSource LockIcon {
            get { return (BitmapSource)GetValue(LockIconProperty); }
            set { SetValue(LockIconProperty, value); }
        }

        public static readonly DependencyProperty IsUnlockedProperty = DependencyProperty.Register("IsUnlocked", typeof(bool), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IsUnlockedChanged)));

        public bool IsUnlocked {
            get { return (bool)GetValue(IsUnlockedProperty); }
            set { SetValue(IsUnlockedProperty, value); }
        }

        private static void IsUnlockedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var c = obj as ItemsGroupBox;
            if (c != null) {
                if ((bool)args.NewValue) {
                    c.LockIcon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Unlocked.png");
                } else {
                    c.LockIcon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Locked.png");                    
                }
            }
            
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(""));

        public string Header {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderPrefixProperty = DependencyProperty.Register("HeaderPrefix", typeof(string), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(""));

        public string HeaderPrefix {
            get { return (string)GetValue(HeaderPrefixProperty); }
            set { SetValue(HeaderPrefixProperty, value); }
        }

        public static readonly DependencyProperty HeaderFontWeightProperty = DependencyProperty.Register("HeaderFontWeight", typeof(FontWeight), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(FontWeights.Normal));

        public FontWeight HeaderFontWeight {
            get { return (FontWeight)GetValue(HeaderFontWeightProperty); }
            set { SetValue(HeaderFontWeightProperty, value); }
        }

        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush));

        public Brush HeaderForeground {
            get { return (Brush)GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        #endregion

        #region Command Handlers

        public static RoutedCommand SelectPrevious { get; private set; }
        public static RoutedCommand SelectNext { get; private set; }
        public static RoutedCommand Unlock { get; private set; }
        public static RoutedCommand AddNew { get; private set; }

        private void CanExecuteSelectPrevious(object sender, CanExecuteRoutedEventArgs e) {

            ItemsGroupBox target = e.Source as ItemsGroupBox;

            if (target != null) {
                e.CanExecute = target.SelectedIndex > 0;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedSelectedPrevious(object sender, ExecutedRoutedEventArgs e) {
            ItemsGroupBox target = e.Source as ItemsGroupBox;
            if (target != null) {
                target.OnPrevClicked(e);
            }
        }

        private void CanExecuteSelectNext(object sender, CanExecuteRoutedEventArgs e) {

            ItemsGroupBox target = e.Source as ItemsGroupBox;

            if (target != null) {
                e.CanExecute = target.SelectedIndex < target.Items.Count - 1;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedSelectedNext(object sender, ExecutedRoutedEventArgs e) {
            ItemsGroupBox target = e.Source as ItemsGroupBox;
            if (target != null) {
                target.OnNextClicked(e);
            }
        }

        private void CanExecuteUnlock(object sender, CanExecuteRoutedEventArgs e) {

            ItemsGroupBox target = e.Source as ItemsGroupBox;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedUnlock(object sender, ExecutedRoutedEventArgs e) {
            ItemsGroupBox target = e.Source as ItemsGroupBox;
            if (target != null) {
                target.OnUnlock(e);                
            }
        }

        private void CanExecuteAddNew(object sender, CanExecuteRoutedEventArgs e) {

            ItemsGroupBox target = e.Source as ItemsGroupBox;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedAddNew(object sender, ExecutedRoutedEventArgs e) {
            ItemsGroupBox target = e.Source as ItemsGroupBox;
            if (target != null) {
                target.OnAddNew(e);
            }
        }

        #endregion

        #region Events

        public event Action<ViewModelBase, ViewModelBase> SelectedItemChanged;

        public event RoutedEventHandler NextClicked;
        public event RoutedEventHandler PrevClicked;
        public event RoutedEventHandler AddNewClicked;
        public event RoutedEventHandler EditClicked;

        #endregion

    }
}