using System;
using System.Collections.Generic;
using System.Reflection;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using UnityEditor;
using static OSCore.ScriptableObjects.AnimSOEdge;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/animator")]
    public class PlayerAnimationCfgSO : ACharacterAnimatorCfgSO<PlayerAnim, PlayerAnimState> {
        [field: Header("Transition speeds")]
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float scopingSpeed { get; private set; }
        [field: SerializeField] public float punchingSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }
        [field: SerializeField] public float landingSpeed { get; private set; }
        [field: SerializeField] public float lungingSpeed { get; private set; }
        [field: SerializeField] public float ledgeShimmySpeed { get; private set; }

        private void BuildAsset() {
            nodes.RemoveAll(node => {
                if (node != null) AssetDatabase.RemoveObjectFromAsset(node);
                return true;
            });
            edges.RemoveAll(edge => {
                if (edge != null) AssetDatabase.RemoveObjectFromAsset(edge);
                return true;
            });

            Vector2 position = Vector2.zero;

            Vector2 toPosition() => position += new Vector2(20, 20);
            string stand_idle = CreateNode<PlayerAnimSONode>(PlayerAnim.stand_idle, toPosition());
            string stand_move = CreateNode<PlayerAnimSONode>(PlayerAnim.stand_move, toPosition());
            string stand_punch = CreateNode<PlayerAnimSONode>(PlayerAnim.stand_punch, toPosition());
            string stand_fall = CreateNode<PlayerAnimSONode>(PlayerAnim.stand_fall, toPosition());
            string crouch_idle_bino = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_idle_bino, toPosition());
            string crouch_move_bino = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_move_bino, toPosition());
            string crouch_tobino = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_tobino, toPosition());
            string crouch_idle = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_idle, toPosition());
            string crouch_move = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_move, toPosition());
            string crouch_punch = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_punch, toPosition());
            string crouch_toaim = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_toaim, toPosition());
            string crouch_idle_aim = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_idle_aim, toPosition());
            string crouch_move_aim = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_move_aim, toPosition());
            string crouch_fire = CreateNode<PlayerAnimSONode>(PlayerAnim.crouch_fire, toPosition());
            string crawl_idle_bino = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_idle_bino, toPosition());
            string crawl_tobino = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_tobino, toPosition());
            string crawl_idle = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_idle, toPosition());
            string crawl_move = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_move, toPosition());
            string crawl_punch = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_punch, toPosition());
            string crawl_toaim = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_toaim, toPosition());
            string crawl_idle_aim = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_idle_aim, toPosition());
            string crawl_fire = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_fire, toPosition());
            string crawl_dive = CreateNode<PlayerAnimSONode>(PlayerAnim.crawl_dive, toPosition());
            string hang_lunge = CreateNode<PlayerAnimSONode>(PlayerAnim.hang_lunge, toPosition());
            string hang_idle = CreateNode<PlayerAnimSONode>(PlayerAnim.hang_idle, toPosition());
            string hang_move = CreateNode<PlayerAnimSONode>(PlayerAnim.hang_move, toPosition());
            string hang_climb = CreateNode<PlayerAnimSONode>(PlayerAnim.hang_climb, toPosition());

            //stand_idle
            Transition(stand_idle, stand_fall,
                And("fall"));
            Transition(stand_idle, crawl_dive,
                And("dive"));
            Transition(stand_idle, stand_move,
                And(("sprint", Comparator.EQ, default, true), ("move", Comparator.EQ, default, true)));
            Transition(stand_idle, stand_move,
                defaultSpeed,
                0f,
                And("move", false),
                And("sprint", false),
                And("stance", Comparator.NE, (int)PlayerStance.STANDING));
            //stand_move
            Transition(stand_move, stand_fall,
                And("fall"));
            Transition(stand_move, crawl_dive,
                And("dive"));
            Transition(stand_move, crouch_move,
                defaultSpeed,
                0f,
                And("stance", Comparator.NE, (int)PlayerStance.STANDING),
                And("sprint", false));
            Transition(stand_move, stand_idle,
                And("move", false));
            Transition(stand_move, crouch_tobino,
                And("scope"));
            Transition(stand_move, crouch_toaim,
                And("aim"));
            Transition(stand_move, stand_punch,
                And("attack"));
            //stand_punch
            Transition(stand_punch, stand_punch,
                And("fall"));
            Transition(stand_punch, stand_move, punchingSpeed, 0f,
                And("move"));
            Transition(stand_punch, stand_idle, punchingSpeed, 0f,
                And("move", false));
            //stand_fall
            Transition(stand_fall, hang_lunge,
                And("hang"));
            Transition(stand_fall, stand_move,
                And(("fall", Comparator.EQ, default, false), ("sprint", Comparator.EQ, default, true)));
            Transition(stand_fall, stand_move,
                And(("fall", Comparator.EQ, default, false), ("sprint", Comparator.EQ, default, false)));


            //crouch_idle_bino
            Transition(crouch_idle_bino, stand_fall,
                And("fall"));
            Transition(crouch_idle_bino, crouch_tobino,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.STANDING));
            Transition(crouch_idle_bino, crawl_idle_bino,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.CRAWLING));
            Transition(crouch_idle_bino, crouch_tobino,
                And("scope", false));
            Transition(crouch_idle_bino, crouch_move_bino,
                And("move"));
            //crouch_move_bino
            Transition(crouch_move_bino, stand_fall,
                And("fall"));
            Transition(crouch_move_bino, crawl_tobino,
                And("sprint"));
            Transition(crouch_move_bino, crouch_tobino,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.STANDING));
            Transition(crouch_move_bino, crouch_tobino,
                And("scope", false));
            Transition(crouch_move_bino, crouch_idle_bino,
                And("move", false));
            //crouch_tobino
            Transition(crouch_tobino, stand_fall,
                And("fall"));
            Transition(crouch_tobino, crawl_dive,
                And("dive"));
            Transition(crouch_tobino, stand_move,
                scopingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("sprint", Comparator.EQ, default, true)));
            Transition(crouch_tobino, crouch_move_bino,
                scopingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("scope", Comparator.EQ, default, true)));
            Transition(crouch_tobino, crouch_move,
                scopingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("scope", Comparator.EQ, default, false)));
            Transition(crouch_tobino, crouch_idle_bino,
                scopingSpeed,
                0f,
                And(("move", Comparator.EQ, default, false), ("scope", Comparator.EQ, default, true)));
            Transition(crouch_tobino, crouch_idle,
                scopingSpeed,
                0f,
                And(("move", Comparator.EQ, default, false), ("scope", Comparator.EQ, default, false)));
            //crouch_idle
            Transition(crouch_idle, stand_fall,
                And("fall"));
            Transition(crouch_idle, crawl_dive,
                And("dive"));
            Transition(crouch_idle, crawl_idle,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.CRAWLING));
            Transition(crouch_idle, crouch_move,
                And("move"));
            Transition(crouch_idle, crouch_tobino,
                And("scope"));
            Transition(crouch_idle, crouch_toaim,
                And("aim"));
            Transition(crouch_idle, crouch_punch,
                And("attack"));
            //crouch_move
            Transition(crouch_move, stand_fall,
                And("fall"));
            Transition(crouch_move, crawl_dive,
                And("dive"));
            Transition(crouch_move, stand_move,
                defaultSpeed,
                0.5f,
                And("sprint"));
            Transition(crouch_move, crawl_move,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.CRAWLING));
            Transition(crouch_move, crouch_idle,
                And("move", false));
            Transition(crouch_move, crouch_tobino,
                And("scope"));
            Transition(crouch_move, crouch_toaim,
                And("aim"));
            Transition(crouch_move, crouch_punch,
                And("attack"));
            //crouch_punch
            Transition(crouch_punch, stand_fall,
                And("fall"));
            Transition(crouch_punch, crouch_move,
                punchingSpeed,
                0f,
                And("move"));
            Transition(crouch_punch, crouch_idle,
                punchingSpeed,
                0f,
                And("move", false));
            //crouch_toaim
            Transition(crouch_toaim, stand_fall,
                And("fall"));
            Transition(crouch_toaim, crawl_dive,
                And("dive"));
            Transition(crouch_toaim, stand_move,
                aimingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("sprint", Comparator.EQ, default, true)));
            Transition(crouch_toaim, crouch_move_aim,
                aimingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("aim", Comparator.EQ, default, true)));
            Transition(crouch_toaim, crouch_move,
                aimingSpeed,
                0f,
                And(("move", Comparator.EQ, default, true), ("aim", Comparator.EQ, default, false)));
            Transition(crouch_toaim, crouch_idle_aim,
                aimingSpeed,
                0f,
                And(("move", Comparator.EQ, default, false), ("aim", Comparator.EQ, default, true)));
            Transition(crouch_toaim, crouch_idle,
                aimingSpeed,
                0f,
                And(("move", Comparator.EQ, default, false), ("aim", Comparator.EQ, default, false)));

            //crouch_idle_aim
            Transition(crouch_idle_aim, stand_fall,
                And("fall"));
            Transition(crouch_idle_aim, crawl_idle_aim,
                defaultSpeed,
                0f,
                And("stance", (int)PlayerStance.CRAWLING));
            Transition(crouch_idle_aim, crouch_move_aim,
                And("move"));
            Transition(crouch_idle_aim, crouch_toaim,
                And("aim", false));
            Transition(crouch_idle_aim, crouch_fire,
                And("attack"));
            //crouch_move_aim
            Transition(crouch_move_aim, stand_fall,
                And("fall"));
            Transition(crouch_move_aim, crouch_toaim,
                And("sprint"));
            Transition(crouch_move_aim, crouch_idle_aim,
                And("move", false));
            Transition(crouch_move_aim, crouch_toaim,
                And("aim", false));
            Transition(crouch_move_aim, crouch_fire,
                And("attack"));
            //crouch_fire
            Transition(crouch_fire, stand_fall,
                And("fall"));
            Transition(crouch_fire, crouch_move_aim,
                firingSpeed,
                0f,
                And("move"));
            Transition(crouch_fire, crouch_idle_aim,
                firingSpeed,
                0f,
                And("move", false));


            //crawl_idle_bino
            Transition(crawl_idle_bino, stand_fall,
                And("fall"));
            Transition(crawl_idle_bino, crouch_idle_bino,
                defaultSpeed,
                0f,
                And("stance", Comparator.NE, (int)PlayerStance.CRAWLING));
            Transition(crawl_idle_bino, crawl_tobino,
                And("scope", false),
                And("move"));
            //crawl_tobino
            Transition(crawl_tobino, stand_fall,
                And("fall"));
            Transition(crawl_tobino, crawl_move,
                scopingSpeed,
                0f,
                And("move"));
            Transition(crawl_tobino, crawl_idle_bino,
                scopingSpeed,
                0f,
                And("scope", false));
            //crawl_idle
            Transition(crawl_idle, stand_fall,
                And("fall"));
            Transition(crawl_idle, crouch_idle,
                defaultSpeed,
                0f,
                And("stance", Comparator.NE, (int)PlayerStance.CRAWLING));
            Transition(crawl_idle, crawl_move,
                And("move"));
            Transition(crawl_idle, crawl_tobino,
                And("scope"));
            Transition(crawl_idle, crawl_toaim,
                And("aim"));
            Transition(crawl_idle, crawl_punch,
                And("attack"));
            //crawl_move
            Transition(crawl_move, stand_fall,
                And("fall"));
            Transition(crawl_move, crouch_move,
                defaultSpeed,
                0.5f,
                And("sprint"));
            Transition(crawl_move, crouch_move,
                defaultSpeed,
                0f,
                And("stance", Comparator.NE, (int)PlayerStance.CRAWLING));
            Transition(crawl_move, crawl_idle,
                And("move", false));
            //crawl_punch
            Transition(crawl_punch, stand_fall,
                And("fall"));
            Transition(crawl_punch, crawl_move,
                And("move"));
            Transition(crawl_punch, crawl_idle,
                And("move", false));
            //crawl_toaim
            Transition(crawl_toaim, stand_fall,
                And("fall"));
            Transition(crawl_toaim, crawl_move,
                aimingSpeed,
                0f,
                And("move"));
            Transition(crawl_toaim, crawl_idle_aim,
                aimingSpeed,
                0f,
                And("aim"));
            Transition(crawl_toaim, crawl_idle,
                aimingSpeed,
                0f,
                And("aim", false));
            //crawl_idle_aim
            Transition(crawl_idle_aim, stand_fall,
                And("fall"));
            Transition(crawl_idle_aim, crouch_idle_aim,
                defaultSpeed,
                0f,
                And("stance", Comparator.NE, (int)PlayerStance.CRAWLING));
            Transition(crawl_idle_aim, crawl_toaim,
                And("aim", false),
                And("move"));
            Transition(crawl_idle_aim, crawl_fire,
                And("attack"));
            //crawl_fire
            Transition(crawl_fire, stand_fall,
                And("fall"));
            Transition(crawl_fire, crawl_idle_aim,
                firingSpeed,
                0f,
                And("attack", false));
            //crawl_dive
            Transition(crawl_dive, stand_fall,
                0.5f,
                0f,
                And("fall"));
            Transition(crawl_dive, crawl_idle,
                And("fall", false));


            //hang_lunge
            Transition(hang_lunge, hang_idle,
                0f,
                1f,
                And("hang"));
            //hang_idle
            Transition(hang_idle, stand_fall,
                And("fall"));
            Transition(hang_idle, hang_climb,
                And("climb"));
            Transition(hang_idle, hang_move,
                And("move"));
            //hang_move
            Transition(hang_move, stand_fall,
                And("fall"));
            Transition(hang_move, hang_climb,
                And("climb"));
            Transition(hang_move, hang_idle,
                ledgeShimmySpeed,
                0f,
                And("move", false));
            //hang_climb
            Transition(hang_climb, crouch_idle,
                0f,
                1f,
                And("climb"));
            AssetDatabase.SaveAssets();
        }

        public override AnimNode<PlayerAnim, PlayerAnimState> Init() {
            BuildAsset();

            return BuildNodes()
                .Values
                .First(Fns.Filter<AnimNode<PlayerAnim, PlayerAnimState>>(entry =>
                    entry.state == PlayerAnim.crouch_idle));
        }

        private void Transition(
            string from,
            string to,
            params (string, Comparator, float, bool)[][] conditions
        ) {
            List<AndCondition> edgeConditions = conditions.Reduce((ors, or) =>
                ors.With(or.Reduce((ands, and) =>
                    ands.With(new PropComparator() {
                        prop = and.Item1,
                        comparator = and.Item2,
                        floatValue = and.Item3,
                        boolValue = and.Item4,
                    }),
                    new AndCondition())),
                new List<AndCondition>());

            SetEdge<PlayerAnimSOEdge>(from, to, edgeConditions.ToArray());
        }

        private void Transition(
            string from,
            string to,
            float minTime,
            float minLoops,
            params (string, Comparator, float, bool)[][] conditions
        ) {
            List<AndCondition> edgeConditions = conditions.Reduce((ors, or) =>
                ors.With(or.Reduce((ands, and) =>
                    ands.With(new PropComparator() {
                        prop = and.Item1,
                        comparator = and.Item2,
                        floatValue = and.Item3,
                        boolValue = and.Item4,
                    }).With(new PropComparator() {
                        prop = "timeInState",
                        comparator = Comparator.GTE,
                        floatValue = minTime,
                    }).With(new PropComparator() {
                        prop = "loops",
                        comparator = Comparator.GTE,
                        floatValue = minLoops,
                    }),
                    new AndCondition())),
                new List<AndCondition>()); ;

            PlayerAnimSOEdge edge = CreateInstance<PlayerAnimSOEdge>();
            edge.from = from;
            edge.to = to;
            edge.conditions = edgeConditions;
            edges.Add(edge);
            AssetDatabase.AddObjectToAsset(edge, this);
        }

        private bool PropMatches(
            object record,
            (string prop, Comparator comparator, float floatValue, bool boolValue) comp
        ) {
            IDictionary<string, PropertyInfo> props = record
                .GetType()
                .GetProperties()
                .Reduce(
                    (m, prop) => { m.Add(prop.Name, prop); return m; },
                    new Dictionary<string, PropertyInfo>());

            object propValue = props[comp.prop].GetValue(record);
            int comparisonResult = propValue switch {
                bool b => b.CompareTo(comp.boolValue),
                _ => comp.floatValue.CompareTo(Convert.ChangeType(propValue, typeof(float))),
            };

            return comp.comparator switch {
                Comparator.EQ => comparisonResult == 0,
                Comparator.NE => comparisonResult != 0,
                Comparator.GT => comparisonResult < 0,
                Comparator.LT => comparisonResult > 0,
                Comparator.GTE => comparisonResult <= 0,
                Comparator.LTE => comparisonResult >= 0,
                _ => false,
            };
        }

        private Predicate<PlayerAnimState> ToPred(List<AndCondition> conditions) {
            return state => {
                foreach (AndCondition group in conditions) {
                    bool all = true;
                    foreach (PropComparator condition in group) {
                        if (!PropMatches(state, (condition.prop, condition.comparator, condition.floatValue, condition.boolValue))) {
                            all = false;
                            break;
                        }
                    }
                    if (all) return true;
                }

                return false;
            };
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, Comparator.EQ, default, true, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            float floatValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, Comparator.EQ, floatValue, default, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            bool boolValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, Comparator.EQ, default, boolValue, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            float floatValue,
            bool boolValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, Comparator.EQ, floatValue, boolValue, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            Comparator comparator,
            float floatValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, comparator, floatValue, default, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            Comparator comparator,
            bool boolValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            return And(name, comparator, default, boolValue, comparison);
        }

        private (string, Comparator, float, bool)[] And(
            string name,
            Comparator comparator,
            float floatValue,
            bool boolValue,
            params (string, Comparator, float, bool)[] comparison
        ) {
            (string, Comparator, float, bool)[] result = new (string, Comparator, float, bool)[comparison.Length + 1];
            result[0] = (name, comparator, floatValue, boolValue);
            Array.Copy(comparison, 0, result, 1, comparison.Length);
            return result;
        }

        private (string, Comparator, float, bool)[] And(
            params (string, Comparator, float, bool)[] comparison) {
            return comparison;
        }

        public record PlayerAnimState : AnimStateDetails<PlayerAnim> {
            public PlayerStance stance { get; init; }
            public bool fall { get; init; }
            public bool sprint { get; init; }
            public bool dive { get; init; }
            public bool attack { get; init; }
            public bool move { get; init; }
            public bool hang { get; init; }
            public bool climb { get; init; }
            public bool scope { get; init; }
            public bool aim { get; init; }
        }
    }
}
