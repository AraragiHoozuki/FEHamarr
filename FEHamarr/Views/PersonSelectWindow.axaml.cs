using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FEHamarr.SerializedData;
using FEHamarr.ViewModels;
using ReactiveUI;
using System;

namespace FEHamarr.Views
{
    public partial class PersonSelectWindow : ReactiveWindow<PersonSelectWindowViewModel>
    {
        public PersonSelectWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ConfirmCommand.Subscribe(Close)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ComboBox comboBox = this.Find<ComboBox>("VersionSelector");
            comboBox.Items = DataManager.Versions;
        }
    }
}
