using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record CVAircraftUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public int NumberInSquad { get; set; }

        public int NumberDuringAttack { get; set; }

        public int AmmoPerAttack { get; set; }

        public int StartingNumberOnDeck { get; set; }

        public int MaxNumberOnDeck { get; set; }

        [DataUiUnit("S")]
        public decimal RestorationTime { get; set; }

        [DataUiUnit("HP")]
        public int PlaneHP { get; set; }

        [DataUiUnit("Knots")]
        public decimal CruisingSpeed { get; set; }

        [DataUiUnit("Knots")]
        public decimal MaxSpeed { get; set; }

        [DataUiUnit("Knots")]
        public decimal MinSpeed { get; set; }

        [DataUiUnit("S")]
        public decimal BoostDurationTime { get; set; }

        [DataUiUnit("S")]
        public decimal BoostReloadTime { get; set; }

        [DataUiUnit("S")]
        public decimal ArmamentReloadTime { get; set; }

        [DataUiUnit("PerCent")]
        public int InnerBombPercentage { get; set; }

        [DataUiUnit("S")]
        public decimal AttackCd { get; set; }

        [DataUiUnit("S")]
        public decimal JatoDuration { get; set; }

        public decimal JatoSpeedMultiplier { get; set; }

        [JsonIgnore]
        public string WeaponType { get; set; } = default!;

        [JsonIgnore]
        public bool IsLast { get; set; } = false;

        [JsonIgnore]
        public ProjectileUI Weapon { get; set; } = default!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> CVAircraftData { get; set; } = default!;

        public static List<CVAircraftUI>? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            if (ship.CvPlanes is null)
            {
                return null;
            }

            var list = new List<CVAircraftUI>();
            var planes = new List<PlaneData>();

            ShipUpgrade? rocketConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Fighter);
            if (rocketConfiguration != null)
            {
                var skipModule = ship.CvPlanes[rocketConfiguration.Components[ComponentType.Fighter].First()];
                planes.Add(skipModule);
            }

            ShipUpgrade? torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.TorpedoBomber);
            if (torpConfiguration != null)
            {
                var skipModule = ship.CvPlanes[torpConfiguration.Components[ComponentType.TorpedoBomber].First()];
                planes.Add(skipModule);
            }

            ShipUpgrade? diveConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.DiveBomber);
            if (diveConfiguration != null)
            {
                var diveModule = ship.CvPlanes[diveConfiguration.Components[ComponentType.DiveBomber].First()];
                planes.Add(diveModule);
            }

            ShipUpgrade? skipConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.SkipBomber);
            if (skipConfiguration != null)
            {
                var skipModule = ship.CvPlanes[skipConfiguration.Components[ComponentType.SkipBomber].First()];
                planes.Add(skipModule);
            }

            foreach (var value in planes)
            {
                var index = value.PlaneName.IndexOf("_");
                var name = value.PlaneName.Substring(0, index);
                var plane = AppData.AircraftList![name];
                var planeUI = ProcessCVPlane(plane, value.PlaneType, ship.Tier, modifiers);
                list.Add(planeUI);
            }

            list.Last().IsLast = true;
            return list;
        }

        private static CVAircraftUI ProcessCVPlane(Aircraft plane, PlaneType type, int shipTier, List<(string name, float value)> modifiers)
        {
            var maxOnDeckModifiers = modifiers.FindModifiers("planeExtraHangarSize");
            int maxOnDeck = maxOnDeckModifiers.Aggregate(plane.MaxPlaneInHangar, (current, modifier) => (int)(current + modifier));

            var restorationTimeModifiers = modifiers.FindModifiers("planeSpawnTime");
            decimal restorationTime = Math.Round((decimal)restorationTimeModifiers.Aggregate(plane.RestorationTime, (current, modifier) => current * modifier), 2);

            float planeHP = 0;
            float cruisingSpeed = plane.Speed;
            float minSpeedMultiplier = plane.SpeedMinModifier;
            float maxSpeedMultiplier = plane.SpeedMaxModifier;
            switch (type)
            {
                case PlaneType.Fighter:
                    var rocketPlaneHPModifiers = modifiers.FindModifiers("fighterHealth");
                    planeHP = (float)Math.Round(rocketPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.DiveBomber:
                    var divePlaneHPModifiers = modifiers.FindModifiers("diveBomberHealth");
                    planeHP = (float)Math.Round(divePlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var divePlaneSpeedModifiers = modifiers.FindModifiers("diveBomberSpeedMultiplier");
                    cruisingSpeed = (float)Math.Round(divePlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

                    var minSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMinSpeedMultiplier");
                    minSpeedMultiplier = (float)Math.Round(minSpeedMultiplierModifiers.Aggregate(minSpeedMultiplier, (current, modifier) => current * modifier), 0);

                    var maxSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMaxSpeedMultiplier");
                    maxSpeedMultiplier = (float)Math.Round(maxSpeedMultiplierModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.TorpedoBomber:
                    var torpPlaneHPModifiers = modifiers.FindModifiers("torpedoBomberHealth");
                    planeHP = (float)Math.Round(torpPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.SkipBomber:
                    var skipPlaneHPModifiers = modifiers.FindModifiers("skipBomberHealth");
                    planeHP = (float)Math.Round(skipPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var skipPlaneSpeedModifiers = modifiers.FindModifiers("skipBomberSpeedMultiplier");
                    cruisingSpeed = (float)Math.Round(skipPlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

                    break;
            }

            var allPlaneHpModifiers = modifiers.FindModifiers("planeHealth");
            int finalplaneHP = (int)Math.Round(allPlaneHpModifiers.Aggregate(planeHP, (current, modifier) => current * modifier), 2);

            var planeHpPerTierIndex = modifiers.FindModifierIndex("planeHealthPerLevel");
            if (planeHpPerTierIndex > 0)
            {
                var additionalHP = (int)modifiers[planeHpPerTierIndex].value * shipTier;
                finalplaneHP += additionalHP;
            }

            var cruisingSpeedModifiers = modifiers.FindModifiers("planeSpeed");
            decimal finalCruisingSpeed = Math.Round((decimal)restorationTimeModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

            var jatoDuration = (decimal)plane.JatoData.JatoDuration;
            var jatoMultiplier = (decimal)plane.JatoData.JatoSpeedMultiplier;
            if (jatoDuration == 0)
            {
                jatoMultiplier = 0;
            }

            var weaponType = AppData.ProjectileList![plane.BombName].ProjectileType;
            ProjectileUI weapon = null!;
            switch (weaponType)
            {
                case ProjectileType.Bomb:
                    weapon = BombUI.FromBombName(plane.BombName, modifiers);
                    break;
                case ProjectileType.SkipBomb:
                    break;
                case ProjectileType.Torpedo:
                    var torpList = new List<string>();
                    torpList.Add(plane.BombName);
                    weapon = TorpedoUI.FromTorpedoName(torpList, modifiers).First();
                    break;
                case ProjectileType.Rocket:
                    weapon = RocketUI.FromRocketName(plane.BombName, modifiers);
                    break;
            }

            var cvAircraft = new CVAircraftUI
            {
                Name = plane.Name,
                PlaneHP = finalplaneHP,
                NumberInSquad = plane.NumPlanesInSquadron,
                StartingNumberOnDeck = plane.StartingPlanes,
                MaxNumberOnDeck = maxOnDeck,
                RestorationTime = restorationTime,
                CruisingSpeed = finalCruisingSpeed,
                MaxSpeed = finalCruisingSpeed * (decimal)maxSpeedMultiplier,
                MinSpeed = finalCruisingSpeed * (decimal)minSpeedMultiplier,
                InnerBombPercentage = (int)plane.InnerBombsPercentage,
                NumberDuringAttack = plane.AttackData.AttackerSize,
                AmmoPerAttack = plane.AttackData.AttackCount,
                AttackCd = Math.Round((decimal)plane.AttackData.AttackCooldown, 1),
                JatoDuration = jatoDuration,
                JatoSpeedMultiplier = jatoMultiplier,
                WeaponType = weaponType.ToString(),
                Weapon = weapon,
            };

            cvAircraft.CVAircraftData = cvAircraft.ToPropertyMapping();

            return cvAircraft;
        }
    }
}
