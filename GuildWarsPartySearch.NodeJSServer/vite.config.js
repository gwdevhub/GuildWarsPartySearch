import { defineConfig } from 'vite'
import * as path from "path";
import * as fs from "fs";

// https://vitejs.dev/config/
export default defineConfig({
    build: {
        minify: false,
        emptyOutDir: false
    },
    plugins: [
        {
            name: 'postbuild-commands', // the name of your custom plugin. Could be anything.
            closeBundle: async () => {
                ['tiles','resources','cards'].forEach((folder) => {
                    const from = path.join(__dirname,'src',folder);
                    if(!fs.existsSync(from))
                        fs.cpSync(from, path.join(__dirname,'dist',folder), {recursive: true});
                });
            }
        },
    ]
})