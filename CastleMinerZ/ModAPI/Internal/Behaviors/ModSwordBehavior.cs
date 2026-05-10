using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal.Behaviors
{
    public class ModSwordBehavior : ModelInventoryItemClass
    {
        public ModSwordBehavior(ItemDef def)
            : base(InventoryItemIDs.BareHands, null, def.DisplayName,
                   def.Description1, def.Description2,
                   def.MaxStackSize, def.CoolDownTime, def.LaserColor)
        {
            ModItemId = def.Id;
            EnemyDamage = def.EnemyDamage;
            EnemyDamageType = def.EnemyDamageType;
            ItemSelfDamagePerUse = def.ItemSelfDamagePerUse;
            // Sword behavior always uses Pick animation mode, matching vanilla
            // laser swords. The avatar prop-bone pose is keyed off this, so
            // overriding via def would break the in-hand orientation.
            _playerMode = PlayerMode.Pick;
            if (!string.IsNullOrEmpty(def.UseSoundCue))
                SetUseSound(def.UseSoundCue);

            _model = CastleMinerZGame.Instance.Content.Load<Model>("Saber");
            ToolColor = def.LaserColor;
            IconTextureName = def.IconTextureName;
        }

        public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
        {
            // Use PickModelEntity so the Saber model renders with its laser-blade
            // additive-blend effect (see PickInventoryItemClass.PickModelEntity).
            // Plain CastleMinerToolModel doesn't render the Beam mesh correctly.
            PickInventoryItemClass.PickModelEntity entity = new PickInventoryItemClass.PickModelEntity(_model);
            entity.EnablePerPixelLighting();
            entity.ToolColor = ToolColor;
            switch (use)
            {
                case ItemUse.UI:
                {
                    Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f);
                    Matrix localToParent = Matrix.Transform(
                        Matrix.CreateScale(32f / entity.GetLocalBoundingSphere().Radius), rotation);
                    entity.LocalToParent = localToParent;
                    entity.EnableDefaultLighting();
                    break;
                }
                case ItemUse.Hand:
                {
                    // Hand pose: same as vanilla laser sword (no rotation tweak,
                    // because PickInventoryItemClass uses the laserSword branch which
                    // is just default orientation).
                    break;
                }
            }
            return entity;
        }
    }
}
