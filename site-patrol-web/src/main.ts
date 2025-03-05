import { createApp } from "vue";
import App from "./App.vue";
import axios from "axios";
import { serviceOptions } from "./api";

serviceOptions.axios = axios.create();
const app = createApp(App);
app.mount("#app");
