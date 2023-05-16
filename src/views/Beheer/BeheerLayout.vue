<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/Beheer/NieuwsEnWerkinstructies"
      >Nieuws en werkinstructies</router-link
    >
    <router-link to="/Beheer/Skills">Skills</router-link>
  </nav>
  <main>
    <router-view />
  </main>
</template>

<script setup lang="ts">
import { useCurrentUser } from "@/features/login";
import { watchEffect } from "vue";
import { useRouter } from "vue-router";

const user = useCurrentUser();
const router = useRouter();

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
}
main {
  max-width: var(--section-width-large);
}
</style>
