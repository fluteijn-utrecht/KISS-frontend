<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/Beheer/NieuwsEnWerkinstructies">
      Nieuws en werkinstructies
    </router-link>
    <router-link to="/Beheer/Skills">Skills</router-link>
    <router-link to="/Beheer/Links">Links</router-link>
    <router-link to="/Beheer/gespreksresultaten"
      >Gespreksresultaten</router-link
    >
    <router-link to="/Beheer/Kanalen">Kanalen</router-link>
    <router-link to="/Beheer/Contactverzoekformulieren">
      Formulieren contactverzoek
    </router-link>
  </nav>

  <main>
    <seed-beheer />

    <router-view />
  </main>

  <!-- Test div for "asdf" in the bottom right corner -->
  <div class="bottom-right">Build: {{ buildNumber }}</div>
</template>

<script setup lang="ts">
import { onMounted, ref, watchEffect } from "vue";
import { useRouter } from "vue-router";
import { useCurrentUser } from "@/features/login";
import SeedBeheer from "./seed/SeedBeheer.vue";

const user = useCurrentUser();
const router = useRouter();
const buildNumber = ref("Loading...");

onMounted(async () => {
  try {
    const response = await fetch("/api/environment/build-number");
    if (response.ok) {
      const data = await response.json();
      buildNumber.value = data.buildNumber;
    }
  } catch (error) {
    buildNumber.value = "Fout bij het laden van het build nummer";
  }
});

watchEffect(() => {
  if (user.success && user.data.isLoggedIn && !user.data.isRedacteur) {
    router.push("/");
  }
});
</script>

<style lang="scss" scoped>
nav {
  display: flex;
  gap: var(--spacing-default);
  margin-bottom: var(--spacing-large);
}

main {
  max-width: var(--section-width-large);
}

:deep(.listItem) {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-block: var(--spacing-small);
  border-bottom: 2px solid var(--color-tertiary);
}

:deep(menu) {
  margin-block-start: var(--spacing-default);
}

:deep(.header-wrapper) {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

:deep(a.utrecht-button) {
  text-decoration: none;
}

:deep(form) {
  max-inline-size: 40rem;
}

:deep(fieldset) {
  display: flex;
  flex-direction: column;

  > label {
    display: flex;
    gap: var(--spacing-small);
    align-items: baseline;

    > input {
      margin: 0;
      padding: 0;
      scale: 1.25;
      accent-color: var(--color-primary);
    }
  }
}

.bottom-right {
  position: fixed;
  bottom: 20px;
  right: 20px;
  background-color: #415a77;
  color: white;
  padding: 10px;
  border-radius: 5px;
  font-size: 14px;
}
</style>
