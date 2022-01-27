using Amplitude.Models;
using Amplitude.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Amplitude.Views
{
    public partial class SoundBoardGridItem : UserControl
    {
        private Panel grd_Control;

        public SoundBoardGridItem()
        {
            InitializeComponent();
            this.grd_Control = this.FindControl<Grid>("grd_Control");
            this.grd_Control.PointerPressed += Control_PointerPressed;
        }

        private void Control_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!e.Handled && e.MouseButton == Avalonia.Input.MouseButton.Left)
            {
                SoundClip? Model = ((SoundBoardGridItemViewModel)DataContext)?.Model;
                if (Model != null && !string.IsNullOrEmpty(Model.Id))
                {
                    Model.PlayAudio();
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
