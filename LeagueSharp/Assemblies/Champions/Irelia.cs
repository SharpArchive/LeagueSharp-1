﻿using System;
using System.Runtime.InteropServices;
using Assemblies.Utilitys;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Irelia : Champion {
        public Irelia() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(0.15f, 80f, 1500f, false, SkillshotType.SkillshotLine);

            //TODO set skillshots
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("gapcloseQ", "Use Q to gapclose").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("OStunE", "Only Use E to stun").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useRH", "Use R in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("harassSlider", "hp to harass").SetValue(new Slider(30)));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQL", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useWL", "Use W in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useEL", "Use E in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useRL", "Use R in laneclear").SetValue(true));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQKS", "Use Q in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useWKS", "Use W in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useEKS", "Use E in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useRKS", "Use R in killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Flee Options", "flee"));
            menu.SubMenu("flee").AddItem(new MenuItem("useQF", "Use Q to flee").SetValue(true));
            menu.SubMenu("flee").AddItem(new MenuItem("useRF", "Use R to lower minions in flee").SetValue(false));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw W Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            //TODO idk ?

            menu.AddItem(new MenuItem("creds", "Made by iJabba & DZ191"));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;
            var target = SimpleTs.GetTarget(isMenuEnabled(menu, "gapcloseQ") ? Q.Range : E.Range,
                SimpleTs.DamageType.Physical);

            switch (xSLxOrbwalker.CurrentMode) {
                case xSLxOrbwalker.Mode.Combo:
                    //TODO onCombo
                    break;
            }
        }

        private void Combo(Obj_AI_Hero target)
        {
            xSLxOrbwalker.ForcedTarget = target;
            if (isMenuEnabled(menu, "gapcloseQ") &&
                player.Distance(target) >= 375)
            {
                if (isMenuEnabled(menu, "useQC") && Q.IsReady() && player.Distance(target)<=Q.Range)
                {
                    Q.Cast(target);
                }
            }
            else
            {
                if (isMenuEnabled(menu, "useQC") && Q.IsReady() && player.Distance(target) <= Q.Range)
                {
                    Q.Cast(target);
                }
            }
            W.Cast();
            if (isMenuEnabled(menu, "OStunE"))
            {
                if (canStun(target) && E.IsReady() && player.Distance(target)<=E.Range)
                {
                    E.Cast();
                }
            }
            else
            {
                if (canStun(target) && E.IsReady() && player.Distance(target) <= E.Range)
                {
                    E.Cast();
                }
            }
        }

        private void onDraw(EventArgs args) {}

        private bool canStun(Obj_AI_Hero target) {
            return getPercentValue(target, false) > getPercentValue(player, false);
        }
    }
}