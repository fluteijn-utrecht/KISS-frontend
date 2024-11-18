<template>
  <div v-if="seedLoading">
    <simple-spinner />
  </div>

  <application-message v-else-if="canSeed" messageType="warning">
    <p>
      Wilt u KISS vullen met voorbeelddata voor Gespreksresultaten, Skills,
      Nieuws en Werkinstructies en Links?
    </p>

    <button type="button" class="utrecht-button start-button" @click="seedData">
      Voorbeelddata aanmaken
    </button>
  </application-message>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { toast } from "@/stores/toast";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { useCurrentUser } from "@/features/login";
import { useLoader } from "@/services/use-loader";

const seedLoading = ref(false);

const user = useCurrentUser();
const isRedacteur = computed(
  () => user.success && user.data.isLoggedIn && user.data.isRedacteur,
);

const seedData = async () => {
  seedLoading.value = true;

  const { ok } = await fetch("/api/seed/start", { method: "POST" });

  if (ok) {
    window.location.reload();
  } else {
    seedLoading.value = false;
    toast({
      text: "Er is een fout opgetreden bij het vullen van KISS met voorbeelddata.",
      type: "error",
    });
  }
};

const { data: canSeed } = useLoader(() => {
  if (isRedacteur.value) {
    return fetch("/api/seed/check").then((r) => r.ok);
  }
});
</script>

<style lang="scss" scoped>
article {
  display: flex;
  column-gap: var(--spacing-default);
  align-items: flex-start;
  justify-content: space-between;
}

p {
  color: var(--color-white);
  margin-block-end: var(--spacing-default);
  flex: 1;
}

.start-button {
  --utrecht-button-background-color: var(--color-white);
  --utrecht-button-color: var(--color-accent-text);
}
</style>
