using OSCore.Data.Enums;
using OSCore.System.Interfaces.Tagging;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSBE.Tagging {
    public class TagRegistry : ITagRegistry {
        private readonly IDictionary<IdTag, GameObject> uniqueTags;
        private readonly IDictionary<IdTag, ISet<GameObject>> tags;

        public TagRegistry() {
            uniqueTags = new Dictionary<IdTag, GameObject>();
            tags = new Dictionary<IdTag, ISet<GameObject>>();
        }

        public ISet<GameObject> Get(IdTag tag) =>
            tags.Get(tag, new HashSet<GameObject>());

        public GameObject GetUnique(IdTag tag) =>
            uniqueTags.Get(tag, null);

        public void Register(IdTag tag, GameObject obj) {
            tags.Update(tag,
                set => Colls.With(set, obj),
                () => new HashSet<GameObject>());
        }

        public void RegisterUnique(IdTag tag, GameObject obj) {
            if (uniqueTags.ContainsKey(tag))  {
                throw new Exception("Unique game object already registered");
            }
            uniqueTags.Add(tag, obj);
        }
    }
}
