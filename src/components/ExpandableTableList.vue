<template>
  <section v-if="items.length">
    <ul>
      <li class="header-row">
        <slot name="header"></slot>
        <span class="chevron"></span>
      </li>
      <li v-for="item in items" :key="item[itemKey]">
        <details @click="toggleDetails">
          <slot name="item" :item="item"></slot>
        </details>
      </li>
    </ul>
  </section>
</template>

<script lang="ts" setup>
defineProps<{
  items: any[];
  itemKey: string;
}>();

const toggleDetails = (e: Event) => {
  e.preventDefault();
  if (e.currentTarget instanceof HTMLDetailsElement) {
    e.currentTarget.open = !e.currentTarget.open;
  }
};
</script>

<style lang="scss" scoped>
ul {
  --column-width: 25ch;
  --gap: var(--spacing-default);
  --columns: 1fr 1fr 1fr 1fr 1rem;
  --spinner-size: 1em;

  display: grid;
  list-style: none;
  padding: 0;
}

li:not(:first-child, :last-child) {
  border-bottom: 2px solid var(--color-tertiary);
}

.header-row {
  display: grid;
  grid-template-columns: var(--columns);
  gap: var(--gap);
  padding-inline: var(--gap);
  padding-block: var(--spacing-default);
  background: var(--color-tertiary);
  color: var(--color-white);
}

:deep(summary) {
  list-style: none;
  display: grid;
  grid-template-columns: var(--columns);
  gap: var(--gap);
  padding-block-start: var(--spacing-default);
  padding-block-end: var(--spacing-default);
}

:deep(details) {
  display: grid;
  gap: var(--spacing-default);
  background: var(--color-white);

  &[open],
  &:hover {
    background-color: var(--color-secondary);
  }

  > * {
    padding-inline: var(--gap);
  }
}

:deep(dt) {
  font-weight: bold;
}

:deep(dl) {
  padding-inline-start: var(--spacing-default);
  display: grid;
  column-gap: var(--gap);
  row-gap: var(--spacing-default);
  grid-template-columns: var(--column-width) 1fr;
  padding-block: var(--spacing-large);
}

:deep(.tekst) {
  max-width: 90ch;
  white-space: pre-wrap;
}
</style>
