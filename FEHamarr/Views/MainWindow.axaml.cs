using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DynamicData;
using FEHamarr.FEHArchive;
using FEHamarr.SerializedData;
using FEHamarr.ViewModels;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace FEHamarr.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowSkillSelector.RegisterHandler(DoSkillSelectAsync)));
            this.WhenActivated(d => d(ViewModel!.ShowPersonSelector.RegisterHandler(DoPersonSelectAsync)));
            DataManager.Init();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ComboBox comboBox = this.Find<ComboBox>("TerrainTypeSelector");
            comboBox.Items = Enum.GetNames(typeof(TerrainType));
        }

        private async Task DoPersonSelectAsync(InteractionContext<PersonSelectWindowViewModel, PersonViewModel?> interaction)
        {
            var dialog = new PersonSelectWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<PersonViewModel?>(this);
            interaction.SetOutput(result);
        }
        private async Task DoSkillSelectAsync(InteractionContext<SkillSelectWindowViewModel, SkillViewModel?> interaction)
        {
            var dialog = new SkillSelectWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<SkillViewModel?>(this);
            interaction.SetOutput(result);
        }
    }
}