import { codegen } from "swagger-axios-codegen";
import axios from "axios";

axios.get("http://localhost:8000/openapi.json").then((resp) => {
  codegen({
    source: resp.data,
    useStaticMethod: true,
    outputDir: "./src/api",
    modelMode: "class",
  });
});
