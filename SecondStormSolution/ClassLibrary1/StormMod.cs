using ItemLib;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using BepInEx;
using UnityEngine;
using System;

namespace SecondStorm
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(ItemLibPlugin.ModGuid)]
    [BepInPlugin("dev.JudsonEsq.SecondStorm", "Second Storm", "1.0.0")]

    public class StormMod : BaseUnityPlugin
    {
        string name = "Second Storm";
        int locStorage;


        void Start()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor zap = new ILCursor(il);
            zap.GotoNext
            (
                x => x.MatchLdcI4(0x18),
                x => x.MatchCall<CharacterBody>("HasBuff"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcR4(1),
                x => x.MatchAdd(),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out locStorage)
            ); 

            zap.Emit(OpCodes.Ldloc, locStorage);
            zap.Emit(OpCodes.Ldarg, 0);

            zap.EmitDelegate<Func<RoR2.CharacterBody, float, float>>(
                (self, speed) =>
                {
                    if (self.inventory && self.HasBuff((BuffIndex)ItemLib.ItemLib.GetBuffId("LightningBuff")))
                    {
                        speed += 2;
                    }
                    return speed;
                });
            Debug.Log(zap);
            zap.Emit(OpCodes.Stloc, locStorage);
            
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log("F3 has been pressed");
                RoR2.PlayerCharacterMasterController.instances[0].master.GetBody().AddTimedBuff((BuffIndex)ItemLib.ItemLib.GetBuffId("LightningBuff"), 5);
            }
        }




        private static ItemDisplayRule[] _itemDisplayRules;

        [Item(ItemAttribute.ItemType.Item)]
        public static ItemLib.CustomItem lowHealthZoomies()
        {
            ItemDef lowHealthZoomies = new ItemDef
            {
                tier = ItemTier.Tier2,
                pickupModelPath = "",
                pickupIconPath = "",
                nameToken = "Second Storm",
                pickupToken = "When you are lightning, you can strike as many times as you like.",
                descriptionToken = "When you fall below 30% health, movement speed doubles."
            };

            _itemDisplayRules = null;
            return new ItemLib.CustomItem(lowHealthZoomies, null, null, _itemDisplayRules);
        }

        [Item(ItemAttribute.ItemType.Buff)]
        public static CustomBuff Lightning()
        {
            BuffDef lightningBuff = new BuffDef
            {
                buffColor = Color.green,
                canStack = false
            };

            Sprite icon = null;

            return new CustomBuff("LightningBuff", lightningBuff, icon);

        }
    }
}
