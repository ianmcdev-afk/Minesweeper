using System.Windows;
using System.Windows.Controls;

namespace Minesweeper;

public sealed class NameDialog : Window
{
    readonly TextBox _box;

    public string EnteredName => _box.Text.Trim();

    public NameDialog(string message, string initialName)
    {
        Title = "New high score";
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
        FontSize = 14;

        _box = new TextBox
        {
            Text = initialName,
            MaxLength = 16,
            Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 0, 14)
        };

        var save = new Button
        {
            Content = "Save score",
            IsDefault = true,
            Padding = new Thickness(14, 5, 14, 5),
            Margin = new Thickness(0, 0, 8, 0)
        };
        save.Click += (_, _) => DialogResult = true;

        var skip = new Button
        {
            Content = "Skip",
            IsCancel = true,
            Padding = new Thickness(14, 5, 14, 5)
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(save);
        buttons.Children.Add(skip);

        var panel = new StackPanel { Margin = new Thickness(20), Width = 300 };
        panel.Children.Add(new TextBlock { Text = "New high score", FontSize = 18, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock { Text = message, Margin = new Thickness(0, 4, 0, 14), Foreground = System.Windows.Media.Brushes.DimGray });
        panel.Children.Add(_box);
        panel.Children.Add(buttons);
        Content = panel;

        Loaded += (_, _) =>
        {
            _box.Focus();
            _box.SelectAll();
        };
    }
}
