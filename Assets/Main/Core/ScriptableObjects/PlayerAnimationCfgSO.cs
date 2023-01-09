using System;
using System.Collections.Generic;
using System.Reflection;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.System;
using OSCore.Utils;
using UnityEditor;
using UnityEngine;
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

        [field: SerializeField] public List<PlayerAnimSONode> nodes { get; private set; }
        [field: SerializeField] public List<PlayerAnimSOEdge> edges { get; private set; }

        private void BuildAsset() {
            nodes.RemoveAll(node => {
                if (node != null) AssetDatabase.RemoveObjectFromAsset(node);
                return true;
            });
            edges.RemoveAll(edge => {
                if (edge != null) AssetDatabase.RemoveObjectFromAsset(edge);
                return true;
            });


            string stand_idle = CreateNode(PlayerAnim.stand_idle);
            string stand_move = CreateNode(PlayerAnim.stand_move);
            string stand_punch = CreateNode(PlayerAnim.stand_punch);
            string stand_fall = CreateNode(PlayerAnim.stand_fall);
            string crouch_idle_bino = CreateNode(PlayerAnim.crouch_idle_bino);
            string crouch_move_bino = CreateNode(PlayerAnim.crouch_move_bino);
            string crouch_tobino = CreateNode(PlayerAnim.crouch_tobino);
            string crouch_idle = CreateNode(PlayerAnim.crouch_idle);
            string crouch_move = CreateNode(PlayerAnim.crouch_move);
            string crouch_punch = CreateNode(PlayerAnim.crouch_punch);
            string crouch_toaim = CreateNode(PlayerAnim.crouch_toaim);
            string crouch_idle_aim = CreateNode(PlayerAnim.crouch_idle_aim);
            string crouch_move_aim = CreateNode(PlayerAnim.crouch_move_aim);
            string crouch_fire = CreateNode(PlayerAnim.crouch_fire);
            string crawl_idle_bino = CreateNode(PlayerAnim.crawl_idle_bino);
            string crawl_tobino = CreateNode(PlayerAnim.crawl_tobino);
            string crawl_idle = CreateNode(PlayerAnim.crawl_idle);
            string crawl_move = CreateNode(PlayerAnim.crawl_move);
            string crawl_punch = CreateNode(PlayerAnim.crawl_punch);
            string crawl_toaim = CreateNode(PlayerAnim.crawl_toaim);
            string crawl_idle_aim = CreateNode(PlayerAnim.crawl_idle_aim);
            string crawl_fire = CreateNode(PlayerAnim.crawl_fire);
            string crawl_dive = CreateNode(PlayerAnim.crawl_dive);
            string hang_lunge = CreateNode(PlayerAnim.hang_lunge);
            string hang_idle = CreateNode(PlayerAnim.hang_idle);
            string hang_move = CreateNode(PlayerAnim.hang_move);
            string hang_climb = CreateNode(PlayerAnim.hang_climb);

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
            //BuildAsset();
            IDictionary<string, AnimNode<PlayerAnim, PlayerAnimState>> dict =
                nodes.Reduce(
                    (acc, item) => {
                        Debug.Log(item.state + " -> " + item.id);
                        return acc.With(
                            new KeyValuePair<string, AnimNode<PlayerAnim, PlayerAnimState>>(
                                item.id,
                                new AnimNode<PlayerAnim, PlayerAnimState>(
                                    item.state,
                                    item.animSpeed)));
                    },
                    new Dictionary<string, AnimNode<PlayerAnim, PlayerAnimState>>());

            edges.ForEach(edge => {
                PlayerAnimSONode from = nodes.Find(node => node.id == edge.from);
                PlayerAnimSONode to = nodes.Find(node => node.id == edge.to);
                if (from == null || to == null) throw new NotSupportedException("BAD");

                dict[from.id].To(ToPred(edge.conditions), dict[to.id]);
            });

            return dict
                .Values
                .First(Fns.Filter<AnimNode<PlayerAnim, PlayerAnimState>>(entry =>
                    entry.state == PlayerAnim.crouch_idle));
        }

        private string CreateNode(PlayerAnim state) => CreateNode(state, 1f);
        private string CreateNode(PlayerAnim state, float animSpeed) {
            string id = GUID.Generate().ToString();
            PlayerAnimSONode node = CreateInstance<PlayerAnimSONode>();
            node.id = id;
            node.state = state;
            node.animSpeed = animSpeed;
            nodes.Add(node);
            AssetDatabase.AddObjectToAsset(node, this);

            return id;
        }

        private void Transition(
            string from,
            string to,
            params (string, Comparator, float, bool)[][] conditions
        ) {
            List<AnimConditions> edgeConditions = conditions.Reduce((ors, or) =>
                ors.With(or.Reduce((ands, and) =>
                    ands.With(new AnimCondition() {
                        prop = and.Item1,
                        comparator = and.Item2,
                        floatValue = and.Item3,
                        boolValue = and.Item4,
                    }),
                    new AnimConditions())),
                new List<AnimConditions>());

            PlayerAnimSOEdge edge = CreateInstance<PlayerAnimSOEdge>();
            edge.from = from;
            edge.to = to;
            edge.conditions = edgeConditions;
            edges.Add(edge);
            AssetDatabase.AddObjectToAsset(edge, this);
        }

        private void Transition(
            string from,
            string to,
            float minTime,
            float minLoops,
            params (string, Comparator, float, bool)[][] conditions
            ) {
            List<AnimConditions> edgeConditions = conditions.Reduce((ors, or) =>
                ors.With(or.Reduce((ands, and) =>
                    ands.With(new AnimCondition() {
                        prop = and.Item1,
                        comparator = and.Item2,
                        floatValue = and.Item3,
                        boolValue = and.Item4,
                    }).With(new AnimCondition() {
                        prop = "timeInState",
                        comparator = Comparator.GTE,
                        floatValue = minTime,
                    }).With(new AnimCondition() {
                        prop = "loops",
                        comparator = Comparator.GTE,
                        floatValue = minLoops,
                    }),
                    new AnimConditions())),
                new List<AnimConditions>()); ;

            PlayerAnimSOEdge edge = CreateInstance<PlayerAnimSOEdge>();
            edge.from = from;
            edge.to = to;
            edge.conditions = edgeConditions;
            edges.Add(edge);
            AssetDatabase.AddObjectToAsset(edge, this);
        }

        private bool PropMatches(object record, string prop, Comparator comparator, float floatValue, bool boolValue) {
            IDictionary<string, PropertyInfo> props = record
                .GetType()
                .GetProperties()
                .Reduce(
                    (m, prop) => { m.Add(prop.Name, prop); return m; },
                    new Dictionary<string, PropertyInfo>());

            object propValue = props[prop].GetValue(record);
            Debug.Log("PROPVALUE" + propValue);

            int comparison = propValue switch {
                //null => -2,
                bool b => b.CompareTo(boolValue),
                IComparable c => floatValue.CompareTo(Convert.ChangeType(c, typeof(float))),
                _ => -2,
            };

            if (comparison == -2) return false;

            return comparator switch {
                Comparator.EQ => comparison == 0,
                Comparator.NE => comparison != 0,
                Comparator.GT => comparison < 0,
                Comparator.LT => comparison > 0,
                Comparator.GTE => comparison <= 0,
                Comparator.LTE => comparison >= 0,
                _ => false,
            };
        }

        private Predicate<PlayerAnimState> ToPred(List<AnimConditions> conditions) {
            return state => {
                foreach (AnimConditions group in conditions) {
                    bool all = true;
                    foreach (AnimCondition condition in group) {
                        if (!PropMatches(state, condition.prop, condition.comparator, condition.floatValue, condition.boolValue)) {
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
