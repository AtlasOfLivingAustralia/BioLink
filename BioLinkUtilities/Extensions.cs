﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections;

namespace BioLink.Client.Utilities {

    public static class Extensions {

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }

        public static void ForEachIndex<T>(this IEnumerable<T> enumeration, Action<T, int> action) {
            int index = 0;
            foreach (T item in enumeration) {
                action(item, index++);
            }
        }

        public static void BackgroundInvoke(this DispatcherObject control, Action action) {
            control.Dispatcher.Invoke(DispatcherPriority.Background, action);
        }

        public static void InvokeIfRequired(this DispatcherObject control, Action action) {
            if (control.Dispatcher.Thread != Thread.CurrentThread) {
                control.Dispatcher.Invoke(action);
            } else {
                action();
            }
        }

        public static bool IsDesignTime(this Control control) {
            return DesignerProperties.GetIsInDesignMode(control);
        }

        public static string _R(this Control control, string messageKey, params object[] args) {
            string message = null;
            control.InvokeIfRequired(() => {

                try {
                    message = control.FindResource(messageKey) as string;
                } catch (Exception) {
                }

                if (message == null) {
                    Logger.Warn("Failed for find message for key '{0}' - using key instead", messageKey);
                    message = messageKey;
                }

                if (args != null) {
                    message = String.Format(message, args);
                }
            });

            return message;
        }

        public static bool Question(this Control control, string question, string caption, MessageBoxImage icon = MessageBoxImage.Question) {

            MessageBoxResult result = MessageBoxResult.No;
            control.InvokeIfRequired(() => {
                Window parent = Window.GetWindow(control);
                result = MessageBox.Show(parent, question, caption, MessageBoxButton.YesNo, icon);
            });
            return result == MessageBoxResult.Yes;
        }

        public static bool DiscardChangesQuestion(this Control control, string question="You have unsaved changes. Are you sure you wish to discard those changes?") {
            bool discard = false;
            control.InvokeIfRequired(() => {
                Window parent = Window.GetWindow(control);
                var frm = new DiscardChangesWindow();
                frm.Owner = parent;
                frm.label1.Text = question;
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (frm.ShowDialog() == true) {
                    discard = true;
                }
            });
            return discard;
        }

        public static bool ValueOrFalse(this bool? value) {
            return value.GetValueOrDefault(false);
        }

        public static bool IsNumeric(this string value) {
            double result;
            return Double.TryParse(value, out result);
        }

        public static bool IsInteger(this string value) {
            int result;
            return Int32.TryParse(value, out result);
        }

        public static string Truncate(this string value, int length, string suffix = "...") {
            if (value.Length > length) {
                return value.Substring(0, length - suffix.Length) + suffix;
            }
            return value;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
            while (toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static void SetRTF(this RichTextBox control, string fragment) {
            if (string.IsNullOrEmpty(fragment)) {
                control.Document.Blocks.Clear();
            } else {
                control.SelectAll();
                control.Selection.Load(new MemoryStream(UTF8Encoding.Default.GetBytes(fragment)), DataFormats.Rtf);
            }
        }

        public static TabItem AddTabItem(this TabControl tab, string title, UIElement content) {
            TabItem tabItem = new TabItem();
            tabItem.Header = title;
            tabItem.Content = content;
            tab.Items.Add(tabItem);

            if (content is ILazyPopulateControl) {
                var lazyLoadee = content as ILazyPopulateControl;
                if (tab.Items.Count == 1) {
                    lazyLoadee.Populate();
                } else {
                    tabItem.RequestBringIntoView += new RequestBringIntoViewEventHandler((s, e) => {

                        if (!lazyLoadee.IsPopulated) {
                            lazyLoadee.Populate();
                        }
                    });
                }
            }

            return tabItem;
        }

        public static TreeViewItem GetTreeViewItemClicked(this TreeView treeView, FrameworkElement sender) {
            Point p = sender.TranslatePoint(new Point(1, 1), treeView);
            DependencyObject obj = treeView.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TreeViewItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TreeViewItem;
        }

        public static string Repeat(this string stringToRepeat, int repeat) {
            var builder = new StringBuilder(repeat);
            for (int i = 0; i < repeat; i++) {
                builder.Append(stringToRepeat);
            }
            return builder.ToString();
        }

        public static string Join<T>(this T[] items, string delimiter) {            
            if (items.Length == 0) {
                return "";
            }
            StringBuilder sb = new StringBuilder(items[0].ToString());
            for (int i = 1; i < items.Length; ++i) {
                sb.Append(delimiter);
                sb.Append(items[i] == null ? "" : items[i].ToString());
            }
            return sb.ToString();
        }

        public static string Join<T>(this IList<T> items, string delimiter) {
            if (items.Count == 0) {
                return "";
            }
            StringBuilder sb = new StringBuilder(items[0].ToString());
            for (int i = 1; i < items.Count; ++i) {
                sb.Append(delimiter);
                sb.Append(items[i] == null ? "" : items[i].ToString());
            }
            return sb.ToString();
        }


    }

    public delegate string MessageFormatterFunc(string format, params object[] args);

}
