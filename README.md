# SitePatrol

**A real-time construction and facility management inspection solution integrating SLAM, BIM, and Visual Language Models.**
## Background and Motivation

During construction and maintenance, onsite inspection often relies on personnel visually detecting and documenting defects or hazards. While **Building Information Modeling (BIM)** provides a rich 3D representation for projects, mapping issues found onsite directly to their BIM locations can be cumbersome.

**Challenges with traditional inspection**:
1. **High skill requirements** – Inspectors must be experienced and knowledgeable to spot issues.
2. **Difficult to manage** – Supervisors have limited visibility into whether inspectors followed a predefined route or overlooked areas.
3. **Inaccurate positioning** – Relying on text and images makes it hard to convey the precise location of problems.
4. **No continuous tracking** – If an issue is not accurately located, it’s difficult to find and track it in subsequent inspections.

**Existing approaches** such as video-based inspection systems (e.g., OpenSpace) still face problems like:
- **Delayed processing** – Real-time feedback is lacking because videos are analyzed only after upload.
- **Complex calibration** – Linking SLAM routes to BIM coordinates often involves manual alignment.
- **Lack of automated detection** – Inspectors still perform visual checks on captured videos.

## Challenges

1. **Real-time SLAM**: We need a robust system that can localize the camera in real time during inspection.  
2. **Seamless BIM alignment**: Automatically align the SLAM coordinate system with the BIM coordinate system.  
3. **Automated issue detection**: Use state-of-the-art computer vision or Visual Language Models (VLM) to pinpoint defects or hazards on the spot.

## Solution Overview

### Core Ideas
1. **VLM-Assisted Detection**: Use Vision-Language Models (VLMs) to automatically identify problems and lower the expertise barrier for inspectors.  
2. **SLAM & AR**: Leverage **AR Foundation** (Unity3D’s wrapper for Apple’s ARKit and Google’s ARCore) to track device position in real time.  
3. **Mixed Reality Markers**: Highlight detected issues in real-world AR space, while also mapping them to the BIM model.  
4. **Ongoing Issue Tracking**: Cluster and tag similar or nearby defects so they can be tracked over time.

### High-Level Approach
- **SLAM**: Use AR Foundation (ARKit/ARCore) to get reliable, real-time device poses.
- **AprilTag-based Coordinate Alignment**: Attach AprilTags onsite. Detect these markers to link AR/SLAM coordinates to BIM coordinates.
- **Visual Language Models**: Employ large-model-based defect detection (e.g., Qwen 2.5 VL) to analyze images and find bounding boxes for potential issues.
- **EMA Filtering**: Smoothen results from AprilTag detection to reduce noise in position/orientation data.

## Key Components

### SLAM Localization
- Originally experimented with **VINS-Mono** for offline localization using iPhone’s IMU and video data.
- Moved to **ARKit** (through Unity3D’s AR Foundation) due to its robust, real-time performance and ease of use on iOS (and cross-platform support via ARCore).

### AprilTag Detection
- **AprilTag** markers are used to establish absolute references.  
- Achieved real-time detection in Unity3D, integrated with AR Foundation for continuous updates on each marker’s position relative to the camera.

### BIM-SLAM Coordinate Alignment
1. From AR Foundation, obtain **camera pose in AR coordinate space**.
2. Use AprilTag detection to get **marker pose in the camera coordinate space**.
3. Use known **marker pose in the BIM coordinate space**.
4. Combine these transforms so that:
   \[
   T_{\text{camera}}^{\text{BIM}} = T_{\text{camera}}^{\text{AR}} \times T_{\text{AR}}^{\text{AprilTag}} \times T_{\text{AprilTag}}^{\text{BIM}}
   \]
5. For multiple tags:
   - Use the nearest visible AprilTag for positional alignment.
   - Use another reliable (farther) AprilTag to refine orientation.
   - Apply an EMA filter for smoother updates.

### Issue Detection & Tracking
- Tested with **GPT-4o** and **Qwen 2.5 VL** for vision-based defect detection.  
- **Qwen 2.5 VL** demonstrated better bounding-box precision.  
- Once a bounding box is found in 2D, cast a ray into the AR space to determine a 3D location of the defect for BIM alignment.  
- Cluster similar or nearby 3D points to track recurring or persistent issues.

## MVP Architecture

### Web Frontend
- **Vue3** for UI/UX.  
- **Unity** (WebGL build) for 3D model visualization and editing.  
- Key features:
  1. **Project management** – Create and manage multiple projects.  
  2. **Model upload** – Support for GLTF or similar formats.  
  3. **AprilTag management** – Visually edit marker positions in Unity.  
  4. **Live tracking** – Monitor inspector locations and detected issues in real time in a 3D view.

### Web Backend
- Provides APIs for data storage, issue management, and user authentication.  
- **WebSocket** for real-time synchronization between multiple inspectors and the web interface.  
- Exposes endpoints for both the **mobile app** and **web frontends** to share and retrieve data.

### Mobile App
- Built with **Unity + AR Foundation**.  
- Determines device location in real time and aligns with BIM using AprilTags.  
- Two primary functions:
  1. **Automated detection** – Capture the camera feed, pass it to **Qwen 2.5 VL**, retrieve bounding boxes of issues, and map them to 3D coordinates.  
  2. **Manual logging** – Manually create an issue by taking a snapshot and adding notes; the app calculates and stores the corresponding 3D location.  
- Continuously sends the user’s position and newly found issues to the server.

## Roadmap
- **Phase 1**: Basic MVP with AR localization, manual issue logging, and simple 3D coordinate mapping.  
- **Phase 2**: Integrate automated defect detection (Qwen 2.5 VL) and real-time bounding box to BIM mapping.  
- **Phase 3**: Enhanced multi-user collaboration, advanced clustering of similar defects, and in-depth analytics.  
- **Phase 4**: Broader device support (e.g., Android, AR-capable helmets, robots) and improved offline capabilities.

## License
This project is licensed under the [MIT License](LICENSE) — or any license you prefer to include.

Contributions are welcome! Please open an issue or submit a pull request if you’d like to help.
