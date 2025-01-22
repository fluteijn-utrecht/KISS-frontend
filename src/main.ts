import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import { createPinia } from "pinia";
import { useIntersectionObserver } from "@vueuse/core";

// warning if closing tab or refreshing
if (import.meta.env.PROD) {
  addEventListener(
    "beforeunload",
    (e) => {
      e.preventDefault();
      return (e.returnValue = "");
    },
    { capture: true },
  );
}

async function checkBackendStatus() {
  try {
    // Controleer de backend status via de status endpoint
    const response = await fetch("/api/environment/status");
    if (response.ok) {
      const { status } = await response.json();
      if (status !== "OK") {
        alert(`Probleem met de backend-configuratie: ${status}`);
      }
    } else {
      alert("Fout bij het ophalen van de backend-status.");
    }
  } catch (error) {
    alert("Er is een probleem bij de verbinding met de backend.");
  }
}

async function initializeApp() {
  await checkBackendStatus();

  const app = createApp(App);

  app.use(createPinia());
  app.use(router);

  // Register a global custom directive called `v-focus`
  app.directive("focus", {
    mounted(el) {
      const { stop } = useIntersectionObserver(el, (entries) => {
        entries.forEach((x) => {
          if (x.intersectionRatio > 0 && x.target instanceof HTMLElement) {
            x.target.focus();
          }
        });
      });
      el.stop = stop;
    },
    unmounted(el) {
      el.stop();
    },
  });

  app.mount("#app");
}

initializeApp();
