import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
var srcPath = decodeURIComponent(new URL("./src", import.meta.url).pathname);
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: [{ find: "@", replacement: srcPath }],
    },
    server: {
        host: "0.0.0.0",
        port: 5173,
    },
});
