<template>
  <utrecht-button
    @click="onStopContactMoment"
    title="Contactmoment afronden"
    appearance="primary-action-button"
    class="afronden"
  >
    Afronden
  </utrecht-button>
</template>

<script lang="ts" setup>
import { useRouter } from "vue-router";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const router = useRouter();
const onStopContactMoment = async () => router.push({ name: "afhandeling" }); // een link zou wellicht toepasselijker zijn, maar de styling adhv het designsystem wordt lastig.
</script>

<style lang="scss" scoped>
/* stylelint-disable custom-property-pattern */
.afronden {
  --_utrecht-button-background-color: var(--color-error);
  --_utrecht-button-focus-background-color: var(--color-error);
  --_utrecht-button-active-background-color: var(--color-error);
  --_utrecht-button-hover-background-color: var(--color-error-hover);
  --_utrecht-button-color: var(--color-headings);
  --_utrecht-button-hover-color: var(--color-headings);
  --_utrecht-button-active-color: var(--color-headings);
  --_utrecht-button-focus-color: var(--color-headings);
}
</style>
