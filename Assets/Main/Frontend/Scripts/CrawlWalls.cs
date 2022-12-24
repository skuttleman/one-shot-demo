using System.Collections.Generic;
using System;
using OSCore.Data.Enums;
using OSCore.System;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSFE.Scripts {
    public class CrawlWalls : ASystemInitializer<StanceChanged> {
        private PlayerStance stance;

        protected override void OnEvent(StanceChanged e) =>
            stance = e.stance;

        void Update() {
            foreach (Transform child in transform)
                child.gameObject.SetActive(stance == PlayerStance.CRAWLING);
        }
    }
}
