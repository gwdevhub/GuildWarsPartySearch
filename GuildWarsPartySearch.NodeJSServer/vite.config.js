import { defineConfig } from 'vite'
import * as path from "path";
import * as fs from "fs";

// https://vitejs.dev/config/
export default defineConfig({
    build: {
        minify: false,
        emptyOutDir: true,
        rollupOptions: {
            output: {
                entryFileNames: `assets/[name]-${new Date().getTime()}.js`,
                chunkFileNames: `assets/[name]-${new Date().getTime()}.js`,
                assetFileNames: `assets/[name]-${new Date().getTime()}.[ext]`
            },
        },
    },
    plugins: [
        {
            name: 'postbuild-commands', // the name of your custom plugin. Could be anything.
            closeBundle: async () => {
                ['tiles','resources','cards'].forEach((folder) => {
                    const from = path.join(__dirname,'src',folder);
                    const to = path.join(__dirname,'dist',folder);
                    if(!fs.existsSync(to))
                        fs.cpSync(from, to, {recursive: true});
                });
            }
        },
    ]
})