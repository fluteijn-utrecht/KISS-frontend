<template>
  <simple-spinner v-if="loading" />

  <application-message v-else-if="!populated" messageType="warning">
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
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { toast } from "@/stores/toast";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

const router = useRouter();

const populated = ref(true);
const loading = ref(false);

const seedData = async () => {
  loading.value = true;

  const { status } = await fetch("/api/seed/start", { method: "POST" }).finally(
    () => (loading.value = false),
  );

  if (status !== 200) {
    toast({
      text: "Er is een fout opgetreden bij het vullen van KISS met voorbeelddata.",
      type: "error",
    });

    seedCheck();
  } else {
    toast({
      text: "KISS is succesvol gevuld met voorbeelddata.",
    });

    populated.value = true;
  }

  router.push("/Beheer");
};

const seedCheck = async () => {
  const { status } = await fetch("/api/seed/check");

  populated.value = !(status === 200);
};

onMounted(() => seedCheck());
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
