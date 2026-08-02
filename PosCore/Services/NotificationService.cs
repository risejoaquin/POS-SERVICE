using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PosCore.Services
{
    public class NotificationService
    {
        private static readonly NotificationService _instance = new NotificationService();
        public static NotificationService Instance => _instance;

        private List<NotificationToast> _activeToasts = new List<NotificationToast>();
        private object _lock = new object();

        public void ShowSuccess(string message) => ShowNotification(message, "#10B981", "✅");
        public void ShowError(string message) => ShowNotification(message, "#EF4444", "❌");
        public void ShowWarning(string message) => ShowNotification(message, "#F59E0B", "⚠️");
        public void ShowInfo(string message) => ShowNotification(message, "#3B82F6", "ℹ️");

        private void ShowNotification(string message, string colorHex, string icon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new NotificationToast(message, colorHex, icon);
                toast.Closed += (s, e) => 
                {
                    lock (_lock)
                    {
                        _activeToasts.Remove(toast);
                        UpdatePositions();
                    }
                };

                lock (_lock)
                {
                    _activeToasts.Add(toast);
                    toast.Show();
                    UpdatePositions();
                }
            });
        }

        private void UpdatePositions()
        {
            double currentY = SystemParameters.WorkArea.Height - 10;
            foreach (var toast in _activeToasts.AsEnumerable().Reverse())
            {
                currentY -= (toast.Height);
                toast.Top = currentY;
                toast.Left = SystemParameters.WorkArea.Width - toast.Width - 10;
            }
        }
    }

    public class NotificationToast : Window
    {
        public NotificationToast(string message, string colorHex, string icon)
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            Width = 320;
            Height = 90;
            IsHitTestVisible = false;
            Opacity = 0;

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex),
                BorderThickness = new Thickness(4, 0, 0, 0),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.15,
                    BlurRadius = 10,
                    ShadowDepth = 2
                }
            };

            var grid = new Grid { Margin = new Thickness(15, 10, 15, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconBlock = new TextBlock
            {
                Text = icon,
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };
            Grid.SetColumn(iconBlock, 0);

            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, 1);

            grid.Children.Add(iconBlock);
            grid.Children.Add(textBlock);
            border.Child = grid;
            Content = border;

            Loaded += (s, e) => AnimateIn();
        }

        private void AnimateIn()
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            BeginAnimation(OpacityProperty, fadeIn);
            
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                AnimateOut();
            };
            timer.Start();
        }

        private void AnimateOut()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            fadeOut.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
