<template>
  <div class="toelichtingen-container">
    <div class="toelichtingen">
      <utrecht-heading :level="3">Toelichtingen</utrecht-heading>

      <p
        class="toelichting"
        v-for="(toelichting, idx) in toelichtingen"
        :key="idx"
      >
        {{ toelichting }}
      </p>
    </div>

  
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import type { ZaakDetails } from "./../types";
import {
  Heading as UtrechtHeading,
} from "@utrecht/component-library-vue";


const props = defineProps<{
  zaak: ZaakDetails;
}>();

const toelichtingen = ref<string[]>(props.zaak.toelichting.split("\n\n"));


watch(
  () => props.zaak.toelichting,
  (toelichting) => {
    toelichtingen.value = toelichting.split("\n\n");
  }
);
</script>

<style scoped lang="scss">
.toelichtingen-container {
  display: flex;
  gap: var(--spacing-large);
  justify-content: space-between;

  utrecht-heading {
    margin-block-end: var(--spacing-default);
  }

  .toelichtingen {
    flex: 3;

    .toelichting {
      padding-block: var(--spacing-small);
      padding-inline-start: var(--spacing-small);
      margin-block-end: var(--spacing-default);
      border-left: var(--spacing-extrasmall) solid var(--color-secondary);
    }
  }

  .add-toelichting {
    flex: 2;
    max-width: 600px;

    .add-toelichting-heading {
      display: flex;
      align-items: center;
      gap: var(--spacing-default);

      .spinner {
        margin: 0;
        bottom: var(--spacing-small);
        font-size: var(--spacing-default);
      }
    }
  }
}
</style>
