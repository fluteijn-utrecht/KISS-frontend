import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import { createPinia } from "pinia";
import { useIntersectionObserver } from "@vueuse/core";

declare global {
  interface Window {
    organisatieIds: string[];
    pubBeheerUrl: string;
    gatewayBaseUri: string;
  }
}

// HACK VOOR RARE ORGANISATIE IDS
try {
  window.organisatieIds = window.organisatieIds.map((x) =>
    Number.parseFloat(x).toFixed(0)
  );
} catch (error) {
  console.error(error);
}

const app = createApp(App);

app.use(createPinia());
app.use(router);

// Register a global custom directive called `v-focus`
app.directive("focus", {
  // When the bound element is mounted into the DOM...
  mounted(el) {
    // start observing whenever the element becomes visible
    const { stop } = useIntersectionObserver(el, (entries) => {
      entries.forEach((x) => {
        if (x.intersectionRatio > 0 && x.target instanceof HTMLElement) {
          // Focus the element
          x.target.focus();
        }
      });
    });
    el.stop = stop;
  },
  // when the bound element is unmounted from the DOM...
  unmounted(el) {
    // stop observing the element
    el.stop();
  },
});

app.mount("#app");
