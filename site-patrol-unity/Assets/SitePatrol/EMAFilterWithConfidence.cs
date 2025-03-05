using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SitePatrol
{
    /// <summary>
    /// 带置信度判断的 EMA 滤波器，用于平滑位置和旋转
    /// 当采样数小于 5 或者采样数据太分散时，IsConfident 为 false
    /// </summary>
    public class EMAFilterWithConfidence
    {
        private Vector3 filteredPosition;
        private Quaternion filteredRotation;
        private bool initialized = false;
        private readonly float alpha; // 平滑系数，范围 0 到 1

        // 用于判断数据分散情况的采样缓存（这里只处理位置）
        private List<Vector3> samples = new List<Vector3>();
        private int sampleCount = 0;
        private readonly int minSamples = 5; // 最小采样数量要求
        private readonly int maxBufferSize = 20; // 用于计算分散度的最大缓存数量

        // 分散程度阈值，单位与位置单位一致，超过这个值则认为数据太分散
        private readonly float dispersionThreshold;

        /// <summary>
        /// 当前数据是否可信：
        /// - 当进入滤波的数据数量 < 5 或者数据太分散时，认为数据不可用（false）
        /// </summary>
        public bool IsConfident { get; private set; } = false;

        /// <summary>
        /// 构造函数
        /// smoothingFactor：平滑系数，数值越大表示新数据权重越高
        /// dispersionThreshold：位置数据的标准差阈值，超过该值则认为数据分散
        /// </summary>
        public EMAFilterWithConfidence(float smoothingFactor, float dispersionThreshold = 0.1f)
        {
            alpha = Mathf.Clamp01(smoothingFactor);
            this.dispersionThreshold = dispersionThreshold;
        }

        /// <summary>
        /// 更新滤波器状态，传入新检测到的位姿数据（位置和旋转）
        /// </summary>
        public void Update(Vector3 newPosition, Quaternion newRotation)
        {
            // 更新 EMA 计算（位置和旋转）
            if (!initialized)
            {
                filteredPosition = newPosition;
                filteredRotation = newRotation;
                initialized = true;
            }
            else
            {
                filteredPosition = alpha * newPosition + (1 - alpha) * filteredPosition;
                filteredRotation = Quaternion.Slerp(filteredRotation, newRotation, alpha);
            }

            // 将新位置加入采样缓存，并记录采样数量
            samples.Add(newPosition);
            sampleCount++;

            // 限制缓存大小，保持最近 maxBufferSize 个样本
            if (samples.Count > maxBufferSize)
            {
                samples.RemoveAt(0);
            }

            // 判断置信度：
            // 1. 如果样本数不足 minSamples，则不可信
            // 2. 否则，计算样本的标准差，若标准差超过 dispersionThreshold，则认为数据分散，不可信
            if (sampleCount < minSamples)
                IsConfident = false;
            else
            {
                var mean = samples.Aggregate(Vector3.zero, (current, pos) => current + pos);

                mean /= samples.Count;

                var sumSq = samples.Sum(pos => (pos - mean).sqrMagnitude);

                var stdDev = Mathf.Sqrt(sumSq / samples.Count);
                IsConfident = stdDev <= dispersionThreshold;
            }
        }

        public Vector3 GetFilteredPosition()
        {
            return filteredPosition;
        }

        public Quaternion GetFilteredRotation()
        {
            return filteredRotation;
        }
    }
}