import { fileURLToPath, URL } from "url";
import { defineConfig, type ProxyOptions, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import basicSsl from "@vitejs/plugin-basic-ssl";

const getProxy = (
  env: Record<string, string>
): Record<string, ProxyOptions> | undefined => {
  const targetPort = env?.BFF_SSL_PORT;
  if (!targetPort) return undefined;
  const redirectOpts = {
    target: `https://localhost:${targetPort}`,
    secure: false,
  };
  return {
    "/api": redirectOpts,
    "/signin-oidc": redirectOpts,
    "/signout-callback-oidc": redirectOpts,
  };
};

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const proxy = getProxy(env);
  return {
    plugins: [vue(), basicSsl()],
    server: {
      port: 3000,
      proxy,
    },
    resolve: {
      alias: {
        "@": fileURLToPath(new URL("./src", import.meta.url)),
      },
    },
    build: {
      assetsInlineLimit: 0,
    },
  };
});
