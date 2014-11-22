﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using Color = System.Drawing.Color;

namespace Assemblies.Champions {
    internal class Kalista : Champion {
        private static bool doneAA;
        /**
         * TODO: 
         * 
         * Works on summoners rift.

           Good job on this, Q and E works nicely, care to add KS drake + Baron/Buffs with E? I stole drake from jungler (more dmg than smite with my E).
           
         * Suggestions:
            Q and E to secure lasthits(that you would normally miss).
            Q waveclear, when enough rend stacks to kill more than 3 minions use Q on killable.(add slider if you want? xD).
            E waveclear lasthitting creeps when they can die from current stacks, its really good seriously, because E resets on minion kill, and cost only 35 mana, you should really add this feature and see how good it is. (with a mana slider). Would add this to priority one xD.
            Auto cast ultimate on gapclosers(on/off toggle xD).
         *  W Spots thats show Sentinel routes.
            Q to dodge skillshots(Ashe ulted me, evades movepackets and auto Q saved me) I know this takes time but quite a good feature.
         *  
         */

        public Kalista() {
            if (player.ChampionName != "Kalista") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Game.PrintChat("[Assemblies] - Kalista Loaded.");

            var wc = new WebClient {Proxy = null};

            wc.DownloadString("http://league.square7.ch/put.php?name=iKalista");
            string amount = wc.DownloadString("http://league.square7.ch/get.php?name=iKalista");
            Game.PrintChat("[Assemblies] - iKalista has been loaded " + Convert.ToInt32(amount) +
                           " times by LeagueSharp Users.");
            Game.PrintChat(
                "[Assemblies] - This is only in BETA, please PM iJava or leave feedback on thread with suggestions and bugs.");
        }

        private static int GetSpearCount {
            get {
                int xBuffCount = 0;
                foreach (
                    BuffInstance buff in
                        from enemy in
                            ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget(975))
                        from buff in enemy.Buffs
                        where buff.Name.Contains("kalistaexpungemarker")
                        select buff) {
                    xBuffCount = buff.Count;
                }
                return xBuffCount;
            }
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 5500);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 1350);

            Q.SetSkillshot(0.12f, 40, 1800, true, SkillshotType.SkillshotLine);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            //menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useER", "Use E out of range").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(true));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("autoE", "Auto E killable Minions").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("eNum", "Number of minions").SetValue(new Slider(3, 1, 10)));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQK", "Use Q for killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useEK", "Use E for killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E Range").SetValue(false));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("eStacks", "Cast E on stacks").SetValue(new Slider(2, 1, 10)));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;
            // (player.IsAutoAttacking && !doneAA)
            //   {
            //     sendMovementPacket(Game.CursorPos.To2D());
            //       doneAA = true;
            //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            //   }
            //   else
            //   {
            //       doneAA = false;
            //   }
            // Console.WriteLine(player..ToString());
            Obj_AI_Hero target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);

            killsteal(target);

            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    if (isMenuEnabled(menu, "useQC"))
                        castQ(target);
                    if (isMenuEnabled(menu, "useEC") && E.IsReady()) {
                        if (GetSpearCount >= menu.Item("eStacks").GetValue<Slider>().Value &&
                            player.Distance(target) <= E.Range) {
                            E.Cast(true);
                        }
                    }
                    if (isMenuEnabled(menu, "useER")) {
                        if (player.Distance(target) >= 965 && GetSpearCount >= 1) {
                            if (E.IsReady())
                                E.Cast(true);
                        }
                    }
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (isMenuEnabled(menu, "useQH"))
                        castQ(target);
                    if (isMenuEnabled(menu, "useEH")) {
                        if (GetSpearCount >= menu.Item("eStacks").GetValue<Slider>().Value &&
                            player.Distance(target) <= E.Range) {
                            E.Cast(true);
                        }
                    }
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    AutoKillMinion();
                    break;
                case LXOrbwalker.Mode.Flee:
                    //TODO flee mode
                    break;
            }
        }

        private void AutoKillMinion() {
            List<Obj_AI_Base> minions = MinionManager.GetMinions(player.ServerPosition, E.Range);
            int count = minions.Count(minion => E.GetDamage(minion) > minion.Health);

            if (count >= menu.Item("eNum").GetValue<Slider>().Value && menu.Item("autoE").GetValue<bool>() &&
                E.IsReady())
                E.Cast(true);
        }

        private void onDraw(EventArgs args) {
            if (menu.Item("drawQ").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, Q.Range, Color.Cyan);
            }
            if (menu.Item("drawE").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, E.Range, Color.Crimson);
            }
        }

        private void castQ(Obj_AI_Hero target) {
            if (target.IsValidTarget(Q.Range) && player.Distance(target) <= Q.Range) {
                if (Q.IsReady() && Q.GetPrediction(target).Hitchance >= HitChance.High) {
                    Q.Cast(target, true);
                }
            }
        }

        private void QCalculation(Obj_AI_Hero target) {
            List<Obj_AI_Base> List = MinionManager.GetMinions(player.Position, Q.Range);
            float QDistance = Q.Range;
            float QWidth = Q.Width;
            float QSpeed = Q.Speed;
            foreach (Obj_AI_Base Minion in List.Where(m => m.IsEnemy && Q.GetDamage(m) >= m.Health)) {
                float Distance = player.Distance(Minion);
                float PTD = player.Distance(target);
                float Diff = PTD - Distance;
                for (int i = 0; i < Diff; i += (int) target.BoundingRadius) {
                    Vector3 Point = Minion.Position.To2D().Extend(player.Position.To2D(), -i).To3D();
                    float DistToPoint = player.Distance(Point);
                    float Time = (DistToPoint/Q.Speed)*1000;
                        //Maybe *1000 should be removed ? not sure //TODO - corey - idk im dumb.
                    PredictionOutput Pred = Prediction.GetPrediction(target, Time);
                    if (Pred.UnitPosition.Distance(Point) <= QWidth && !List.Any(m => m.Distance(Point) <= QWidth)) {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }

        private void killsteal(Obj_AI_Hero target) {
            if (target.IsValidTarget(E.Range) && E.IsReady() &&
                player.GetSpellDamage(target, SpellSlot.E)*GetSpearCount > target.Health) {
                if (isMenuEnabled(menu, "useEK"))
                    E.Cast(true);
            }
            if (target.IsValidTarget(Q.Range) && Q.IsReady() && Q.IsKillable(target)) {
                if (isMenuEnabled(menu, "useQK")) {
                    castQ(target);
                }
            }
        }
    }
}