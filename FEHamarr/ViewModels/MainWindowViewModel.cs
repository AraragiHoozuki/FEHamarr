using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using DynamicData;
using DynamicData.Binding;
using FEHamarr.FEHArchive;
using FEHamarr.SerializedData;
using FEHamarr.Views;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FEHamarr.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ObservableCollection<ObservableCollection<MapGrid>> grid_collection;
        public MainWindowViewModel()
        {
            OpenMapCommand = ReactiveCommand.CreateFromTask(OpenSRPGMap);
            SaveMapCommand = ReactiveCommand.Create(SaveMap);
            GridSelectCommand = ReactiveCommand.Create<MapGrid>(SelectGrid);
            AddUnitCommand = ReactiveCommand.Create(AddUnit);
            DeleteUnitCommand = ReactiveCommand.Create<MapUnit>(DeleteUnit);
            ChangePersonCommand = ReactiveCommand.CreateFromTask<MapUnit>(OpenPersonSelectWindow);
            ChangeSkillCommand = ReactiveCommand.CreateFromTask<MapUnitAndSkillIndex>(OpenSkillSelectWindow);
            ShowPersonSelector = new Interaction<PersonSelectWindowViewModel, PersonViewModel?>();
            ShowSkillSelector = new Interaction<SkillSelectWindowViewModel, SkillViewModel?>();
            grid_collection = new ObservableCollection<ObservableCollection<MapGrid>>();
        }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> OpenMapCommand { get; }
        public ReactiveCommand<MapGrid, System.Reactive.Unit> GridSelectCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddUnitCommand { get; }
        public ReactiveCommand<MapUnit, System.Reactive.Unit> DeleteUnitCommand { get; }
        public ReactiveCommand<MapUnit, System.Reactive.Unit> ChangePersonCommand { get; }
        public ReactiveCommand<MapUnitAndSkillIndex, System.Reactive.Unit> ChangeSkillCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SaveMapCommand { get; }
        public Interaction<PersonSelectWindowViewModel, PersonViewModel?> ShowPersonSelector { get; }
        public Interaction<SkillSelectWindowViewModel, SkillViewModel?> ShowSkillSelector { get; }

        public ObservableCollection<ObservableCollection<MapGrid>> Grids { 
            get => grid_collection; 
            set => grid_collection = value; 
        }

        MapGrid lastSelectedGrid;
        public MapGrid CurrentGrid
        {
            get => lastSelectedGrid;
            set => this.RaiseAndSetIfChanged(ref lastSelectedGrid, value);
        }

        FEHArcSRPGMap _arc;
        SRPGMap _map;
        async Task OpenSRPGMap()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            var dialog = new OpenFileDialog();
            var result = await dialog.ShowAsync(mainWindow);
            if (result != null)
            {
                _arc = new FEHArcSRPGMap(result[0]);
                var map = _map = _arc.GetData();
                grid_collection.Clear();
                MapGrid[][] grids = new MapGrid[map.field.terrain.Length][];
                for (int i = 0; i < map.field.terrain.Length; i++)
                {
                    int view_y = map.field.terrain.Length - i - 1;
                    grids[view_y] = new MapGrid[map.field.terrain[0].Length];
                    for (int j = 0; j < map.field.terrain[0].Length; j++)
                    {
                        grids[view_y][j] = new MapGrid()
                        {
                            Terrain = (TerrainType)map.field.terrain[i][j],
                            Y = (ushort)i,
                            X = (ushort)j,
                        };
                    }
                    grid_collection.Insert(0, new ObservableCollection<MapGrid>(grids[view_y]));
                }


                foreach (var unit in map.units.list)
                {
                    grids[map.field.terrain.Length - 1 - unit.pos.y][unit.pos.x].Units.Add(new MapUnit(unit));
                }
            }
        }
        void SelectGrid(MapGrid grid)
        {
            if (CurrentGrid != null)
            {
                CurrentGrid.IsSelected = false;
            }
            grid.IsSelected = true;
            CurrentGrid = grid;
        }
        void AddUnit()
        {
            var unit = SerializedData.Unit.Create(CurrentGrid.X, CurrentGrid.Y);
            CurrentGrid.AddUnit(unit);
            _map.units.list.Add(unit);
        }

        void DeleteUnit(MapUnit u)
        {
            CurrentGrid.DeleteUnit(u);
            _map.units.list.Remove(u.Unit);
        }
        async Task OpenPersonSelectWindow(MapUnit unit)
        {
            var vm = new PersonSelectWindowViewModel();
            var result = await ShowPersonSelector.Handle(vm);
            if (result != null)
            {
                Person p = result.Person;
                unit.ID = p.id;
                unit.ChangeSkill(0, result.w);
                unit.ChangeSkill(3, result.a);
                unit.ChangeSkill(4, result.b);
                unit.ChangeSkill(5, result.c);
                unit.HP = (ushort)(p.Stat(0) + 10);
                unit.ATK = (ushort)(p.Stat(1) + 4);
                unit.SPD = (ushort)(p.Stat(2) + 4);
                unit.DEF = (ushort)(p.Stat(3) + 4);
                unit.RES = (ushort)(p.Stat(4) + 4);
            }
        }
        async Task OpenSkillSelectWindow(MapUnitAndSkillIndex unit_and)
        {
            var vm = new SkillSelectWindowViewModel(unit_and);
            var result = await ShowSkillSelector.Handle(vm);
            if (result != null)
            {
                unit_and.Unit.ChangeSkill(unit_and.Index, result.Skill.id);
            }
        }

        void SaveMap()
        {
            byte[] buffer;
            for (int i = 0; i < _map.field.terrain.Length; i++)
            {
                for (int j = 0; j < _map.field.terrain[0].Length; j++)
                {
                    _map.field.terrain[i][j] = (byte)grid_collection[_map.field.terrain.Length - i - 1][j].Terrain;
                }
            }
            using (MemoryStream ms = new MemoryStream())
            using (SRPGMapWriter writer = new SRPGMapWriter(ms))
            using (FileStream fs = File.OpenWrite(_arc.FilePath))
            {
                writer.WriteAll(_arc, _map);
                buffer = _arc.XStart.Concat(ms.ToArray()).ToArray();
            }
            File.WriteAllBytes(_arc.FilePath, Cryptor.EncryptAndCompress(buffer));
        }
    }

    public class MapGrid : ReactiveObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ObservableCollection<MapUnit> units = new ObservableCollection<MapUnit>();
        TerrainType terrain;
        ushort x;
        ushort y;

        public void AddUnit(SerializedData.Unit unit) 
        { 
            units.Add(new MapUnit(unit)); 
            if (units.Count()==1) NotifyPropertyChanged(nameof(FirstUnit));
        }

        public bool DeleteUnit(MapUnit unit)
        {
            if (units.Remove(unit)) {
                NotifyPropertyChanged(nameof(FirstUnit));
                return true;
            }
            return false;
        }

        public MapUnit? FirstUnit
        {
            get => units.FirstOrDefault();
        }

        public TerrainType Terrain
        {
            get => terrain; 
            set  {
                terrain = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(TerrainColor));
            }
        }

        public IBrush TerrainColor
        {
            get { 
               switch(Terrain)
                {
                    case TerrainType.Mountain:  case TerrainType.River: case TerrainType.Sea:
                    case TerrainType.Lava:
                    case TerrainType.IndoorWater:
                        return Brushes.Cyan;
                    case TerrainType.Forest:
                    case TerrainType.OutdoorTrench:
                    case TerrainType.IndoorTrench:
                    case TerrainType.OutdoorDefensiveTrench:
                    case TerrainType.IndoorDefensiveTrench:
                        return Brushes.Green;
                    case TerrainType.Inaccessible:
                    case TerrainType.Wall:
                        return Brushes.Black;
                    case TerrainType.OutdoorBreakable:
                    case TerrainType.OutdoorBreakable2: case TerrainType.IndoorBreakable: case TerrainType.IndoorBreakable2:
                    case TerrainType.DesertBreakable:
                    case TerrainType.DesertBreakable2:
                    case TerrainType.BridgeBreakable2:
                    case TerrainType.BridgeBreakable:
                    case TerrainType.PlayerStructure:
                    case TerrainType.EnemyStructure:
                    case TerrainType.OutdoorPlayerCamp: case TerrainType.IndoorPlayerCamp:
                        return Brushes.Brown;
                    
                    case TerrainType.PlayerCamp:
                    case TerrainType.EnemyCamp:
                        return Brushes.DarkSeaGreen;
                    default:
                        return Brushes.Transparent;
                };
            }
        }

        public ushort X
        {
            get => x; set => x = value;
        }

        public ushort Y
        {
            get => y; set => y = value;
        }

        public ObservableCollection<MapUnit> Units
        {
            get { return units; }
        }
        public string Name => FirstUnit == null ? string.Empty  : FirstUnit.Name;

        bool selected = false;
        public bool IsSelected
        {
            get => selected;
            set {
                selected =value;
                NotifyPropertyChanged();
            }
        }
    }

    public class MapUnit : ReactiveObject, INotifyPropertyChanged
    {
        SerializedData.Unit unit;

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MapUnit(SerializedData.Unit u)
        {
            unit = u;
        }

        public SerializedData.Unit Unit => unit;

        public void ChangeSkill(int index, string id)
        {
            unit.skills[index] = id;
            NotifyPropertyChanged(nameof(Weapon));
            NotifyPropertyChanged(nameof(Assist));
            NotifyPropertyChanged(nameof(Special));
            NotifyPropertyChanged(nameof(A));
            NotifyPropertyChanged(nameof(B));
            NotifyPropertyChanged(nameof(C));
            NotifyPropertyChanged(nameof(S));
            NotifyPropertyChanged(nameof(WeaponImage));
            NotifyPropertyChanged(nameof(AssistImage));
            NotifyPropertyChanged(nameof(SpecialImage));
            NotifyPropertyChanged(nameof(AImage));
            NotifyPropertyChanged(nameof(BImage));
            NotifyPropertyChanged(nameof(CImage));
            NotifyPropertyChanged(nameof(SImage));
        }

        public string ID
        {
            get => unit.id_tag;
            set {
                this.RaiseAndSetIfChanged(ref unit.id_tag, value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Name));
                NotifyPropertyChanged(nameof(Title));
                NotifyPropertyChanged(nameof(MoveIcon));
                NotifyPropertyChanged(nameof(WeaponIcon));
            }
        }

        public string Name {
            get => DataManager.GetMessage("M" + unit.id_tag);
        }

        public IBrush NameColor
        {
            get
            {
                return IsEnemy ? Brushes.IndianRed : Brushes.Teal;
            }
        }

        public string Title
        {
            get
            {
                string body = DataManager.StripIdPrefix(unit.id_tag, out string prefix);
                return DataManager.GetMessage("M" + prefix + "HONOR_" + body);
            }
        }
        public IImage WeaponIcon
        {
            get
            {
                var p = DataManager.GetPerson(unit.id_tag);
                return DataManager.GetWeaponIcon((int)p.weapon_type);
            }
        }
        public IImage MoveIcon
        {
            get
            {
                var p = DataManager.GetPerson(unit.id_tag);
                return DataManager.GetMoveIcon((int)p.move_type);
            }
        }

        public Skill Weapon
        {
            get => DataManager.GetSkill(unit.skills[0]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[0], value.id);
            }
        }
        public Skill Assist
        {
            get => DataManager.GetSkill(unit.skills[1]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[1], value.id);
            }
        }
        public Skill Special
        {
            get => DataManager.GetSkill(unit.skills[2]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[2], value.id);
            }
        }
        public Skill A
        {
            get => DataManager.GetSkill(unit.skills[3]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[3], value.id);
            }
        }
        public Skill B
        {
            get => DataManager.GetSkill(unit.skills[4]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[4], value.id);
            }
        }
        public Skill C
        {
            get => DataManager.GetSkill(unit.skills[5]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[5], value.id);
            }
        }
        public Skill S
        {
            get => DataManager.GetSkill(unit.skills[6]);
            set
            {
                this.RaiseAndSetIfChanged(ref unit.skills[6], value.id);
            }
        }

        public IImage WeaponImage => GetSkillImage(0);
        public IImage AssistImage => GetSkillImage(1);
        public IImage SpecialImage => GetSkillImage(2);
        public IImage AImage => GetSkillImage(3);
        public IImage BImage => GetSkillImage(4);
        public IImage CImage => GetSkillImage(5);
        public IImage SImage => GetSkillImage(6);

        private IImage GetSkillImage(int index)
        {
            if (unit.skills[index] == null)
            {
                return DataManager.GetSkillIcon(0);
            }
            else
            {
                Skill s = DataManager.GetSkill(unit.skills[index]);
                return s != null ? DataManager.GetSkillIcon((int)s.icon) : DataManager.GetSkillIcon(0);
            }
        }

        public ushort HP
        {
            get => unit.stats.hp;
            set {
                unit.stats.hp = value;
                NotifyPropertyChanged();
            }
        }
        public ushort ATK
        {
            get => unit.stats.atk;
            set
            {
                unit.stats.atk = value;
                NotifyPropertyChanged();
            }
        }
        public ushort SPD
        {
            get => unit.stats.spd;
            set
            {
                unit.stats.spd = value;
                NotifyPropertyChanged();
            }
        }
        public ushort DEF
        {
            get => unit.stats.def;
            set
            {
                unit.stats.def = value;
                NotifyPropertyChanged();
            }
        }
        public ushort RES
        {
            get => unit.stats.res;
            set
            {
                unit.stats.res = value;
                NotifyPropertyChanged();
            }
        }

        public byte CD
        {
            get => unit.cd;
            set { this.RaiseAndSetIfChanged(ref unit.cd, value); }
        }
        public byte StartTurn
        {
            get => unit.start_turn;
            set { this.RaiseAndSetIfChanged(ref unit.start_turn, value); }
        }
        public byte MoveGroup
        {
            get => unit.movement_group;
            set { this.RaiseAndSetIfChanged(ref unit.movement_group, value); }
        }
        public byte MoveDelay
        {
            get => unit.movement_delay;
            set { this.RaiseAndSetIfChanged(ref unit.movement_delay, value); }
        }
        public byte IsReturning
        {
            get => unit.tether;
            set { this.RaiseAndSetIfChanged(ref unit.tether, value); }
        }

        public bool IsEnemy
        {
            get => unit.is_enemy == 1;
            set
            {
                unit.is_enemy = (byte)(value == true ? 1 : 0);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(NameColor));
            }
        }

        public string SpawnCheck
        {
            get => unit.spawn_check;
            set { this.RaiseAndSetIfChanged(ref unit.spawn_check, value); }
        }
        public byte SpawnCount
        {
            get => unit.spawn_count;
            set { this.RaiseAndSetIfChanged(ref unit.spawn_count, value); }
        }
        public byte SpawnTurns
        {
            get => unit.spawn_turns;
            set { this.RaiseAndSetIfChanged(ref unit.spawn_turns, value); }
        }
        public byte SpawnTargetRemain
        {
            get => unit.spawn_target_remain;
            set { this.RaiseAndSetIfChanged(ref unit.spawn_target_remain, value); }
        }
        public byte SpawnTargetKill
        {
            get => unit.spawn_target_kills;
            set { this.RaiseAndSetIfChanged(ref unit.spawn_target_kills, value); }
        }

        public MapUnitAndSkillIndex UnitAndWeapon => new MapUnitAndSkillIndex(this, 0);
        public MapUnitAndSkillIndex UnitAndAssist => new MapUnitAndSkillIndex(this, 1);
        public MapUnitAndSkillIndex UnitAndSpecial => new MapUnitAndSkillIndex(this, 2);
        public MapUnitAndSkillIndex UnitAndA => new MapUnitAndSkillIndex(this, 3);
        public MapUnitAndSkillIndex UnitAndB => new MapUnitAndSkillIndex(this, 4);
        public MapUnitAndSkillIndex UnitAndC => new MapUnitAndSkillIndex(this, 5);
        public MapUnitAndSkillIndex UnitAndS => new MapUnitAndSkillIndex(this, 6);
    }

    public class MapUnitAndSkillIndex
    {
        public MapUnit Unit { get; }
        public int Index { get; }

        public MapUnitAndSkillIndex(MapUnit unit, int index)
        {
            Unit = unit;
            Index = index;
        }
    }
}