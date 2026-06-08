using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hereticide
{
    public class Player
    {
        public Vector2 Position;
        public Vector2 Velocity;

        // base stats (modified by upgrades)
        public float MaxHp = 100f;
        public float Hp = 100f;
        public float MoveSpeed = 130f;
        public float PickupRadius = 70f;
        public float Radius = 6f;       // collision radius in world units

        // multipliers applied to weapons
        public float DamageMult = 1f;
        public float CooldownMult = 1f;
        public float ProjSpeedMult = 1f;
        public float AreaMult = 1f;
        public float Regen = 0f;        // hp per second
        public float Armor = 0f;        // flat damage reduction
        public float MagnetPulse = 0f;  // when >0, pulls all gems

        public int Level = 1;
        public float Xp = 0f;
        public float XpToNext = 5f;

        public int Facing = 1;          // 1 right, -1 left
        public float HurtFlash = 0f;
        public float WalkCycle = 0f;
        public bool Alive = true;

        public readonly List<Weapon> Weapons = new List<Weapon>();

        public void Reset(Vector2 pos)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            MaxHp = 100f; Hp = 100f;
            MoveSpeed = 130f;
            PickupRadius = 70f;
            DamageMult = 1f; CooldownMult = 1f; ProjSpeedMult = 1f; AreaMult = 1f;
            Regen = 0f; Armor = 0f; MagnetPulse = 0f;
            Level = 1; Xp = 0f; XpToNext = 5f;
            Facing = 1; HurtFlash = 0f; WalkCycle = 0f; Alive = true;
            Weapons.Clear();
        }

        public T GetWeapon<T>() where T : Weapon
        {
            foreach (var w in Weapons) if (w is T t) return t;
            return null;
        }

        public void Update(float dt, Vector2 moveDir)
        {
            if (moveDir.LengthSquared() > 1f) moveDir.Normalize();
            Velocity = moveDir * MoveSpeed;
            Position += Velocity * dt;

            if (Velocity.X > 5f) Facing = 1;
            else if (Velocity.X < -5f) Facing = -1;

            if (Velocity.LengthSquared() > 1f) WalkCycle += dt * 10f;
            else WalkCycle = 0f;

            if (Regen > 0f && Hp < MaxHp)
                Hp = Math.Min(MaxHp, Hp + Regen * dt);

            if (HurtFlash > 0f) HurtFlash -= dt;
            if (MagnetPulse > 0f) MagnetPulse -= dt;
        }

        public void TakeDamage(float dmg)
        {
            float final = Math.Max(1f, dmg - Armor);
            Hp -= final;
            HurtFlash = 0.22f;
            if (Hp <= 0f) { Hp = 0f; Alive = false; }
        }
    }
}
