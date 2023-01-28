using System.Collections.Generic;
using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Flows {
    public class TransformPatrol : AStateNode {
        private AStateNode child;

        public TransformPatrol(Transform transform) : base(transform) { }

        protected override StateNodeStatus ProcessImpl() {
            return child.Process();
        }

        public override void Init() {
            List<AStateNode> nodes = new();

            Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .ForEach(xform => {
                    float waitTime = xform.localScale.z;
                    float rotation = xform.rotation.eulerAngles.y;

                    nodes.Add(new BNodeGoto(transform, xform.position));
                    if (waitTime > 0 && rotation >= 0) {
                        nodes.Add(new BNodeLookAtDirection(transform, Vectors.ToVector3(rotation)));
                    }
                    if (waitTime > 0) {
                        nodes.Add(new BNodeWait(transform, 0f));
                    }
                });

            child = new BNodeRepeat(transform, new BNodeAnd(transform, nodes.ToArray()));
            child.Init();
        }
    }
}