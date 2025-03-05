using Unity.Collections;
using Unity.Jobs;
using System;
using System.Collections.Generic;
using UnityEngine;
using Color32 = UnityEngine.Color32;

namespace AprilTag
{
//
// Multithreaded tag detector and pose estimator
//
    public sealed class TagDetector : System.IDisposable
    {
        #region Public properties

        public IEnumerable<TagPose> DetectedTags
            => _detectedTags;

        public IEnumerable<(string name, long time)> ProfileData
            => _profileData ?? (_profileData = GenerateProfileData());

        #endregion

        #region Constructor

        public TagDetector(int width, int height, int decimation = 2)
        {
            // Object creation
            Detector = Interop.Detector.Create();
            Family = Interop.Family.CreateTagStandard41h12();
            Image = Interop.ImageU8.Create(width, height);

            // Detector configuration
            Detector.ThreadCount = SystemConfig.PreferredThreadCount;
            Detector.QuadDecimate = decimation;
            Detector.AddFamily(Family);
        }

        #endregion

        #region Public methods

        public void Dispose()
        {
            Detector?.RemoveFamily(Family);
            Detector?.Dispose();
            Family?.Dispose();
            Image?.Dispose();

            Detector = null;
            Family = null;
            Image = null;
        }

        public void ProcessImage(ReadOnlySpan<Color32> image, float fov, float tagSize)
        {
            ImageConverter.Convert(image, Image);
            RunDetectorAndEstimator(fov, tagSize);
        }

        #endregion


        public Interop.Detector Detector;
        public Interop.Family Family;
        public Interop.ImageU8 Image;

        List<TagPose> _detectedTags = new List<TagPose>();
        List<(string, long)> _profileData;


        #region Detection/estimation procedure

        //
        // We can simply use the multithreaded AprilTag detector for tag detection.
        //
        // In contrast, AprilTag only provides single-threaded pose estimator, so
        // we have to manage threading ourselves.
        //
        // We don't want to spawn extra threads just for it, so we run them on
        // Unity's job system. It's a bit complicated due to "impedance mismatch"
        // things (unmanaged vs managed vs Unity DOTS).
        //
        void RunDetectorAndEstimator(float fov, float tagSize)
        {
            _profileData = null;

            // Run the AprilTag detector.
            using var tags = Detector.Detect(Image);
            var tagCount = tags.Length;
            
            // Convert the detector output into a NativeArray to make them
            // accessible from the pose estimation job.
            using var jobInput = new NativeArray<PoseEstimationJob.Input>
                (tagCount, Allocator.TempJob);

            var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);

            for (var i = 0; i < tagCount; i++)
                slice[i] = new PoseEstimationJob.Input(ref tags[i]);

            // Pose estimation output buffer
            using var jobOutput
                = new NativeArray<TagPose>(tagCount, Allocator.TempJob);

            // Pose estimation job
            var job = new PoseEstimationJob
                (jobInput, jobOutput, Image.Width, Image.Height, fov, tagSize);

            // Run and wait the jobs.
            job.Schedule(tagCount, 1, default(JobHandle)).Complete();

            // Job output -> managed list
            jobOutput.CopyTo(_detectedTags);
        }

        #endregion

        #region Profile data aggregation

        List<(string, long)> GenerateProfileData()
        {
            var list = new List<(string, long)>();
            var stamps = Detector.TimeProfile.Stamps;
            var time = Detector.TimeProfile.UTime;
            for (var i = 0; i < stamps.Length; i++)
            {
                var stamp = stamps[i];
                list.Add((stamp.Name, stamp.UTime - time));
                time = stamp.UTime;
            }

            return list;
        }

        #endregion
    }
} // namespace AprilTag