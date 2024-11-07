<template>
  <application-message v-if="!populated" messageType="warning">
    <span>
      Wilt u KISS vullen met voorbeelddata voor Gespreksresultaten, Skills,
      Nieuws en Werkinstructies en Links?
    </span>

    <button type="button" class="utrecht-button start-button" @click="seedData">
      Voorbeelddata aanmaken
    </button>
  </application-message>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { toast } from "@/stores/toast";
import ApplicationMessage from "../../../components/ApplicationMessage.vue";

const populated = ref(true);

const seedData = async () => {
  const { status } = await fetch("/api/seed/start", { method: "POST" });

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
};

const seedCheck = async () => {
  const { status } = await fetch("/api/seed/check");

  populated.value = !(status === 200);
};

onMounted(() => seedCheck());
</script>

<style lang="scss" scoped>
span {
  display: block;
  margin-block-end: var(--spacing-default);
}

.start-button {
  --utrecht-button-background-color: var(--color-white);
  --utrecht-button-color: var(--color-accent-text);
}
</style>
