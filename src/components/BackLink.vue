<template>
  <router-link v-if="back" :to="back" class="icon-before chevron-left">
    <slot>Terug</slot>
  </router-link>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const back = computed(() => {
  const { back } = router.options.history.state;
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
