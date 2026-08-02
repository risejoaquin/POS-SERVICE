using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PosBuilder.Views.Controls
{
    public partial class ColorPickerControl : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor", 
                typeof(string), 
                typeof(ColorPickerControl), 
                new FrameworkPropertyMetadata("#2D5F2E", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public string SelectedColor
        {
            get => (string)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public event EventHandler<string> ColorChanged;

        public ObservableCollection<PaletteColor> Palette { get; set; } = new ObservableCollection<PaletteColor>();
        private bool _isUpdatingFromCode = false;

        public ColorPickerControl()
        {
            InitializeComponent();
            
            // 12 Colores (estilo mexicano/vibrantes)
            string[] hexColors = { 
                "#D4145A", "#FDB913", "#00A859", "#006847", "#00B2E2", "#2B3990", 
                "#F37021", "#9E005D", "#FFC20E", "#ED1C24", "#7B3F00", "#6366F1"
            };

            foreach (var hex in hexColors)
            {
                Palette.Add(new PaletteColor { Hex = hex, Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)) });
            }
            PaletteItems.ItemsSource = Palette;
            UpdateColorUI(SelectedColor);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl control && e.NewValue is string hex)
            {
                control.UpdateColorUI(hex);
            }
        }

        private void HexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode) return;

            string hex = HexInput.Text;
            if (IsValidHex(hex))
            {
                SelectedColor = hex;
                ColorPreview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                ColorChanged?.Invoke(this, hex);
            }
        }

        private void PaletteColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PaletteColor pc)
            {
                SelectedColor = pc.Hex; // Updates UI via DependencyProperty callback
                ColorChanged?.Invoke(this, pc.Hex);
            }
        }

        private void UpdateColorUI(string hex)
        {
            if (IsValidHex(hex))
            {
                _isUpdatingFromCode = true;
                if (HexInput != null && HexInput.Text != hex) 
                    HexInput.Text = hex;
                if (ColorPreview != null) 
                    ColorPreview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                _isUpdatingFromCode = false;
            }
        }

        private bool IsValidHex(string hex)
        {
            return Regex.IsMatch(hex, "^#(?:[0-9a-fA-F]{3}){1,2}$");
        }
    }

    public class PaletteColor
    {
        public string Hex { get; set; }
        public SolidColorBrush Brush { get; set; }
    }
}
