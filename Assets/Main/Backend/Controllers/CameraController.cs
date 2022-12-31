using System.Collections.Generic;
using Cinemachine;
using OSCore;
using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.ScriptableObjects.CameraAuxCfgSO;

namespace OSBE.Controllers {
    public class CameraController : ASystemInitializer<AnimationChanged> {
        [SerializeField] private CameraAuxCfgSO cfg;
        private CinemachineVirtualCamera[] cams;
        private bool isShaking = false;

        // Start is called before the first frame update
        private void Start() {
            system = FindObjectOfType<GameController>();
            cams = transform.parent.GetComponentsInChildren<CinemachineVirtualCamera>();
        }

        private IEnumerator<YieldInstruction> ShakeCamera(ShakeConfig cfg) {
            isShaking = true;
            float[] currentAmps = new float[cams.Length];
            float[] currentFreqs  = new float[cams.Length];

            for (int idx = 0; idx < cams.Length; idx++) {
                currentAmps[idx] = cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain;
                cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = cfg.amp;

                currentFreqs[idx] = cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain;
                cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = cfg.freq;
            }

            yield return new WaitForSeconds(cfg.duration);

            for (int idx = 0; idx < cams.Length; idx ++) {
                cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = currentAmps[idx];
                cams[idx].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = currentFreqs[idx];
            }

            isShaking = false;
        }

        protected override void OnEvent(AnimationChanged e) {
            if (!isShaking) {
                switch (e.curr) {
                    case PlayerAnim.crouch_fire:
                    case PlayerAnim.crawl_fire:
                        StartCoroutine(ShakeCamera(cfg.fire));
                        break;

                    case PlayerAnim.stand_punch:
                    case PlayerAnim.crouch_punch:
                    case PlayerAnim.crawl_punch:
                        StartCoroutine(ShakeCamera(cfg.punch));
                        break;

                    case PlayerAnim.stand_move:
                    case PlayerAnim.stand_idle:
                        if (e.prev == PlayerAnim.stand_fall) {
                            StartCoroutine(ShakeCamera(cfg.fallLand));
                        }
                        break;

                    case PlayerAnim.crawl_idle:
                        if (e.prev == PlayerAnim.crawl_dive) {
                            StartCoroutine(ShakeCamera(cfg.diveLand));
                        }
                        break;
                }
            }
        }
    }
}
