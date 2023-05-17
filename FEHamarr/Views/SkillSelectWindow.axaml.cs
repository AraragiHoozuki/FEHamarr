using Avalonia.Controls;
using Avalonia.ReactiveUI;
using FEHamarr.ViewModels;
using ReactiveUI;
using System;

namespace FEHamarr.Views
{
    public partial class SkillSelectWindow : ReactiveWindow<SkillSelectWindowViewModel>
    {
        public SkillSelectWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ConfirmCommand.Subscribe(Close)));
        }
    }
}
