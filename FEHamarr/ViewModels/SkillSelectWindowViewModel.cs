using Avalonia.Media;
using FEHamarr.SerializedData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEHamarr.ViewModels
{
    public class SkillSelectWindowViewModel : ViewModelBase
    {
        public ObservableCollection<SkillViewModel> FilteredSkills { get; } = new();
        SkillViewModel? _selectedSkill;
        IEnumerable<Skill> _skillPool;
        int _skillIndex;
        public SkillViewModel? SelectedSkill
        {
            get => _selectedSkill;
            set => this.RaiseAndSetIfChanged(ref _selectedSkill, value);
        }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SearchCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, SkillViewModel?> ConfirmCommand { get; }

        public SkillSelectWindowViewModel(MapUnitAndSkillIndex unit_and)
        {
            SearchCommand = ReactiveCommand.Create(DoSearch);
            ConfirmCommand = ReactiveCommand.Create(() => {
                return SelectedSkill;
            });
            _skillIndex = unit_and.Index;
            _skillPool = DataManager.Skills.Select(kv => kv.Value).Where(s=> {
                return (_skillIndex == 6 && (int)s.category >=3 && (int)s.category <=6)||(_skillIndex !=6 && ((int)s.category==_skillIndex) && s.refine_sort_id < 100);
            }).OrderBy(s => s.sort_value).Reverse();
            if (_skillIndex == 0) WeaponFilterEnabled = true;
            if (_skillIndex == 6) SkillTypeFilterEnabled = true;
            DoSearch();

            WeaponTypeIcons = DataManager.WeaponTypeIcons.Values.ToArray();
            SkillTypeItems = Enum.GetNames<SkillCategory>();
        }

        void DoSearch()
        {
            FilteredSkills.Clear();
            foreach (var s in _skillPool)
            {
                if ((!SP240Plus || s.sp_cost >= 240) &&
                    (!SP300Plus || s.sp_cost >= 300) &&
                    (!SP500Plus || s.sp_cost >= 500) &&
                    (_skillIndex==6 || (int)s.category == _skillIndex) &&
                    (Exclusive == false || s.is_exclusive == 1 ) &&
                    (_skillIndex != 0 || Refined == (s.is_refined == 1)) &&
                    (WeaponFilterEnabled==false||_skillIndex!=0 || (s.wep_equip & (1 << WeaponType)) == (1 << WeaponType) ) &&
                    (SkillTypeFilterEnabled == false || SkillType == (int)s.category)
                    )
                {
                    FilteredSkills.Add(new SkillViewModel(s));
                }
            }
        }
        bool _sp240 = true;
        public bool SP240Plus
        {
            get => _sp240;
            set => this.RaiseAndSetIfChanged(ref _sp240, value);
        }

        bool _sp300 = false;
        public bool SP300Plus
        {
            get => _sp300;
            set => this.RaiseAndSetIfChanged(ref _sp300, value);
        }
        bool _sp500 = false;
        public bool SP500Plus
        {
            get => _sp500;
            set => this.RaiseAndSetIfChanged(ref _sp500, value);
        }

        public bool Exclusive { get; set; }
        public bool Refined { get; set; }
        public bool WeaponFilterEnabled { get; set; }
        public int WeaponType { get; set; } = 0;
        public IImage[] WeaponTypeIcons { get; set; }

        public bool SkillTypeFilterEnabled { get; set; } = false;
        public string[] SkillTypeItems { get; set; }

        public int SkillType { get; set; } = 3;
    }

    public class SkillViewModel : ViewModelBase
    {
        Skill _skill;
        public SkillViewModel(Skill s)
        {
            _skill = s;
        }
        public Skill Skill { get => _skill; }

        public IImage Icon => DataManager.GetSkillIcon((int)_skill.icon);
        public string Name => _skill.Name;
        public string Description => _skill.Description;
    }
}
