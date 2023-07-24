<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>
    <section v-if="antwoordSection">
      <utrecht-heading :level="headingLevel + 1">{{
        antwoordSection.label
      }}</utrecht-heading>
      <div v-html="antwoordSection.html" class="htmlcontent"></div>
    </section>
    <section v-if="toelichtingSection">
      <utrecht-heading :level="headingLevel + 1">{{
        toelichtingSection.label
      }} (interne informatie)</utrecht-heading>
      <div v-html="toelichtingSection.html" class="htmlcontent"></div>
    </section>
  </article>
</template>
<script setup lang="ts">
import {  unescapedSanatizedWithIncreadesHeadingsHtml } from "@/helpers/html";
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


const getSection = (
  sectionName: string
): { label: string; html: string } | null => {
  const section = props.raw[sectionName];

  return section ? { label: sectionName, html: unescapedSanatizedWithIncreadesHeadingsHtml(section, props.headingLevel) } : null;
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

 

  // > section {
  //   flex: 1;
  //   display: none;

  //   &.is-active {
  //     display: block;
  //   }


}




</style>
