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
using System.Drawing.Drawing2D;
using SharpMap.Layers;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for VectorOptionsControl.xaml
    /// </summary>
    public partial class VectorOptionsControl : UserControl {

        internal static List<StyleViewModel> _styleModel;

        static VectorOptionsControl() {
            _styleModel = new List<StyleViewModel>();
            _styleModel.Add(new StyleViewModel(null));

            foreach (HatchStyle item in Enum.GetValues(typeof(HatchStyle))) {
                _styleModel.Add(new StyleViewModel(item));
            }

        }


        public VectorOptionsControl(VectorLayerViewModel layer) {
            InitializeComponent();
            DataContext = layer;
            cmbStyle.ItemsSource = _styleModel;
        }

    }

    internal class StyleViewModel : ViewModelBase {

        protected int _iconSize = 15;

        public StyleViewModel(HatchStyle? hatchStyle) {
            this.HatchStyle = hatchStyle;
            this.Icon = MakeHatchIcon(hatchStyle);
        }

        private BitmapSource MakeHatchIcon(HatchStyle? hatchStyle) {

            System.Drawing.Color foreColor = System.Drawing.Color.Black;
            System.Drawing.Color backColor = System.Drawing.Color.White;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(_iconSize, _iconSize);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm)) {

                var rect = new System.Drawing.Rectangle(0, 0, _iconSize - 1, _iconSize - 1);
                if (hatchStyle != null && hatchStyle.HasValue) {
                    g.FillRectangle(new HatchBrush(hatchStyle.Value, foreColor, backColor), rect);
                    g.DrawRectangle(new System.Drawing.Pen(foreColor), rect);
                } else {
                    g.FillRectangle(new System.Drawing.SolidBrush(foreColor), rect);
                }
            }

            return GraphicsUtils.SystemDrawingImageToBitmapSource(bm);
        }

        public HatchStyle? HatchStyle { get; set; }

        public override int? ObjectID {
            get { return null; }
        }
    }

    public class VectorLayerViewModel : ViewModelBase {

        private System.Drawing.Pen _Line;
        private bool _drawOutline;
        private System.Drawing.Image _symbol;
        private System.Drawing.Color _fillColor;
        private HatchStyle? _hatchStyle;

        public VectorLayerViewModel(VectorLayer model) {
            Model = model;
            ConnectionID = model.DataSource.ConnectionID;

            _fillColor = GraphicsUtils.GetColorFromBrush(model.Style.Fill);
            _hatchStyle = GraphicsUtils.GetHatchStyleFromBrush(model.Style.Fill);
            _Line = model.Style.Line;
            _drawOutline = model.Style.EnableOutline;
            _symbol = model.Style.Symbol;
        }

        public string Name {
            get { return Model.LayerName; }
        }

        public System.Drawing.Color FillColor {
            get { return _fillColor; }
            set { SetProperty("FillColor", ref _fillColor, value); }
        }

        public HatchStyle? HatchStyle {
            get { return _hatchStyle; }
            set { SetProperty("HatchStyle", ref _hatchStyle, value); }
        }

        public bool DrawOutline {
            get { return _drawOutline; }
            set { SetProperty("DrawOutline", ref _drawOutline, value); }
        }

        public string ConnectionID { get; private set; }


        internal System.Drawing.Brush FillBrush() {
            return GraphicsUtils.CreateBrush(FillColor, HatchStyle);
        }

        public System.Drawing.Image Symbol {
            get { return _symbol; }
            set { SetProperty("Symbol", ref _symbol, value); }
        }

        public ILayer Model { get; private set; }

        public override int? ObjectID {
            get { return null; }
        }

    }

    public class HatchStyleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var hs = value as HatchStyle?;
            foreach (StyleViewModel vm in VectorOptionsControl._styleModel) {
                if (vm.HatchStyle == hs) {
                    return vm;
                }
            }
            return new StyleViewModel(hs);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var vm = value as StyleViewModel;
            if (vm != null) {
                return vm.HatchStyle;
            }
            return default(HatchStyle);
        }
    }

}
