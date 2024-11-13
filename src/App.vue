<template>
  <switchable-store-provider>
    <the-toast-section />
    <div
      :class="[
        'app-layout',
        {
          'contactmoment-loopt': contactmomentStore.contactmomentLoopt,
          'hide-sidebar': route.meta.hideSidebar,
        },
      ]"
    >
      <the-header />
      <the-sidebar />
      <main>
        <router-view>
          <template #default="{ Component }">
            <back-link-provider>
              <component :is="Component" />
              <div class="versienummer">Versie {{ versienummer }}</div>
            </back-link-provider>
          </template>
        </router-view>
      </main>
    </div>
  </switchable-store-provider>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { RouterView, useRoute } from "vue-router";
import { useContactmomentStore } from "@/stores/contactmoment";
import TheToastSection from "@/components/TheToastSection.vue";
import TheSidebar from "./layout/TheSidebar.vue";
import TheHeader from "./layout/TheHeader.vue";
import { SwitchableStoreProvider } from "./stores/switchable-store";
import BackLinkProvider from "./components/BackLinkProvider.vue";

const contactmomentStore = useContactmomentStore();
const route = useRoute();
const versienummer = ref("Laden...");

onMounted(async () => {
  try {
    const response = await fetch("/api/environment/versienummer");
    if (response.ok) {
      const data = await response.json();
      versienummer.value = data.versienummer;
    }
  } catch (error) {
    versienummer.value = "Fout bij het laden van het versienummer";
  }
});
</script>

<style lang="scss">
/* Design Tokens */
@import "@utrecht/component-library-css";
@import "@/assets/design-tokens";
@import "@/assets/fonts/fonts.css";
@import "@/assets/reset.css";
@import "@/assets/main";
</style>
