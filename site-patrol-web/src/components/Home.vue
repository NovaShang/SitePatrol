<template>
  <div class="hero" id="hero">
    <div class="mask"></div>
    <div class="slogan">Welcome to SitePatrol</div>
    <div>Improved construction site inspections with AR and AI.</div>
    <el-button class="go" type="primary" size="large" @click="scrollBottom"
      >Upload Your Model and Start!
    </el-button>
    <div style="opacity: 0.7">
      If you have previous uploaded model, please use previous url.
    </div>
  </div>
  <div id="features">
    <div class="feature">
      <img class="feature-icon" alt="icon-1" src="../assets/icon1.svg" />
      <div class="feature-name">Real-time Positioning In Model</div>
    </div>
    <div class="feature">
      <img class="feature-icon" alt="icon-1" src="../assets/icon2.svg" />
      <div class="feature-name">Track Issues Across Patrols</div>
    </div>
    <div class="feature">
      <img class="feature-icon" alt="icon-1" src="../assets/icon3.svg" />
      <div class="feature-name">Discover Issues With AI</div>
    </div>
  </div>
  <div id="background" class="section odd">
    <div class="sub-header">Background</div>
    <p>
      Construction sites are busy, complex environments where even the smallest
      oversight can lead to significant problems. Traditionally, engineers and
      supervisors have had to patrol the site manually, visually inspecting for
      issues such as cracks, water leakage, or uneven surfaces. This method is
      not only time-consuming but also prone to human error. Imagine if every
      defect could be automatically detected and instantly highlighted on a 3D
      model—making it easier for everyone to track and address issues
      efficiently.
    </p>
  </div>
  <div id="concept" class="section even">
    <div class="sub-header">Core Concept</div>
    <p>At the heart of SitePatrol is the fusion of advanced technologies:</p>

    <ul>
      <li class="item">
        <span class="bold"
          >Real-time SLAM (Simultaneous Localization and Mapping):</span
        >
        Our system continuously calculates the camera's position on the
        construction site.
      </li>
      <li class="item">
        <span class="bold">VLM-Based Issue Detection:</span> Instead of
        real-time defect detection, we use a pre-trained Visual Large Model
        (VLM) to analyze captured images for quality defects and safety hazards.
        This powerful model processes the images and identifies potential issues
        accurately.
      </li>
      <li class="item">
        <span class="bold">User-Driven Markup:</span> Users have the flexibility
        to manually mark any issues they observe, ensuring that even subtle
        defects can be flagged and tracked.
      </li>
      <li class="item">
        <span class="bold">Coordinate Mapping with AprilTags:</span> We use
        AprilTags to establish a precise relationship between the camera’s
        coordinate system and the BIM (Building Information Modeling) system.
      </li>
      <li class="item">
        <span class="bold">3D Visualization:</span> Detected and marked issues
        are transformed into 3D coordinates and overlaid on your BIM model,
        offering an engaging, interactive view of your site's condition.
      </li>
    </ul>
    <p>
      This integration of SLAM, VLM-driven analysis, and BIM data creates a
      seamless workflow—from data capture to actionable insights.
    </p>
  </div>
  <div id="tech" class="section odd">
    <div class="sub-header">Technical Overview</div>

    At the core of SitePatrol’s innovative approach is a blend of cutting-edge
    AR technology and precise visual marker detection:

    <div class="subtitle">
      <span class="bold">SLAM Positioning with ARKit:</span>
    </div>
    <p>
      Utilizing AR Foundation, our system taps into Apple’s ARKit to achieve
      robust SLAM-based positioning. This enables accurate tracking of the
      device’s movements on-site, ensuring that every captured image is
      precisely anchored in space.
    </p>

    <div class="subtitle">
      <span class="bold">Visual Marker Detection via AprilTag:</span>
    </div>
    <p>
      Our integrated AprilTag Detector identifies visual markers throughout the
      environment. A dedicated web editor lets users pre-calibrate the positions
      of these AprilTags within the 3D model, setting a reliable foundation for
      subsequent coordinate mapping.
    </p>

    <div class="subtitle">
      <span class="bold">Flexible Tag Modes for Coordinate Mapping:</span>
    </div>
    <p>We offer two positioning modes:</p>
    <ul>
      <li class="item">
        Single-Tag Mode: When only one tag is available, its position and
        orientation are used to establish a direct coordinate mapping between
        the AR session and the BIM model.
      </li>
      <li class="item">
        Multi-Tag Mode: In scenarios where two or more tags are detected (not
        necessarily captured simultaneously), our algorithm disregards
        orientation data. Instead, it harnesses the spatial distribution of
        multiple tags to build a more resilient coordinate matching
        relationship.
      </li>
    </ul>

    <div class="subtitle">
      <span class="bold">Stabilized Mapping with EMAFilter:</span>
    </div>

    <p>
      To ensure that the coordinate mapping remains stable, we apply an
      Exponential Moving Average Filter (EMAFilter) to both the detected
      AprilTag positions within the AR session and the computed offsets between
      the two coordinate systems. This smoothing process significantly enhances
      the reliability of the mapping, even in dynamic or noisy environments.
      This technical strategy underpins SitePatrol’s ability to deliver accurate
      and stable 3D tracking, seamlessly integrating on-site data with the BIM
      model for a truly immersive inspection experience.
    </p>
  </div>

  <div id="gltf" class="section even">
    <div class="sub-header">GLTF File Upload</div>

    <p>To get started, simply click the button below to upload a GLTF file.</p>

    <el-upload
      class="upload-demo"
      drag
      action="/api/v1/file/upload"
      :limit="1"
      accept=".gltf"
      v-model:file-list="fileList"
    >
      <el-icon class="el-icon--upload">
        <upload-filled />
      </el-icon>
      <div class="el-upload__text">
        Drop file here or <em>click to upload</em>
      </div>
      <template #tip>
        <div class="el-upload__tip">glTF file, less than 50MB</div>
      </template>
    </el-upload>

    <el-button type="primary" size="large" @click="start">Start!</el-button>

    <div class="subtitle">What is a GLTF File?</div>
    <p>
      GLTF (GL Transmission Format) is a cutting-edge 3D file format optimized
      for fast loading and efficient rendering on the web. It's perfect for
      displaying complex 3D models in an interactive and lightweight manner.
    </p>

    <div class="subtitle">How to Create a GLTF File:</div>

    <ul>
      <li>
        <div class="item">
          <span class="bold">Using an iPhone 3D Scanner App:</span>
        </div>
        <div>
          Modern 3D Scanner apps on the iPhone allow you to capture detailed
          scans of your environment. Once scanned, you can export your scene
          directly as a GLTF file, ensuring it’s ready for use in our system.
        </div>
      </li>
      <li>
        <div class="item"><span class="bold">From BIM Models:</span></div>
        <div>
          If you already have a BIM model, export it in FBX format from your BIM
          software. Then, using one of the available conversion tools, convert
          the FBX file into a GLTF file. This process optimizes your model for
          web display without sacrificing detail. By following these steps, your
          3D content will be perfectly prepared for the SitePatrol experience.
        </div>
      </li>
    </ul>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { UploadFilled } from "@element-plus/icons-vue";
import { ModelFilesService } from "../api";
import type { UploadUserFile } from "element-plus";

const fileList = ref<UploadUserFile[]>([]);

function scrollBottom() {
  window.scrollTo({
    top: document.documentElement.scrollHeight,
  });
}

const showPreviousDialog = ref(false);

// When the page is loaded, check localStorage for an existing model_file_id
onMounted(() => {
  const modelFileId = localStorage.getItem("model_file_id");
  if (modelFileId) {
    const result = confirm(
      " You have already uploaded a model file. Would you like to return to the previous model?",
    );
    if (result) navigateToUnityPage(modelFileId);
    else localStorage.removeItem("model_file_id");
  }
});

// Function to navigate to page2 (the Unity WebGL page) with the model_file_id as a URL parameter.
const navigateToUnityPage = (modelFileId: string) => {
  // Update the URL to your actual Unity WebGL page address.
  window.location.href = `/unity?model_file_id=${modelFileId}`;
};

const start = async () => {
  if (fileList.value.length == 0 || fileList.value[0].status != "success"){
    alert("Please upload gltf model first.")
    return;
  }
  let fileUrl: string = fileList.value[0].response as any;
  let model = await ModelFilesService.createModelFileApiV1ModelFilesPost({
    body: {
      file_url: fileUrl,
    },
  });
  localStorage.setItem("model_file_id", model.id);
  navigateToUnityPage(model.id);
};
</script>

<style scoped>
.hero {
  height: 500px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background-color: #333333;
  color: white;
  margin-bottom: 20px;
  background-image: url("../assets/hero.jpg");
  background-position: center;
  background-size: cover;
  position: relative;
}

.hero > * {
  z-index: 1;
}

.hero .mask {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #00000088;
  z-index: 0;
}

.hero .slogan {
  font-weight: bold;
  font-size: 1.5em;
  margin: 20px;
}

.hero .go {
  margin: 20px;
}

#features {
  display: flex;
  flex-direction: row;
}

.feature {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 10px;
  flex-grow: 1;
  padding: 20px;
  border-radius: 5px;
  border: 1px solid #ccc;
  margin: 0px 10px;
  font-weight: bold;
  color: #666;
  font-size: 1.2em;
}

.feature .feature-icon {
  height: 40px;
  margin-bottom: 20px;
}

.section {
  padding: 20px 40px;
}

.section.odd {
  background-color: white;
}

.section.even {
  background-color: rgba(110, 145, 160, 0.2);
}

.bold {
  font-weight: bold;
}

.subtitle {
  font-weight: bold;
  margin-top: 15px;
  font-size: 1.2em;
}

.sub-header {
  font-size: 1.5em;
  font-weight: bold;
  color: #08f;
  margin-bottom: 10px;
  margin-top: 10px;
}
</style>
