using static Cinemachine.CinemachineTargetGroup;
using UnityEngine;
using Cinemachine;
using System;

namespace Ashes
{
    public class TargetGroupMixer : MonoBehaviour
    {
        [SerializeField] private LeanTweenType _leanType;
        private CinemachineTargetGroup _targets;

        private void Awake()
        {
            _targets = GetComponent<CinemachineTargetGroup>();
        }

        public void WeightTop() => LeanFlag(TargetFlag.TOP);
        public void WeightHex() => LeanFlag(TargetFlag.HEX);
        public void WeightLeft() => LeanFlag(TargetFlag.LEFT);
        public void WeightRight() => LeanFlag(TargetFlag.RIGHT);

        public void LeanFlag(TargetFlag flag)
        {
            for(int i = 0; i < _targets.m_Targets.Length; i++)
            {
                int weight = flag.HasFlag((TargetFlag)(1 << i)) ? 1 : 0;
                LeanWeight(i, weight);
            }
        }
        
        private void LeanWeight(int index, float finalWeight)
        {
            Target target = _targets.m_Targets[index];

            if (target.weight == finalWeight) return;

            LeanTween.value(target.weight, finalWeight, 1F).setOnUpdate((a) =>
            {
                _targets.m_Targets[index] = new Target()
                {
                    target = target.target, 
                    weight = a
                };
            }).setEase(_leanType);
        }
    }

    [Flags]
    public enum TargetFlag
    {
        NONE    = 0,
        TOP     = 1,
        HEX     = 1 << 1,
        LEFT    = 1 << 2,
        RIGHT   = 1 << 3
    }
}