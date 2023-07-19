<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>
    <section v-if="antwoordSection">
      <utrecht-heading :level="headingLevel + 1">{{
        antwoordSection.label
      }}</utrecht-heading>
      <div v-html="antwoordSection.html"></div>
    </section>
    <section v-if="toelichtingSection">
      <utrecht-heading :level="headingLevel + 1">{{
        toelichtingSection.label
      }}</utrecht-heading>
      <div v-html="toelichtingSection.html"></div>
    </section>
  </article>
</template>
<script setup lang="ts">
import {
  sanitizeHtmlToBerichtFormat,
  unescapeHtml,
  increaseHeadings,
} from "@/helpers/html";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed } from "vue";

const knownSections = {
  toelichting: "toelichting",
  antwoord: "antwoord",
} as const;

const props = defineProps<{
  raw: any;
  title: string;
  headingLevel: 2 | 3 | 4;
}>();

function processHtml(html: string) {
  if (!html) {
    return html;
  }
  const unescapedHtml = unescapeHtml(html);
  const cleanedHtml = sanitizeHtmlToBerichtFormat(unescapedHtml);
  const htmlWithIncreasedHeadings = increaseHeadings(
    cleanedHtml,
    (props.headingLevel + 1) as any
  );
  return htmlWithIncreasedHeadings;
}

const getSection = (
  sectionName: string
): { label: string; html: string } | null => {
  const section = props.raw[sectionName];

  return section
    ? {
        label: sectionName,
        html: processHtml(section),
      }
    : null;
};

const antwoordSection = computed(() => getSection(knownSections.antwoord));

const toelichtingSection = computed(() =>
  getSection(knownSections.toelichting)
);
</script>

<style scoped lang="scss">
article {
  display: flex;
  flex-flow: column wrap;
  gap: var(--spacing-large);

  .heading {
    width: 100%;
  }
}
</style>
