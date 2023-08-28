<template>
  <a
    v-if="back"
    :href="back"
    class="icon-before chevron-left"
    @click.prevent.stop="$router.back()"
  >
    <slot>Terug</slot>
  </a>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();
const back = computed(() => {
  const { back } = route.meta;
  return typeof back === "string" && back;
});
</script>

<style lang="scss" scoped>
.icon-before::before {
  margin-inline-end: 0.5ch;
  min-inline-size: 1.25ch;
}

.icon-before {
  text-decoration: none;
  color: inherit;
}
</style>
