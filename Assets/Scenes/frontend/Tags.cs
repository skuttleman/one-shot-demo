using OSCore.Data.Enums;
using OSCore.System.Interfaces.Tagging;
using OSCore;
using System;
using UnityEngine;

namespace OSFE {
    public class Tags : MonoBehaviour {
        [SerializeField] Tag[] tags;

        void OnEnable() {

            FindObjectOfType<GameController>()
                .Send<ITagRegistry>(registry => {
                    foreach (Tag tag in tags)
                        if (tag.isUnique) registry.RegisterUnique(tag.tag, gameObject);
                        else registry.Register(tag.tag, gameObject);
                });
        }

        [Serializable]
        public struct Tag {
            public IdTag tag;
            public bool isUnique;
        }
    }
}
