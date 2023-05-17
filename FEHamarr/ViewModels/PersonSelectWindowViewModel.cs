using Avalonia.Media;
using DynamicData.Binding;
using FEHamarr.SerializedData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEHamarr.ViewModels
{
    public class PersonSelectWindowViewModel : ViewModelBase
    {
        public ObservableCollection<PersonViewModel> FilteredPersons { get; } = new();
        PersonViewModel? _selectedPerson;
        IEnumerable<Person> _personPool;
        public PersonViewModel? SelectedPerson { 
            get => _selectedPerson; 
            set => this.RaiseAndSetIfChanged(ref _selectedPerson, value);
        }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SearchCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, PersonViewModel?> ConfirmCommand { get; }

        public PersonSelectWindowViewModel()
        {
            SearchCommand = ReactiveCommand.Create(DoSearch);
            ConfirmCommand = ReactiveCommand.Create(() => {
                return SelectedPerson;
            });

            _personPool = DataManager.Persons.Select(kv => kv.Value).OrderBy(p=>p.version_num).Reverse();
            foreach (var p in _personPool)
            {
                FilteredPersons.Add(new PersonViewModel(p));
            }


            _moveTypeIcons = DataManager.MoveTypeIcons.Values.ToArray();
            _weaponTypeIcons = DataManager.WeaponTypeIcons.Values.ToArray();
        }

        void DoSearch()
        {
            FilteredPersons.Clear();
            foreach (var p in _personPool)
            {
                if ((!IsMoveTypeSelectorEnabled || p.move_type == (MoveType)MoveType) && 
                    (!IsWeaponTypeSelectorEnabled || p.weapon_type == (WeaponType)WeaponType) &&
                    (!IsVersionSelectorEnabled || p.version_num == Version)) FilteredPersons.Add(new PersonViewModel(p));
            }
        }

        public bool IsVersionSelectorEnabled { get; set; } = false;
        uint _version = 0;
        public uint Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }
        public bool IsMoveTypeSelectorEnabled { get; set; } = false;
        int _moveType = -1;
        public int MoveType
        {
            get => _moveType;
            set => this.RaiseAndSetIfChanged(ref _moveType, value);
        }
        public bool IsWeaponTypeSelectorEnabled { get; set; } = false;
        int _weaponType = -1;
        public int WeaponType
        {
            get => _weaponType;
            set => this.RaiseAndSetIfChanged(ref _weaponType, value);
        }
        IImage[] _moveTypeIcons;
        IImage[] _weaponTypeIcons;
        public IImage[] MoveTypeIcons => _moveTypeIcons;
        public IImage[] WeaponTypeIcons => _weaponTypeIcons;
    }

    public class PersonViewModel : ViewModelBase {
        readonly Person _person;
        List<IImage> _skillIcons = new List<IImage>();

        public PersonViewModel(Person person)
        {
            _person = person;
            if (!(person is Enemy)) {
                List<string> list = _person.skills[3].Concat(_person.skills[4]).Reverse().ToList();
                w = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.Weapon));
                a = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.A));
                if (a != null) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(a).icon));
                b = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.B));
                if (b != null) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(b).icon));
                c = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.C));
                if (c != null) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(c).icon));
            } else
            {
                w = ((Enemy)person).top_weapon;
            }

        }
        public Person Person => _person;
        public string? w;
        public string? a;
        public string? b;
        public string? c;
        public string Name => DataManager.GetMessage("M" + _person.id);
        public string Title
        {
            get
            {
                string body = DataManager.StripIdPrefix(_person.id, out string prefix);
                return DataManager.GetMessage("M" + prefix + "HONOR_" + body);
            }
        }
        public IImage MoveIcon => DataManager.GetMoveIcon((int)_person.move_type);
        public IImage AtkIcon => DataManager.GetWeaponIcon((int)_person.weapon_type);
        public uint Version => _person.version_num;

        public List<IImage> SkillIcons => _skillIcons;
    }
}
