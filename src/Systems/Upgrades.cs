using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hereticide
{
    public class UpgradeOption
    {
        public string Title;
        public string Tag;     // "NEW WEAPON", "LV 3", "PASSIVE"
        public string Desc;
        public Color Accent;
        public Action Apply;
    }

    public static class Upgrades
    {
        static readonly Random Rng = new Random();

        public static List<UpgradeOption> Roll(World w, int count)
        {
            var pool = new List<UpgradeOption>();
            var p = w.Player;

            // ---- level up existing weapons ----
            foreach (var wp in p.Weapons)
            {
                if (wp.IsMaxed) continue;
                var weapon = wp; // capture
                pool.Add(new UpgradeOption
                {
                    Title = weapon.Name,
                    Tag = "LV " + (weapon.Level + 1),
                    Desc = weapon.NextLevelText(),
                    Accent = new Color(255, 200, 80),
                    Apply = () => weapon.LevelUp()
                });
            }

            // ---- acquire new weapons ----
            if (p.GetWeapon<PlasmaWeapon>() == null)
                pool.Add(NewWeapon(p, new PlasmaWeapon(), "Piercing plasma bolts"));
            if (p.GetWeapon<FragWeapon>() == null)
                pool.Add(NewWeapon(p, new FragWeapon(), "Lobs explosive frag grenades"));

            // ---- passives ----
            var heart = new Color(235, 90, 90);
            var blue = new Color(120, 200, 255);
            var green = new Color(120, 220, 130);
            pool.Add(Passive("BIONIC HEART", "+25 max HP & heal", heart,
                () => { p.MaxHp += 25f; p.Hp = Math.Min(p.MaxHp, p.Hp + 25f); }));
            pool.Add(Passive("FRENZON", "+12% move speed", green,
                () => { p.MoveSpeed *= 1.12f; }));
            pool.Add(Passive("MACHINE SPIRIT", "+14% all damage", new Color(255, 150, 60),
                () => { p.DamageMult *= 1.14f; }));
            pool.Add(Passive("TARGETER", "-8% weapon cooldown", new Color(255, 220, 120),
                () => { p.CooldownMult *= 0.92f; }));
            pool.Add(Passive("AUTO-LOADER", "+15% projectile speed", blue,
                () => { p.ProjSpeedMult *= 1.15f; }));
            pool.Add(Passive("BLESSED RANGE", "+25 pickup range", green,
                () => { p.PickupRadius += 25f; }));
            pool.Add(Passive("CERAMITE PLATING", "+2 armour", new Color(170, 180, 200),
                () => { p.Armor += 2f; }));
            pool.Add(Passive("NARTHECIUM", "+0.6 HP/sec regen", heart,
                () => { p.Regen += 0.6f; }));
            pool.Add(Passive("BLAST RADIUS", "+12% area size", new Color(255, 120, 90),
                () => { p.AreaMult *= 1.12f; }));

            // ---- shuffle and take ----
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                var t = pool[i]; pool[i] = pool[j]; pool[j] = t;
            }

            var result = new List<UpgradeOption>();
            for (int i = 0; i < pool.Count && result.Count < count; i++)
                result.Add(pool[i]);
            return result;
        }

        static UpgradeOption NewWeapon(Player p, Weapon weapon, string desc)
        {
            return new UpgradeOption
            {
                Title = weapon.Name,
                Tag = "NEW WEAPON",
                Desc = desc,
                Accent = new Color(120, 220, 255),
                Apply = () => p.Weapons.Add(weapon)
            };
        }

        static UpgradeOption Passive(string title, string desc, Color accent, Action apply)
        {
            return new UpgradeOption { Title = title, Tag = "PASSIVE", Desc = desc, Accent = accent, Apply = apply };
        }
    }
}
