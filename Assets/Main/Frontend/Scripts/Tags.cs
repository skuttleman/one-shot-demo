using OSCore.Data.Enums;
using OSCore.System.Interfaces.Tagging;
using OSCore.System;
using System;
using UnityEngine;

namespace OSFE.Scripts {
    public class Tags : ASystemInitializer {
        [SerializeField] private Tag[] tags;

        protected override void OnEnable() {
            base.OnEnable();
            system.Send<ITagRegistry>(registry => {
                foreach (Tag tag in tags) {
                    if (tag.isUnique) registry.RegisterUnique(tag.tag, gameObject);
                    else registry.Register(tag.tag, gameObject);
                }
            });
        }

        [Serializable]
        private struct Tag {
            [field: SerializeField] public IdTag tag { get; private set; }
            [field: SerializeField] public bool isUnique { get; private set; }
        }
    }
}
