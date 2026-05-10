using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal.Behaviors
{
    public class ModAxeBehavior : ModelInventoryItemClass
    {
        public ModAxeBehavior(ItemDef def)
            : base(InventoryItemIDs.BareHands, null, def.DisplayName,
                   def.Description1, def.Description2,
                   def.MaxStackSize, def.CoolDownTime, Color.White)
        {
            ModItemId = def.Id;
            EnemyDamage = def.EnemyDamage;
            EnemyDamageType = def.EnemyDamageType;
            ItemSelfDamagePerUse = def.ItemSelfDamagePerUse;
            _playerMode = PlayerMode.Pick;
            if (!string.IsNullOrEmpty(def.UseSoundCue))
                SetUseSound(def.UseSoundCue);

            _model = CastleMinerZGame.Instance.Content.Load<Model>("Axe");
            ToolColor = Color.White;
            IconTextureName = def.IconTextureName;
        }

        public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
        {
            CastleMinerToolModel toolModel = new CastleMinerToolModel(_model);
            toolModel.EnablePerPixelLighting();
            toolModel.ToolColor = ToolColor;
            switch (use)
            {
                case ItemUse.UI:
                {
                    Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f);
                    Matrix localToParent = Matrix.Transform(
                        Matrix.CreateScale(32f / toolModel.GetLocalBoundingSphere().Radius), rotation);
                    toolModel.LocalToParent = localToParent;
                    toolModel.EnableDefaultLighting();
                    break;
                }
                case ItemUse.Hand:
                {
                    toolModel.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2f)
                        * Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 4f);
                    toolModel.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
                    break;
                }
            }
            return toolModel;
        }

        public override InventoryItem CreateItem(int stackCount)
        {
            return new AxeInventoryItem(this, stackCount);
        }
    }
}
