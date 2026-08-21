using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Globalization;
using System.Text;
using System.IO;
using Systems;
using Systems.ComponentSystemGroups;
using Systems.GameStateSystems;
using Components;
using AgentBase = Components.Navigation.AgentBase;
using Components.RawComponents;
using Components.SharedContainerSingletons;
using Components.SingletonComponents;
using Components.Structs;
using Microsoft.CodeAnalysis;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utility;
using Utility.EnumsStorage;
using Utility.InterfacesStorage;
using Utility.NativeContainers;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETStandard,Version=v2.0", FrameworkDisplayName = ".NET Standard 2.0")]
[assembly: AssemblyCompany("DnoStatsConfigMod")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("DnoStatsConfigMod")]
[assembly: AssemblyTitle("DnoStatsConfigMod")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace DnoStatsConfigMod
{
	public static class StatsConfigPlugin
	{
		public const string PluginGuid = "local.dno.statsconfig";

		public const string PluginName = "DNO Stats Config Mod";

		public const string PluginVersion = "2.0.0";


		internal static ConfigEntry<bool> Enabled;

		internal static ConfigEntry<bool> ApplyToExistingEntities;

		internal static ConfigEntry<bool> VerboseLogging;

		internal static ConfigEntry<float> BuildingHealthMultiplier;

		internal static ConfigEntry<float> ArmyHealthMultiplier;

		internal static ConfigEntry<float> ArmyDamageMultiplier;

		internal static ConfigEntry<float> ArmySpeedMultiplier;

		internal static ConfigEntry<float> ResearchSpeedMultiplier;

		internal static ConfigEntry<float> UnitTrainingSpeedMultiplier;

		internal static ConfigEntry<float> BuildingConstructionSpeedMultiplier;

		internal static ConfigEntry<float> WorkerWorkSpeedMultiplier;

		internal static ConfigEntry<float> WorkerMovementSpeedMultiplier;

		internal static ConfigEntry<float> UnitAttackRangeMultiplier;

		internal static ConfigEntry<float> UnitVisionRangeMultiplier;

		internal static ConfigEntry<bool> InfiniteIronSources;

		internal static ConfigEntry<bool> InfiniteFishSources;

		internal static ConfigEntry<bool> InfiniteStoneSources;

		internal static ConfigEntry<int> InfiniteResourceAmount;

		internal static ConfigEntry<int> IronSourceStartingAmount;

		internal static ConfigEntry<int> FishSourceStartingAmount;

		internal static ConfigEntry<int> StoneSourceStartingAmount;

		internal static ConfigEntry<int> MinimumFood;

		internal static ConfigEntry<int> MinimumMoney;

		internal static ConfigEntry<int> MinimumWood;

		internal static ConfigEntry<int> MinimumStone;

		internal static ConfigEntry<int> MinimumIron;

		internal static ConfigEntry<int> MinimumSouls;

		internal static ConfigEntry<float> FoodGainMultiplier;

		internal static ConfigEntry<float> MoneyGainMultiplier;

		internal static ConfigEntry<float> WoodGainMultiplier;

		internal static ConfigEntry<float> StoneGainMultiplier;

		internal static ConfigEntry<float> IronGainMultiplier;

		internal static ConfigEntry<float> SoulsGainMultiplier;

		internal static ConfigEntry<float> GranaryCapacityMultiplier;

		internal static ConfigEntry<int> GranaryCapacityOverride;

		internal static ConfigEntry<float> StorageCapacityMultiplier;

		internal static ConfigEntry<int> StorageCapacityOverride;

		internal static ConfigEntry<float> HouseResidentsMultiplier;

		internal static ConfigEntry<int> HouseResidentsOverride;

		internal static ConfigEntry<bool> EnforceHealthEveryTick;

		private static readonly Dictionary<UnitType, TroopConfig> TroopConfigs = new Dictionary<UnitType, TroopConfig>();

		private static bool _initDone;
		public static void Init()
		{
			if (_initDone) return; _initDone = true;
			LoadIni();
			Enabled = Bind<bool>("General", "Enabled", true, "Enable or disable all stat changes.");
			ApplyToExistingEntities = Bind<bool>("General", "ApplyToExistingEntities", true, "Also update entities that already exist when the system runs.");
			VerboseLogging = Bind<bool>("General", "VerboseLogging", false, "Log each newly discovered unit/building stat key.");
			BuildingHealthMultiplier = Bind<float>("Multipliers", "BuildingHealthMultiplier", 1f, "Multiplier for player building max health.");
			ArmyHealthMultiplier = Bind<float>("Multipliers", "ArmyHealthMultiplier", 1f, "Multiplier for player army unit max health.");
			ArmyDamageMultiplier = Bind<float>("Multipliers", "ArmyDamageMultiplier", 1f, "Multiplier for player army unit damage.");
			ArmySpeedMultiplier = Bind<float>("Multipliers", "ArmySpeedMultiplier", 1f, "Multiplier for player army unit movement speed, acceleration, and rotation speed.");
			ResearchSpeedMultiplier = Bind<float>("Speed Multipliers", "ResearchSpeedMultiplier", 1f, "Multiplier for research progress speed.");
			UnitTrainingSpeedMultiplier = Bind<float>("Speed Multipliers", "UnitTrainingSpeedMultiplier", 1f, "Multiplier for player unit training speed.");
			BuildingConstructionSpeedMultiplier = Bind<float>("Speed Multipliers", "BuildingConstructionSpeedMultiplier", 1f, "Multiplier for player building construction speed.");
			WorkerWorkSpeedMultiplier = Bind<float>("Speed Multipliers", "WorkerWorkSpeedMultiplier", 1f, "Multiplier for worker work output.");
			WorkerMovementSpeedMultiplier = Bind<float>("Speed Multipliers", "WorkerMovementSpeedMultiplier", 1f, "Multiplier for worker movement speed, acceleration, and rotation speed.");
			UnitAttackRangeMultiplier = Bind<float>("Range Multipliers", "UnitAttackRangeMultiplier", 1f, "Multiplier for player unit maximum attack range.");
			UnitVisionRangeMultiplier = Bind<float>("Range Multipliers", "UnitVisionRangeMultiplier", 1f, "Multiplier for player unit fog reveal and target-search range.");
			BindTroopConfigs();
			InfiniteIronSources = Bind<bool>("Resources", "InfiniteIronSources", false, "Make iron veins/mines never run out.");
			InfiniteFishSources = Bind<bool>("Resources", "InfiniteFishSources", false, "Make fish food sources never run out. Berry bushes are not affected.");
			InfiniteStoneSources = Bind<bool>("Resources", "InfiniteStoneSources", false, "Make stone sources never run out.");
			InfiniteResourceAmount = Bind<int>("Resources", "InfiniteResourceAmount", 1000000, "Current/max amount assigned to resource sources when an infinite resource option is enabled.");
			IronSourceStartingAmount = Bind<int>("Resources", "IronSourceStartingAmount", 0, "Starting amount for iron sources/mines. Set to 0 to keep the game default.");
			FishSourceStartingAmount = Bind<int>("Resources", "FishSourceStartingAmount", 0, "Starting amount for fisheries. Set to 0 to keep the game default. Berry bushes are not affected.");
			StoneSourceStartingAmount = Bind<int>("Resources", "StoneSourceStartingAmount", 0, "Starting amount for stone sources/mines. Set to 0 to keep the game default.");
			MinimumFood = Bind<int>("Resources", "MinimumFood", 0, "Keep current food at least this high. Set to 0 to disable.");
			MinimumMoney = Bind<int>("Resources", "MinimumMoney", 0, "Keep current money/gold at least this high. Set to 0 to disable.");
			MinimumWood = Bind<int>("Resources", "MinimumWood", 0, "Keep current wood at least this high. Set to 0 to disable.");
			MinimumStone = Bind<int>("Resources", "MinimumStone", 0, "Keep current stone at least this high. Set to 0 to disable.");
			MinimumIron = Bind<int>("Resources", "MinimumIron", 0, "Keep current iron at least this high. Set to 0 to disable.");
			MinimumSouls = Bind<int>("Resources", "MinimumSouls", 0, "Keep current souls at least this high. Set to 0 to disable.");
			FoodGainMultiplier = Bind<float>("Resource Gain Multipliers", "FoodGainMultiplier", 1f, "Multiplier for positive food gains. Does not multiply spending or this mod's minimum refill.");
			MoneyGainMultiplier = Bind<float>("Resource Gain Multipliers", "MoneyGainMultiplier", 1f, "Multiplier for positive money/gold gains. Does not multiply spending or this mod's minimum refill.");
			WoodGainMultiplier = Bind<float>("Resource Gain Multipliers", "WoodGainMultiplier", 1f, "Multiplier for positive wood gains. Does not multiply spending or this mod's minimum refill.");
			StoneGainMultiplier = Bind<float>("Resource Gain Multipliers", "StoneGainMultiplier", 1f, "Multiplier for positive stone gains. Does not multiply spending or this mod's minimum refill.");
			IronGainMultiplier = Bind<float>("Resource Gain Multipliers", "IronGainMultiplier", 1f, "Multiplier for positive iron gains. Does not multiply spending or this mod's minimum refill.");
			SoulsGainMultiplier = Bind<float>("Resource Gain Multipliers", "SoulsGainMultiplier", 1f, "Multiplier for positive souls gains. Does not multiply spending or this mod's minimum refill.");
			GranaryCapacityMultiplier = Bind<float>("Capacities", "GranaryCapacityMultiplier", 1f, "Multiplier for granary food capacity. Ignored when GranaryCapacityOverride is greater than 0.");
			GranaryCapacityOverride = Bind<int>("Capacities", "GranaryCapacityOverride", 0, "Exact granary food capacity. Set to 0 to use GranaryCapacityMultiplier. If greater than 0, this overrides the multiplier.");
			StorageCapacityMultiplier = Bind<float>("Capacities", "StorageCapacityMultiplier", 1f, "Multiplier for regular storage wood/stone/iron capacity. Ignored when StorageCapacityOverride is greater than 0.");
			StorageCapacityOverride = Bind<int>("Capacities", "StorageCapacityOverride", 0, "Exact regular storage wood/stone/iron capacity. Set to 0 to use StorageCapacityMultiplier. If greater than 0, this overrides the multiplier.");
			HouseResidentsMultiplier = Bind<float>("Capacities", "HouseResidentsMultiplier", 1f, "Multiplier for house resident capacity. Ignored when HouseResidentsOverride is greater than 0.");
			HouseResidentsOverride = Bind<int>("Capacities", "HouseResidentsOverride", 0, "Exact house resident capacity. Set to 0 to use HouseResidentsMultiplier. If greater than 0, this overrides the multiplier.");
			EnforceHealthEveryTick = Bind<bool>("Advanced", "EnforceHealthEveryTick", true, "Keep selected/player entities at their configured scaled health if the game refreshes them back to vanilla values.");
			WriteTemplateIfMissing();
			LogInfo("DNO Stats Config Mod 2.0.0 (standalone, no BepInEx) loaded.");
			if (VerboseLogging.Value)
			{
				DnoStatsConfigSystem.RunDiagnosticSelfTests();
			}
		}


		// ---------- standalone config (replaces BepInEx) ----------
		private static readonly Dictionary<string,string> _ini = new Dictionary<string,string>();
		private static readonly List<string[]> _template = new List<string[]>(); // section,key,value,desc
		private static string _iniPath;

		internal static void LogInfo(string m){ DnoLog.Info(m); }
		internal static void LogError(string m){ DnoLog.Error(m); }

		private static string IniPath()
		{
			try { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dno.statsconfig.ini"); }
			catch { return "dno.statsconfig.ini"; }
		}

		private static void LoadIni()
		{
			_ini.Clear(); _template.Clear();
			_iniPath = IniPath();
			try
			{
				if (System.IO.File.Exists(_iniPath))
				{
					string section="";
					foreach (var raw in System.IO.File.ReadAllLines(_iniPath))
					{
						var line = raw.Trim();
						if (line.Length==0 || line.StartsWith("#") || line.StartsWith(";")) continue;
						if (line.StartsWith("[") && line.EndsWith("]")) { section=line.Substring(1,line.Length-2).Trim(); continue; }
						int eq=line.IndexOf('=');
						if (eq<=0) continue;
						string key=line.Substring(0,eq).Trim();
						string val=line.Substring(eq+1).Trim();
						_ini[section+"/"+key]=val;
					}
				}
			}
			catch (Exception e) { DnoLog.Error("config read failed: "+e.Message); }
		}

		private static ConfigEntry<T> Bind<T>(string section, string key, T def, string desc)
		{
			T val = def;
			string raw;
			if (_ini.TryGetValue(section+"/"+key, out raw))
			{
				try
				{
					object o=null;
					if (typeof(T)==typeof(bool)) o = (raw.Trim().ToLowerInvariant()=="true" || raw.Trim()=="1");
					else if (typeof(T)==typeof(int)) o = int.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
					else if (typeof(T)==typeof(float)) o = float.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
					else o = Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
					val=(T)o;
				}
				catch { val=def; }
			}
			_template.Add(new string[]{ section, key, Fmt(val), desc });
			return new ConfigEntry<T>(val);
		}

		private static string Fmt(object v)
		{
			if (v is bool b) return b ? "true" : "false";
			if (v is float f) return f.ToString(CultureInfo.InvariantCulture);
			return Convert.ToString(v, CultureInfo.InvariantCulture);
		}

		internal static void WriteTemplateIfMissing()
		{
			try
			{
				if (_iniPath!=null && !System.IO.File.Exists(_iniPath))
				{
					var sb=new StringBuilder();
					string cur=null;
					foreach (var row in _template)
					{
						if (row[0]!=cur){ if(cur!=null) sb.AppendLine(); sb.AppendLine("["+row[0]+"]"); cur=row[0]; }
						if (!string.IsNullOrEmpty(row[3])) sb.AppendLine("# "+row[3]);
						sb.AppendLine(row[1]+" = "+row[2]);
					}
					System.IO.File.WriteAllText(_iniPath, sb.ToString());
					DnoLog.Info("wrote default config: "+_iniPath);
				}
			}
			catch (Exception e){ DnoLog.Error("config write failed: "+e.Message); }
		}

		internal static float SafeMultiplier(ConfigEntry<float> entry)
		{
			float value = entry.Value;
			if (!(value > 0f))
			{
				return 1f;
			}
			return value;
		}

		internal static void Debug(string message)
		{
			if (VerboseLogging != null && VerboseLogging.Value)
			{
				LogInfo(message);
			}
		}

		internal static UnitMultipliers GetUnitMultipliers(UnitType unitType, float globalHealth, float globalDamage, float globalSpeed)
		{
			UnitMultipliers result;
			if (TroopConfigs.TryGetValue(unitType, out var value))
			{
				result = default(UnitMultipliers);
				result.Health = ((value.Health.Value > 0f) ? value.Health.Value : globalHealth);
				result.Damage = ((value.Damage.Value > 0f) ? value.Damage.Value : globalDamage);
				result.Speed = ((value.Speed.Value > 0f) ? value.Speed.Value : globalSpeed);
				return result;
			}
			result = default(UnitMultipliers);
			result.Health = globalHealth;
			result.Damage = globalDamage;
			result.Speed = globalSpeed;
			return result;
		}

		private static void BindTroopConfigs()
		{
			TroopConfigs.Clear();
			AddTroop((UnitType)1, "Archer");
			AddTroop((UnitType)2, "Footman");
			AddTroop((UnitType)11, "Spearman");
			AddTroop((UnitType)9, "Crossbowman");
			AddTroop((UnitType)10, "Hammer Guy");
			AddTroop((UnitType)31, "Axe Warrior");
			AddTroop((UnitType)8, "Horseman");
			AddTroop((UnitType)29, "Cavalier");
			AddTroop((UnitType)30, "Knight");
			AddTroop((UnitType)32, "Mounted Archer");
			AddTroop((UnitType)95, "Mounted Crossbowman");
			AddTroop((UnitType)3, "Ballista");
			AddTroop((UnitType)96, "Peasant Ballista");
			AddTroop((UnitType)6, "Catapult");
			AddTroop((UnitType)5, "Trebuchet");
			AddTroop((UnitType)7, "Healer");
			AddTroop((UnitType)4, "Banner Bearer");
			AddTroop((UnitType)28, "Torchbearer");
			AddTroop((UnitType)18, "Dark Knight");
			AddTroop((UnitType)88, "Player Undead Kamikaze");
			AddTroop((UnitType)74, "Native Healer");
			AddTroop((UnitType)42, "Summoned Melee");
			AddTroop((UnitType)41, "Summoned Ranged");
		}

		private static void AddTroop(UnitType unitType, string displayName)
		{
			string text = "Troop - " + displayName;
			TroopConfigs[unitType] = new TroopConfig
			{
				Health = Bind<float>(text, "HealthMultiplier", 0f, "Specific max health multiplier for " + displayName + ". Set to 0 to use ArmyHealthMultiplier."),
				Damage = Bind<float>(text, "DamageMultiplier", 0f, "Specific damage multiplier for " + displayName + ". Set to 0 to use ArmyDamageMultiplier."),
				Speed = Bind<float>(text, "SpeedMultiplier", 0f, "Specific movement speed multiplier for " + displayName + ". Set to 0 to use ArmySpeedMultiplier.")
			};
		}
	}
	internal struct UnitMultipliers
	{
		public float Health;

		public float Damage;

		public float Speed;
	}
	internal struct TroopConfig
	{
		public ConfigEntry<float> Health;

		public ConfigEntry<float> Damage;

		public ConfigEntry<float> Speed;
	}
	[UpdateInGroup(typeof(GameplayInitializationSystemsGroup))]
	[UpdateBefore(typeof(GameStateUpdateHandler))]
	public sealed class DnoStatsConfigSystem : SystemBaseSimulation
	{
		private struct UnitOriginals
		{
			public UnitBaseData Unit;

			public HealthBaseData Health;

			public NavigationAgentBaseData Navigation;
		}

		private struct ResourceSourceModifiedRef
		{
			public int Amount;

			public bool GameInfinite;

			public BlobAssetReference<ResourceSourceBaseData> ResourceSourceBase;
		}

		private struct ConstructionOriginal
		{
			public float WorkAmountRequired;

			public float AppliedMultiplier;
		}

		private struct StorageOriginal
		{
			public StorageBaseData Data;

			public CapacityKind Kind;

			public int AppliedCapacity;

			public BlobAssetReference<StorageBaseData> Modified;
		}

		private struct HouseOriginal
		{
			public HouseBaseData Data;

			public int AppliedCapacity;

			public BlobAssetReference<HouseBaseData> Modified;
		}

		private enum CapacityKind
		{
			Granary,
			Storage
		}

		private struct ResourceTracker
		{
			public bool Initialized;

			public int LastAmount;
		}

		private readonly struct BuildingKey : IEquatable<BuildingKey>
		{
			public readonly BuildingType Type;

			public readonly int Level;

			public BuildingKey(BuildingType type, int level)
			{
				Type = type;
				Level = level;
			}

			public bool Equals(BuildingKey other)
			{
				if (Type == other.Type)
				{
					return Level == other.Level;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is BuildingKey other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return ((int)Type * 397) ^ Level;
			}
		}

		private readonly struct EntityKey : IEquatable<EntityKey>
		{
			private readonly int _index;

			private readonly int _version;

			public EntityKey(Entity entity)
			{
				_index = entity.Index;
				_version = entity.Version;
			}

			public bool Equals(EntityKey other)
			{
				if (_index == other._index)
				{
					return _version == other._version;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is EntityKey other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (_index * 397) ^ _version;
			}
		}

		private struct TrainingOriginal
		{
			public float FullTime;

			public float AppliedFullTime;
		}

		private readonly struct TrainingTypeKey : IEquatable<TrainingTypeKey>
		{
			private readonly EntityKey _trainer;

			private readonly int _unitId;

			public TrainingTypeKey(Entity trainer, int unitId)
			{
				_trainer = new EntityKey(trainer);
				_unitId = unitId;
			}

			public bool Equals(TrainingTypeKey other)
			{
				if (_trainer.Equals(other._trainer))
				{
					return _unitId == other._unitId;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is TrainingTypeKey other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (_trainer.GetHashCode() * 397) ^ _unitId;
			}
		}

		private readonly Dictionary<EntityKey, UnitOriginals> _unitOriginals = new Dictionary<EntityKey, UnitOriginals>();

		private readonly Dictionary<BlobAssetReference<UnitBaseData>, UnitBaseData> _unitBaseOriginals = new Dictionary<BlobAssetReference<UnitBaseData>, UnitBaseData>();

		private readonly Dictionary<BlobAssetReference<NavigationAgentBaseData>, NavigationAgentBaseData> _unitNavigationOriginals = new Dictionary<BlobAssetReference<NavigationAgentBaseData>, NavigationAgentBaseData>();

		private readonly Dictionary<BlobAssetReference<NavigationAgentBaseData>, NavigationAgentBaseData> _workerNavigationOriginals = new Dictionary<BlobAssetReference<NavigationAgentBaseData>, NavigationAgentBaseData>();

		private readonly Dictionary<BlobAssetReference<AffectFogBaseData>, AffectFogBaseData> _visionOriginals = new Dictionary<BlobAssetReference<AffectFogBaseData>, AffectFogBaseData>();

		private readonly Dictionary<EntityKey, ConstructionOriginal> _constructionOriginals = new Dictionary<EntityKey, ConstructionOriginal>();

		private readonly Dictionary<TrainingTypeKey, TrainingOriginal> _trainingOriginals = new Dictionary<TrainingTypeKey, TrainingOriginal>();

		private readonly HashSet<EntityKey> _initializedResourceSources = new HashSet<EntityKey>();

		private readonly Dictionary<BuildingKey, HealthBaseData> _buildingOriginals = new Dictionary<BuildingKey, HealthBaseData>();

		private readonly Dictionary<EntityKey, float> _unitHealthApplied = new Dictionary<EntityKey, float>();

		private readonly Dictionary<EntityKey, float> _buildingHealthApplied = new Dictionary<EntityKey, float>();

		private readonly Dictionary<ResourceSourceType, ResourceSourceModifiedRef> _resourceSourceModifiedRefs = new Dictionary<ResourceSourceType, ResourceSourceModifiedRef>();

		private readonly Dictionary<EntityKey, StorageOriginal> _storageOriginals = new Dictionary<EntityKey, StorageOriginal>();

		private readonly Dictionary<EntityKey, HouseOriginal> _houseOriginals = new Dictionary<EntityKey, HouseOriginal>();

		private EntityQuery _unitQuery;

		private EntityQuery _buildingQuery;

		private EntityQuery _resourceSourceQuery;

		private EntityQuery _visionQuery;

		private EntityQuery _workerQuery;

		private EntityQuery _unitTrainingQuery;

		private EntityQuery _constructionQuery;

		private EntityQuery _granaryQuery;

		private EntityQuery _storageQuery;

		private EntityQuery _houseQuery;

		private EntityQuery _timeQuery;

		private ResourceTracker _foodTracker;

		private ResourceTracker _moneyTracker;

		private ResourceTracker _woodTracker;

		private ResourceTracker _stoneTracker;

		private ResourceTracker _ironTracker;

		private ResourceTracker _soulsTracker;

		private bool _workerPerformanceInitialized;

		private float _workerPerformanceOriginal;

		private float _workerPerformanceApplied;

		private bool _researchSpeedInitialized;

		private float _researchSpeedOriginal;

		private float _researchSpeedApplied;

		private float _lastLogTime;

		protected override void OnCreateSimulation()
		{
			StatsConfigPlugin.Init();
			_lastLogTime = float.MinValue;
			EntityQueryDesc[] array = new EntityQueryDesc[1];
			EntityQueryDesc val = new EntityQueryDesc();
			val.All = new ComponentType[1] { ComponentType.ReadOnly<CurrentSessionTimeSingleton>() };
			array[0] = val;
			_timeQuery = GetEntityQuery(array);
			EntityQueryDesc[] array2 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[6]
			{
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadWrite<UnitBase>(),
				ComponentType.ReadWrite<HealthBase>(),
				ComponentType.ReadWrite<Health>(),
				ComponentType.ReadWrite<AgentBase>(),
				ComponentType.ReadOnly<Unit>()
			};
			val.None = new ComponentType[6]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<Dead>(),
				ComponentType.ReadOnly<IsInside>(),
				ComponentType.ReadOnly<GhostCitizen>(),
				ComponentType.ReadOnly<ExplodedInFly>(),
				ComponentType.ReadOnly<DelayedDestroy>()
			};
			array2[0] = val;
			_unitQuery = GetEntityQuery(array2);
			EntityQueryDesc[] array3 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[4]
			{
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadOnly<BuildingBase>(),
				ComponentType.ReadWrite<HealthBase>(),
				ComponentType.ReadWrite<Health>()
			};
			val.None = new ComponentType[3]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<DelayedDestroy>(),
				ComponentType.ReadOnly<BuildingDestroyRequest>()
			};
			array3[0] = val;
			_buildingQuery = GetEntityQuery(array3);
			EntityQueryDesc[] array4 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[2]
			{
				ComponentType.ReadOnly<Unit>(),
				ComponentType.ReadWrite<AffectFogBase>()
			};
			val.None = new ComponentType[3]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<Dead>(),
				ComponentType.ReadOnly<DelayedDestroy>()
			};
			array4[0] = val;
			_visionQuery = GetEntityQuery(array4);
			EntityQueryDesc[] array5 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[2]
			{
				ComponentType.ReadOnly<Worker>(),
				ComponentType.ReadWrite<AgentBase>()
			};
			val.None = new ComponentType[4]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<Dead>(),
				ComponentType.ReadOnly<GhostCitizen>(),
				ComponentType.ReadOnly<DelayedDestroy>()
			};
			array5[0] = val;
			_workerQuery = GetEntityQuery(array5);
			_unitTrainingQuery = GetEntityQuery(new ComponentType[1] { ComponentType.ReadWrite<UnitTrainingProcess>() });
			EntityQueryDesc[] array6 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[2]
			{
				ComponentType.ReadOnly<InConstruction>(),
				ComponentType.ReadWrite<ConstructionWorkCollector>()
			};
			val.None = new ComponentType[2]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<DelayedDestroy>()
			};
			array6[0] = val;
			_constructionQuery = GetEntityQuery(array6);
			EntityQueryDesc[] array7 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[2]
			{
				ComponentType.ReadWrite<ResourceSourceBase>(),
				ComponentType.ReadWrite<ResourceSource>()
			};
			val.None = new ComponentType[1] { ComponentType.ReadOnly<BerryBush>() };
			array7[0] = val;
			_resourceSourceQuery = GetEntityQuery(array7);
			EntityQueryDesc[] array8 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[4]
			{
				ComponentType.ReadOnly<BuildingBase>(),
				ComponentType.ReadOnly<Granary>(),
				ComponentType.ReadWrite<StorageBase>(),
				ComponentType.ReadWrite<FoodStorage>()
			};
			val.None = new ComponentType[3]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<DelayedDestroy>(),
				ComponentType.ReadOnly<BuildingDestroyRequest>()
			};
			array8[0] = val;
			_granaryQuery = GetEntityQuery(array8);
			EntityQueryDesc[] array9 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[5]
			{
				ComponentType.ReadOnly<BuildingBase>(),
				ComponentType.ReadWrite<StorageBase>(),
				ComponentType.ReadWrite<WoodStorage>(),
				ComponentType.ReadWrite<StoneStorage>(),
				ComponentType.ReadWrite<IronStorage>()
			};
			val.None = new ComponentType[3]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<DelayedDestroy>(),
				ComponentType.ReadOnly<BuildingDestroyRequest>()
			};
			array9[0] = val;
			_storageQuery = GetEntityQuery(array9);
			EntityQueryDesc[] array10 = new EntityQueryDesc[1];
			val = new EntityQueryDesc();
			val.All = new ComponentType[3]
			{
				ComponentType.ReadOnly<BuildingBase>(),
				ComponentType.ReadOnly<House>(),
				ComponentType.ReadWrite<HouseBase>()
			};
			val.None = new ComponentType[3]
			{
				ComponentType.ReadOnly<Enemy>(),
				ComponentType.ReadOnly<DelayedDestroy>(),
				ComponentType.ReadOnly<BuildingDestroyRequest>()
			};
			array10[0] = val;
			_houseQuery = GetEntityQuery(array10);
			((ComponentSystemBase)this).RequireSingletonForUpdate<CurrentSessionTimeSingleton>();
			((ComponentSystemBase)this).RequireSingletonForUpdate<GameRunningSingleton>();
		}

		protected override void OnUpdateSimulation()
		{
			if (StatsConfigPlugin.Enabled == null || !StatsConfigPlugin.Enabled.Value || ((ComponentSystemBase)this).HasSingleton<WinLoseSingleton>())
			{
				return;
			}
			float elapsedTime = _timeQuery.GetSingleton<CurrentSessionTimeSingleton>().elapsedTime;
			try
			{
				if (_lastLogTime != float.MinValue && elapsedTime < _lastLogTime)
				{
					ResetSessionCaches();
				}
				int num = ApplyUnitStats();
				int num2 = ApplyUnitVision();
				int num3 = ApplyWorkerOptions();
				int num4 = ApplyUnitTrainingSpeed();
				int num5 = ApplyConstructionSpeed();
				ApplyResearchSpeed();
				int num6 = ApplyBuildingStats();
				int num7 = ApplyResourceSources();
				int num8 = ApplyCurrentResourceOptions();
				int num9 = ApplyCapacityLimits();
				if (elapsedTime < _lastLogTime || elapsedTime - _lastLogTime >= 2f)
				{
					_lastLogTime = elapsedTime;
					StatsConfigPlugin.Debug($"Applied stats to {num} units, {num2} vision affectors, {num3} workers, {num4} training queues, {num5} constructions, {num6} buildings, {num7} resource sources, {num8} current resources, and {num9} capacity entities.");
				}
			}
			catch (Exception arg)
			{
				StatsConfigPlugin.LogError($"DNO stat update failed: {arg}");
			}
		}

		private void ResetSessionCaches()
		{
			_unitOriginals.Clear();
			_unitBaseOriginals.Clear();
			_unitNavigationOriginals.Clear();
			_workerNavigationOriginals.Clear();
			_visionOriginals.Clear();
			_constructionOriginals.Clear();
			_trainingOriginals.Clear();
			_initializedResourceSources.Clear();
			_buildingOriginals.Clear();
			_resourceSourceModifiedRefs.Clear();
			_storageOriginals.Clear();
			_houseOriginals.Clear();
			_unitHealthApplied.Clear();
			_buildingHealthApplied.Clear();
			_foodTracker = default(ResourceTracker);
			_moneyTracker = default(ResourceTracker);
			_woodTracker = default(ResourceTracker);
			_stoneTracker = default(ResourceTracker);
			_ironTracker = default(ResourceTracker);
			_soulsTracker = default(ResourceTracker);
			_workerPerformanceInitialized = false;
			_researchSpeedInitialized = false;
			_lastLogTime = float.MinValue;
			StatsConfigPlugin.Debug("Detected a new session and reset cached vanilla values.");
		}

		private int ApplyUnitStats()
		{
			float globalHealth = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.ArmyHealthMultiplier);
			float globalDamage = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.ArmyDamageMultiplier);
			float globalSpeed = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.ArmySpeedMultiplier);
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.UnitAttackRangeMultiplier);
			float val = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.UnitVisionRangeMultiplier);
			int num2 = 0;
			NativeArray<Entity> val2 = _unitQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<UnitBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<UnitBase>(false);
				ComponentDataFromEntity<HealthBase> componentDataFromEntity2 = ((SystemBase)this).GetComponentDataFromEntity<HealthBase>(false);
				ComponentDataFromEntity<Health> componentDataFromEntity3 = ((SystemBase)this).GetComponentDataFromEntity<Health>(false);
				ComponentDataFromEntity<AgentBase> componentDataFromEntity4 = ((SystemBase)this).GetComponentDataFromEntity<AgentBase>(false);
				var enumerator = val2.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						UnitBase val3 = componentDataFromEntity[current];
						UnitBaseData value = val3.value.Value;
						UnitType type = value.type;
						EntityKey key = new EntityKey(current);
						UnitMultipliers unitMultipliers = StatsConfigPlugin.GetUnitMultipliers(type, globalHealth, globalDamage, globalSpeed);
						if (!_unitBaseOriginals.TryGetValue(val3.value, out var value2))
						{
							value2 = value;
							_unitBaseOriginals.Add(val3.value, value2);
						}
						AgentBase val4 = componentDataFromEntity4[current];
						if (!_unitNavigationOriginals.TryGetValue(val4.value, out var value3))
						{
							value3 = val4.value.Value;
							_unitNavigationOriginals.Add(val4.value, value3);
						}
						if (!_unitOriginals.TryGetValue(key, out var value4))
						{
							UnitOriginals unitOriginals = default(UnitOriginals);
							unitOriginals.Unit = value2;
							unitOriginals.Health = componentDataFromEntity2[current].value.Value;
							unitOriginals.Navigation = value3;
							value4 = unitOriginals;
							_unitOriginals.Add(key, value4);
							StatsConfigPlugin.Debug($"[Unit] {type} entity={current.Index}:{current.Version} damage {value4.Unit.damage}->{value4.Unit.damage * unitMultipliers.Damage}, speed {value4.Navigation.speed}->{value4.Navigation.speed * unitMultipliers.Speed}, attackRange {value4.Unit.maxAttackRange}->{value4.Unit.maxAttackRange * num}, targetSearch {value4.Unit.maxTargetSearchRange}->{value4.Unit.maxTargetSearchRange * Math.Max(num, val)}");
						}
						UnitBaseData unit = value4.Unit;
						unit.damage = value4.Unit.damage * unitMultipliers.Damage;
						unit.maxAttackRange = value4.Unit.maxAttackRange * num;
						unit.maxTargetSearchRange = value4.Unit.maxTargetSearchRange * Math.Max(num, val);
						val3.value.Value = unit;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<UnitBase>(current, val3);
						HealthBase val5 = componentDataFromEntity2[current];
						val5.SetMaxHealthMultiplier((GameWorldLoaderSystem)null, unitMultipliers.Health);
						entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<HealthBase>(current, val5);
						NavigationAgentBaseData navigation = value4.Navigation;
						navigation.speed = value4.Navigation.speed * unitMultipliers.Speed;
						navigation.rotationSpeed = value4.Navigation.rotationSpeed * unitMultipliers.Speed;
						navigation.acceleration = value4.Navigation.acceleration * unitMultipliers.Speed;
						val4.value.Value = navigation;
						entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<AgentBase>(current, val4);
						ScaleCurrentHealth(current, componentDataFromEntity3, _unitHealthApplied, value4.Health.maxHealth, value4.Health.maxHealth * unitMultipliers.Health, unitMultipliers.Health);
						num2++;
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val2).Dispose();
			}
		}

		private int ApplyUnitVision()
		{
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.UnitVisionRangeMultiplier);
			int num2 = 0;
			NativeArray<Entity> val = _visionQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<AffectFogBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<AffectFogBase>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						AffectFogBase val2 = componentDataFromEntity[current];
						if (!_visionOriginals.TryGetValue(val2.value, out var value))
						{
							value = val2.value.Value;
							_visionOriginals.Add(val2.value, value);
							StatsConfigPlugin.Debug($"[Vision] entity={current.Index}:{current.Version} radius {value.affectRadius}->{Math.Max(1, (int)Math.Round((float)value.affectRadius * num))}");
						}
						AffectFogBaseData val3 = value;
						val3.affectRadius = Math.Max(1, (int)Math.Round((float)value.affectRadius * num));
						val2.value.Value = val3;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<AffectFogBase>(current, val2);
						num2++;
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private int ApplyWorkerOptions()
		{
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.WorkerMovementSpeedMultiplier);
			int num2 = 0;
			NativeArray<Entity> val = _workerQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<AgentBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<AgentBase>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						AgentBase val2 = componentDataFromEntity[current];
						if (!_workerNavigationOriginals.TryGetValue(val2.value, out var value))
						{
							value = val2.value.Value;
							_workerNavigationOriginals.Add(val2.value, value);
							StatsConfigPlugin.Debug($"[WorkerMovement] entity={current.Index}:{current.Version} speed {value.speed}->{value.speed * num}, rotation {value.rotationSpeed}->{value.rotationSpeed * num}, acceleration {value.acceleration}->{value.acceleration * num}");
						}
						NavigationAgentBaseData val3 = value;
						val3.speed = value.speed * num;
						val3.rotationSpeed = value.rotationSpeed * num;
						val3.acceleration = value.acceleration * num;
						val2.value.Value = val3;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<AgentBase>(current, val2);
						num2++;
					}
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
			if (((ComponentSystemBase)this).HasSingleton<CitizensLifecycleSingleton>())
			{
				CitizensLifecycleSingleton singleton = ((ComponentSystemBase)this).GetSingleton<CitizensLifecycleSingleton>();
				float workerPerformanceApplied = _workerPerformanceApplied;
				bool flag = false;
				if (!_workerPerformanceInitialized || !NearlyEqual(singleton.workersPerformancePerSecond, _workerPerformanceApplied))
				{
					_workerPerformanceOriginal = singleton.workersPerformancePerSecond;
					_workerPerformanceInitialized = true;
					flag = true;
				}
				float num3 = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.WorkerWorkSpeedMultiplier);
				_workerPerformanceApplied = _workerPerformanceOriginal * num3;
				singleton.workersPerformancePerSecond = _workerPerformanceApplied;
				((ComponentSystemBase)this).SetSingleton<CitizensLifecycleSingleton>(singleton);
				if (flag || !NearlyEqual(workerPerformanceApplied, _workerPerformanceApplied))
				{
					StatsConfigPlugin.Debug($"[WorkerWork] output {_workerPerformanceOriginal}->{_workerPerformanceApplied} (x{num3})");
				}
			}
			return num2;
		}

		private int ApplyUnitTrainingSpeed()
		{
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.UnitTrainingSpeedMultiplier);
			int num2 = 0;
			NativeArray<Entity> val = _unitTrainingQuery.ToEntityArray((Allocator)3);
			try
			{
				BufferFromEntity<UnitTrainingProcess> bufferFromEntity = ((SystemBase)this).GetBufferFromEntity<UnitTrainingProcess>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						DynamicBuffer<UnitTrainingProcess> val2 = bufferFromEntity[current];
						for (int i = 0; i < val2.Length; i++)
						{
							UnitTrainingProcess process = val2[i];
							if (!(process.FullTime <= 0f))
							{
								TrainingTypeKey key = new TrainingTypeKey(current, process.UnitId);
								if (!_trainingOriginals.TryGetValue(key, out var value))
								{
									TrainingOriginal trainingOriginal = default(TrainingOriginal);
									trainingOriginal.FullTime = process.FullTime;
									value = trainingOriginal;
									StatsConfigPlugin.Debug($"[Training] trainer={current.Index}:{current.Version}, unitId={process.UnitId}, fullTime {process.FullTime}->{process.FullTime / num} (x{num} speed)");
								}
								else if (!NearlyEqual(process.FullTime, value.FullTime) && !NearlyEqual(process.FullTime, value.AppliedFullTime))
								{
									value.FullTime = process.FullTime;
								}
								float num3 = value.FullTime / num;
								if (!NearlyEqual(process.FullTime, num3))
								{
									ScaleTrainingProcess(ref process, num3);
									val2[i] = process;
								}
								value.AppliedFullTime = num3;
								_trainingOriginals[key] = value;
								num2++;
							}
						}
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private int ApplyConstructionSpeed()
		{
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.BuildingConstructionSpeedMultiplier);
			int num2 = 0;
			NativeArray<Entity> val = _constructionQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<ConstructionWorkCollector> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<ConstructionWorkCollector>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						ConstructionWorkCollector construction = componentDataFromEntity[current];
						if (!(construction.WorkAmountRequired <= 0f))
						{
							EntityKey key = new EntityKey(current);
							if (!_constructionOriginals.TryGetValue(key, out var value))
							{
								ConstructionOriginal constructionOriginal = default(ConstructionOriginal);
								constructionOriginal.WorkAmountRequired = construction.WorkAmountRequired;
								constructionOriginal.AppliedMultiplier = 1f;
								value = constructionOriginal;
								StatsConfigPlugin.Debug($"[Construction] entity={current.Index}:{current.Version}, requiredWork {construction.WorkAmountRequired}->{construction.WorkAmountRequired / num} (x{num} speed)");
							}
							if (!NearlyEqual(value.AppliedMultiplier, num))
							{
								ScaleConstructionProcess(ref construction, value.WorkAmountRequired / num);
								value.AppliedMultiplier = num;
								_constructionOriginals[key] = value;
								EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
								entityManager.SetComponentData<ConstructionWorkCollector>(current, construction);
							}
							num2++;
						}
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		internal static void RunDiagnosticSelfTests()
		{
			UnitTrainingProcess val = default(UnitTrainingProcess);
			val.FullTime = 10f;
			val.TimeToTrainingEnd = 8f;
			UnitTrainingProcess process = val;
			ScaleTrainingProcess(ref process, 5f);
			bool flag = NearlyEqual(process.FullTime, 5f) && NearlyEqual(process.TimeToTrainingEnd, 4f);
			ConstructionWorkCollector val2 = default(ConstructionWorkCollector);
			val2.WorkAmountRequired = 100f;
			val2.WorkAmountDone = 25f;
			ConstructionWorkCollector construction = val2;
			ScaleConstructionProcess(ref construction, 50f);
			bool flag2 = NearlyEqual(construction.WorkAmountRequired, 50f) && NearlyEqual(construction.WorkAmountDone, 12.5f);
			StatsConfigPlugin.Debug(string.Format("[SelfTest] Training scaling: {0} (10/8 -> {1}/{2})", flag ? "PASS" : "FAIL", process.FullTime, process.TimeToTrainingEnd));
			StatsConfigPlugin.Debug(string.Format("[SelfTest] Construction scaling: {0} (100/25 -> {1}/{2})", flag2 ? "PASS" : "FAIL", construction.WorkAmountRequired, construction.WorkAmountDone));
		}

		private static void ScaleTrainingProcess(ref UnitTrainingProcess process, float requestedFullTime)
		{
			float num = Clamp(1f - process.TimeToTrainingEnd / process.FullTime, 0f, 1f);
			process.FullTime = requestedFullTime;
			process.TimeToTrainingEnd = requestedFullTime * (1f - num);
		}

		private static void ScaleConstructionProcess(ref ConstructionWorkCollector construction, float requestedWork)
		{
			float num = Clamp(construction.WorkAmountDone / construction.WorkAmountRequired, 0f, 1f);
			construction.WorkAmountRequired = requestedWork;
			construction.WorkAmountDone = requestedWork * num;
		}

		private void ApplyResearchSpeed()
		{
			if (((ComponentSystemBase)this).HasSingleton<ResearchContainerSingleton>())
			{
				base.Dependency = SharedNativeContainersUtility.GetWriteDependency<ResearchContainerSingleton>((SystemBase)(object)this, base.Dependency);
				JobHandle dependency = base.Dependency;
				dependency.Complete();
				ResearchContainerSingleton singleton = ComponentSystemBaseManagedComponentExtensions.GetSingleton<ResearchContainerSingleton>((ComponentSystemBase)(object)this);
				float value = ((SharedNativeHandler<ResearchContainer>)(object)singleton).Container.ResearchSpeed.Value;
				float researchSpeedApplied = _researchSpeedApplied;
				bool flag = false;
				if (!_researchSpeedInitialized || !NearlyEqual(value, _researchSpeedApplied))
				{
					_researchSpeedOriginal = value;
					_researchSpeedInitialized = true;
					flag = true;
				}
				_researchSpeedApplied = _researchSpeedOriginal * StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.ResearchSpeedMultiplier);
				((SharedNativeHandler<ResearchContainer>)(object)singleton).Container.ResearchSpeed.Value = _researchSpeedApplied;
				if (flag || !NearlyEqual(researchSpeedApplied, _researchSpeedApplied))
				{
					StatsConfigPlugin.Debug($"[Research] speed {_researchSpeedOriginal}->{_researchSpeedApplied} (x{StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.ResearchSpeedMultiplier)})");
				}
				SharedNativeContainersUtility.AddWriteDependency<ResearchContainerSingleton>((SystemBase)(object)this, base.Dependency);
			}
		}

		private int ApplyBuildingStats()
		{
			float num = StatsConfigPlugin.SafeMultiplier(StatsConfigPlugin.BuildingHealthMultiplier);
			int num2 = 0;
			NativeArray<Entity> val = _buildingQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<BuildingBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<BuildingBase>(true);
				ComponentDataFromEntity<HealthBase> componentDataFromEntity2 = ((SystemBase)this).GetComponentDataFromEntity<HealthBase>(false);
				ComponentDataFromEntity<Health> componentDataFromEntity3 = ((SystemBase)this).GetComponentDataFromEntity<Health>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						BuildingBaseData value = componentDataFromEntity[current].value.Value;
						BuildingKey key = new BuildingKey(value.type, value.level);
						if (!_buildingOriginals.TryGetValue(key, out var value2))
						{
							value2 = componentDataFromEntity2[current].value.Value;
							_buildingOriginals.Add(key, value2);
							StatsConfigPlugin.Debug($"Cached building {key.Type} L{key.Level}: health={value2.maxHealth}");
						}
						HealthBase val2 = componentDataFromEntity2[current];
						val2.SetMaxHealthMultiplier((GameWorldLoaderSystem)null, num);
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<HealthBase>(current, val2);
						ScaleCurrentHealth(current, componentDataFromEntity3, _buildingHealthApplied, value2.maxHealth, value2.maxHealth * num, num);
						num2++;
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private int ApplyResourceSources()
		{
			int num = StatsConfigPlugin.InfiniteResourceAmount.Value;
			if (num < 1)
			{
				num = 1;
			}
			int num2 = 0;
			NativeArray<Entity> val = _resourceSourceQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<ResourceSourceBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<ResourceSourceBase>(false);
				ComponentDataFromEntity<ResourceSource> componentDataFromEntity2 = ((SystemBase)this).GetComponentDataFromEntity<ResourceSource>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						ResourceSourceBase val2 = componentDataFromEntity[current];
						ResourceSourceBaseData value = val2.value.Value;
						EntityKey item = new EntityKey(current);
						bool flag = ShouldMakeInfinite(value.resourceType);
						int num3 = ConfiguredStartingAmount(value.resourceType);
						bool flag2 = !_initializedResourceSources.Contains(item);
						if (!flag && (num3 <= 0 || _initializedResourceSources.Contains(item)))
						{
							continue;
						}
						int num4 = (flag ? num : num3);
						bool flag3 = flag && (int)value.resourceType > 0;
						val2.value = GetOrCreateResourceSourceRef(value, num4, flag3).ResourceSourceBase;
						val2.maxTotalAmount = num4;
						val2.increased = true;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<ResourceSourceBase>(current, val2);
						ResourceSource val3 = componentDataFromEntity2[current];
						if (flag)
						{
							if (val3.currentAmount < num4)
							{
								val3.currentAmount = num4;
							}
						}
						else
						{
							val3.currentAmount = num4;
						}
						entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<ResourceSource>(current, val3);
						entityManager = ((ComponentSystemBase)this).EntityManager;
						if (entityManager.HasComponent<SourceResourceEmpty>(current))
						{
							entityManager = ((ComponentSystemBase)this).EntityManager;
							entityManager.RemoveComponent<SourceResourceEmpty>(current);
						}
						_initializedResourceSources.Add(item);
						if (flag2)
						{
							StatsConfigPlugin.Debug($"[ResourceSource] entity={current.Index}:{current.Version}, type={value.resourceType}, amount={num4}, infinite={flag}, gameInfiniteFlag={flag3}");
						}
						num2++;
					}
					return num2;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private int ApplyCurrentResourceOptions()
		{
			return 0 + ApplyCurrentFoodOptions() + ApplyCurrentMoneyOptions() + ApplyCurrentWoodOptions() + ApplyCurrentStoneOptions() + ApplyCurrentIronOptions() + ApplyCurrentSoulsOptions();
		}

		private int ApplyCurrentFoodOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentFood>())
			{
				return 0;
			}
			CurrentFood current = ((ComponentSystemBase)this).GetSingleton<CurrentFood>();
			bool num = ApplyResourceGainMultiplier<CurrentFood>(ref current, ref _foodTracker, StatsConfigPlugin.FoodGainMultiplier.Value) | ApplyResourceMinimum<CurrentFood>(ref current, StatsConfigPlugin.MinimumFood.Value);
			_foodTracker.LastAmount = current.CurrentAmount();
			_foodTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentFood>(current);
			}
			return num ? 1 : 0;
		}

		private int ApplyCurrentMoneyOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentMoney>())
			{
				return 0;
			}
			CurrentMoney current = ((ComponentSystemBase)this).GetSingleton<CurrentMoney>();
			bool num = ApplyResourceGainMultiplier<CurrentMoney>(ref current, ref _moneyTracker, StatsConfigPlugin.MoneyGainMultiplier.Value) | ApplyResourceMinimum<CurrentMoney>(ref current, StatsConfigPlugin.MinimumMoney.Value);
			_moneyTracker.LastAmount = current.CurrentAmount();
			_moneyTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentMoney>(current);
			}
			return num ? 1 : 0;
		}

		private int ApplyCurrentWoodOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentWood>())
			{
				return 0;
			}
			CurrentWood current = ((ComponentSystemBase)this).GetSingleton<CurrentWood>();
			bool num = ApplyResourceGainMultiplier<CurrentWood>(ref current, ref _woodTracker, StatsConfigPlugin.WoodGainMultiplier.Value) | ApplyResourceMinimum<CurrentWood>(ref current, StatsConfigPlugin.MinimumWood.Value);
			_woodTracker.LastAmount = current.CurrentAmount();
			_woodTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentWood>(current);
			}
			return num ? 1 : 0;
		}

		private int ApplyCurrentStoneOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentStone>())
			{
				return 0;
			}
			CurrentStone current = ((ComponentSystemBase)this).GetSingleton<CurrentStone>();
			bool num = ApplyResourceGainMultiplier<CurrentStone>(ref current, ref _stoneTracker, StatsConfigPlugin.StoneGainMultiplier.Value) | ApplyResourceMinimum<CurrentStone>(ref current, StatsConfigPlugin.MinimumStone.Value);
			_stoneTracker.LastAmount = current.CurrentAmount();
			_stoneTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentStone>(current);
			}
			return num ? 1 : 0;
		}

		private int ApplyCurrentIronOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentIron>())
			{
				return 0;
			}
			CurrentIron current = ((ComponentSystemBase)this).GetSingleton<CurrentIron>();
			bool num = ApplyResourceGainMultiplier<CurrentIron>(ref current, ref _ironTracker, StatsConfigPlugin.IronGainMultiplier.Value) | ApplyResourceMinimum<CurrentIron>(ref current, StatsConfigPlugin.MinimumIron.Value);
			_ironTracker.LastAmount = current.CurrentAmount();
			_ironTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentIron>(current);
			}
			return num ? 1 : 0;
		}

		private int ApplyCurrentSoulsOptions()
		{
			if (!((ComponentSystemBase)this).HasSingleton<CurrentSouls>())
			{
				return 0;
			}
			CurrentSouls current = ((ComponentSystemBase)this).GetSingleton<CurrentSouls>();
			bool num = ApplyResourceGainMultiplier<CurrentSouls>(ref current, ref _soulsTracker, StatsConfigPlugin.SoulsGainMultiplier.Value) | ApplyResourceMinimum<CurrentSouls>(ref current, StatsConfigPlugin.MinimumSouls.Value);
			_soulsTracker.LastAmount = current.CurrentAmount();
			_soulsTracker.Initialized = true;
			if (num)
			{
				((ComponentSystemBase)this).SetSingleton<CurrentSouls>(current);
			}
			return num ? 1 : 0;
		}

		private static bool ApplyResourceGainMultiplier<T>(ref T current, ref ResourceTracker tracker, float multiplier) where T : struct, IUserUIResource
		{
			int num = current.CurrentAmount();
			if (!tracker.Initialized)
			{
				return false;
			}
			if (multiplier <= 1f || num <= tracker.LastAmount)
			{
				return false;
			}
			int num2 = (int)Math.Round((float)(num - tracker.LastAmount) * (multiplier - 1f));
			if (num2 <= 0)
			{
				return false;
			}
			object boxed = current;
			((IUserUIResource)boxed).IncreaseAmount(num2);
			current = (T)boxed;
			return true;
		}

		private static bool ApplyResourceMinimum<T>(ref T current, int minimum) where T : struct, IUserUIResource
		{
			if (minimum <= 0)
			{
				return false;
			}
			int num = current.CurrentAmount();
			if (num >= minimum)
			{
				return false;
			}
			object boxed = current;
			((IUserUIResource)boxed).IncreaseAmount(minimum - num);
			current = (T)boxed;
			return true;
		}

		private int ApplyCapacityLimits()
		{
			return 0 + ApplyStorageCapacity(_granaryQuery, CapacityKind.Granary) + ApplyStorageCapacity(_storageQuery, CapacityKind.Storage) + ApplyHouseCapacity();
		}

		private int ApplyStorageCapacity(EntityQuery query, CapacityKind kind)
		{
			int num = 0;
			NativeArray<Entity> val = query.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<StorageBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<StorageBase>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						EntityKey key = new EntityKey(current);
						StorageBase val2 = componentDataFromEntity[current];
						if (!_storageOriginals.TryGetValue(key, out var value))
						{
							StorageOriginal storageOriginal = default(StorageOriginal);
							storageOriginal.Data = val2.value.Value;
							storageOriginal.Kind = kind;
							storageOriginal.AppliedCapacity = -1;
							value = storageOriginal;
							_storageOriginals.Add(key, value);
							StatsConfigPlugin.Debug($"Cached {kind} capacity: food={value.Data.foodCapacity}, wsi={value.Data.woodStoneIronCapacity}");
						}
						StorageBaseData data = value.Data;
						int num2 = ((kind == CapacityKind.Granary) ? ConfiguredCapacity(value.Data.foodCapacity, StatsConfigPlugin.GranaryCapacityMultiplier, StatsConfigPlugin.GranaryCapacityOverride) : ConfiguredCapacity(value.Data.woodStoneIronCapacity, StatsConfigPlugin.StorageCapacityMultiplier, StatsConfigPlugin.StorageCapacityOverride));
						if (kind == CapacityKind.Granary)
						{
							data.foodCapacity = num2;
						}
						else
						{
							data.woodStoneIronCapacity = num2;
						}
						if (value.AppliedCapacity != num2)
						{
							value.AppliedCapacity = num2;
							value.Modified = CreateBlob<StorageBaseData>(data);
							_storageOriginals[key] = value;
						}
						val2.value = value.Modified;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<StorageBase>(current, val2);
						entityManager = ((ComponentSystemBase)this).EntityManager;
						if (entityManager.HasComponent<IsFullStorage>(current))
						{
							entityManager = ((ComponentSystemBase)this).EntityManager;
							entityManager.RemoveComponent<IsFullStorage>(current);
						}
						num++;
					}
					return num;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private int ApplyHouseCapacity()
		{
			int num = 0;
			NativeArray<Entity> val = _houseQuery.ToEntityArray((Allocator)3);
			try
			{
				ComponentDataFromEntity<HouseBase> componentDataFromEntity = ((SystemBase)this).GetComponentDataFromEntity<HouseBase>(false);
				var enumerator = val.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Entity current = enumerator.Current;
						EntityKey key = new EntityKey(current);
						HouseBase val2 = componentDataFromEntity[current];
						if (!_houseOriginals.TryGetValue(key, out var value))
						{
							HouseOriginal houseOriginal = default(HouseOriginal);
							houseOriginal.Data = val2.value.Value;
							houseOriginal.AppliedCapacity = -1;
							value = houseOriginal;
							_houseOriginals.Add(key, value);
							StatsConfigPlugin.Debug($"Cached house residents: peopleCapacity={value.Data.peopleCapacity}, bornCoefficient={value.Data.bornCoefficient}");
						}
						HouseBaseData data = value.Data;
						int num2 = (data.peopleCapacity = ConfiguredCapacity(value.Data.peopleCapacity, StatsConfigPlugin.HouseResidentsMultiplier, StatsConfigPlugin.HouseResidentsOverride));
						if (value.AppliedCapacity != num2)
						{
							value.AppliedCapacity = num2;
							value.Modified = CreateBlob<HouseBaseData>(data);
							_houseOriginals[key] = value;
						}
						val2.value = value.Modified;
						EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
						entityManager.SetComponentData<HouseBase>(current, val2);
						num++;
					}
					return num;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			finally
			{
				((IDisposable)val).Dispose();
			}
		}

		private static int ConfiguredCapacity(int originalCapacity, ConfigEntry<float> multiplierEntry, ConfigEntry<int> overrideEntry)
		{
			if (overrideEntry != null && overrideEntry.Value > 0)
			{
				return overrideEntry.Value;
			}
			float num = StatsConfigPlugin.SafeMultiplier(multiplierEntry);
			int num2 = (int)Math.Round((float)originalCapacity * num);
			if (num2 <= 0)
			{
				return originalCapacity;
			}
			return num2;
		}

		private static bool ShouldMakeInfinite(ResourceSourceType type)
		{
			return (int)type switch
			{
				2 => StatsConfigPlugin.InfiniteIronSources.Value, 
				1 => StatsConfigPlugin.InfiniteStoneSources.Value, 
				0 => StatsConfigPlugin.InfiniteFishSources.Value, 
				_ => false, 
			};
		}

		private static int ConfiguredStartingAmount(ResourceSourceType type)
		{
			return (int)type switch
			{
				2 => Math.Max(0, StatsConfigPlugin.IronSourceStartingAmount.Value), 
				1 => Math.Max(0, StatsConfigPlugin.StoneSourceStartingAmount.Value), 
				0 => Math.Max(0, StatsConfigPlugin.FishSourceStartingAmount.Value), 
				_ => 0, 
			};
		}

		private ResourceSourceModifiedRef GetOrCreateResourceSourceRef(ResourceSourceBaseData original, int amount, bool gameInfinite)
		{
			if (_resourceSourceModifiedRefs.TryGetValue(original.resourceType, out var value) && value.Amount == amount && value.GameInfinite == gameInfinite)
			{
				return value;
			}
			ResourceSourceBaseData value2 = original;
			value2.infinite = gameInfinite;
			value2.initialAmount = amount;
			ResourceSourceModifiedRef resourceSourceModifiedRef = default(ResourceSourceModifiedRef);
			resourceSourceModifiedRef.Amount = amount;
			resourceSourceModifiedRef.GameInfinite = gameInfinite;
			resourceSourceModifiedRef.ResourceSourceBase = CreateBlob<ResourceSourceBaseData>(value2);
			value = resourceSourceModifiedRef;
			_resourceSourceModifiedRefs[original.resourceType] = value;
			StatsConfigPlugin.Debug($"Prepared resource source {original.resourceType}: amount={amount}, gameInfinite={gameInfinite}");
			return value;
		}

		private static BlobAssetReference<T> CreateBlob<T>(T value) where T : struct
		{
			BlobBuilder val = default(BlobBuilder);
			val = new BlobBuilder((Allocator)2, 65536);
			try
			{
				val.ConstructRoot<T>() = value;
				return val.CreateBlobAssetReference<T>((Allocator)4);
			}
			finally
			{
				val.Dispose();
			}
		}

		private void ScaleCurrentHealth(Entity entity, ComponentDataFromEntity<Health> healthData, Dictionary<EntityKey, float> applied, float originalMaxHealth, float newMaxHealth, float multiplier)
		{
			if (!StatsConfigPlugin.ApplyToExistingEntities.Value || originalMaxHealth <= 0f)
			{
				return;
			}
			EntityKey key = new EntityKey(entity);
			if (!applied.TryGetValue(key, out var value) || !(Math.Abs(value - multiplier) < 0.0001f) || (StatsConfigPlugin.EnforceHealthEveryTick.Value && !(healthData[entity].currentHealth > originalMaxHealth + 0.01f)))
			{
				Health val = healthData[entity];
				float num = ((value > 0f) ? (originalMaxHealth * value) : originalMaxHealth);
				float num2 = ((num > 0f) ? (val.currentHealth / num) : 1f);
				float num3 = Clamp(newMaxHealth * num2, 1f, newMaxHealth);
				if (StatsConfigPlugin.EnforceHealthEveryTick.Value && multiplier > 1f && num3 < newMaxHealth)
				{
					num3 = newMaxHealth;
				}
				val.currentHealth = num3;
				EntityManager entityManager = ((ComponentSystemBase)this).EntityManager;
				entityManager.SetComponentData<Health>(entity, val);
				applied[key] = multiplier;
			}
		}

		private static float Clamp(float value, float min, float max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		private static bool NearlyEqual(float left, float right)
		{
			return Math.Abs(left - right) < 0.0001f;
		}
	}
}
