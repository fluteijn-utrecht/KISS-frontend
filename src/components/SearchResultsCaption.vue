<template>
  <caption>
    <p>Zoekresultaten</p>
    <p v-if="resultCount === 0">Geen resultaten gevonden</p>
    <p v-else-if="resultCount === 1">1 resultaat gevonden</p>
    <p v-else>{{ resultCount }} resultaten gevonden</p>
  </caption>
</template>

<script setup lang="ts">
import type { Paginated, PaginatedResult } from "@/services";
import { computed } from "vue";

const props = defineProps<{
  results: Paginated<unknown> | PaginatedResult<unknown>;
}>();

const resultCount = computed(() => {
  if (
    "totalRecords" in props.results &&
    typeof props.results.totalRecords === "number"
  )
    return props.results.totalRecords;
  if ("count" in props.results && typeof props.results.count === "number")
    return props.results.count;
  return props.results.page.length;
});
</script>

<style lang="scss" scoped>
caption {
  text-align: left;
  margin-block-end: var(--spacing-default);

  > * {
    &:first-child {
      color: var(--caption-heading-color, var(--color-headings));
      font-size: var(
        --caption-heading-size,
        var(--utrecht-heading-2-font-size, 2rem)
      );
      font-weight: var(
        --caption-heading-weight,
        var(
          --utrecht-heading-2-font-weight,
          var(--utrecht-heading-font-weight, bold)
        )
      );
      border-bottom: 1px solid var(--color-tertiary);
      padding-block-end: var(--spacing-small);
    }

    &:last-child {
      margin-block-start: var(--spacing-small);
      color: var(--caption-results-color, var(--color-primary));
    }
  }
}
</style>
