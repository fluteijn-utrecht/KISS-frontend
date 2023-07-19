<template>
    <article>
       <utrecht-heading :level="headingLevel" class="heading">
        {{ title }}
      </utrecht-heading>
     <section
        v-for="{ id, html, label } in mappedSections"
        :key="id + 'text'"
        :class="{ 'is-active': true }"
        :id="id"
      >
        <utrecht-heading :level="headingLevel + 1">{{ label }}</utrecht-heading>
        <div v-html="html"></div>
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
  import { nanoid } from "nanoid";
  import { computed, ref } from "vue";
 
  
  const knownSections = {
 
    toelichting: "toelichting",
    antwoord: "antwoord",    
  } as const;
  
  const componentId = nanoid();
  
  const props = defineProps<{
    raw: any;
    title: string;
    headingLevel: 2 | 3 | 4;
  }>();
  
  function processHtml(html: string) {

    if(!html){
        return html
    }
    const unescapedHtml = unescapeHtml(html);
    const cleanedHtml = sanitizeHtmlToBerichtFormat(unescapedHtml);
    const htmlWithIncreasedHeadings = increaseHeadings(
      cleanedHtml,
      (props.headingLevel + 1) as any
    );
    return htmlWithIncreasedHeadings;
  }
  
  const currentSectionIndex = ref(0);

  const processedSections = computed(() => {

    const allSections = Object.entries(knownSections).map(([key, label]) => ({
      label,
      key: key,
      text: props.raw[key],
    }));

    const sectionsWithActualText = allSections.filter(({ text }) => !!text);
  
    const sectionsWithProcessedHtml = sectionsWithActualText.map(
      ({ label, text, key }) => ({
        key: key,
        label,
        html: processHtml(text),
      })
    );
  
    return sectionsWithProcessedHtml;
   });
  
  // seperate this computed variable for caching purposes: making a section active doesn't trigger the reprocessing of the source html
  const mappedSections = computed(() =>
    processedSections.value.map((section, index) => ({
      ...section,
      id: componentId + index,
      isActive: index === currentSectionIndex.value,
      setActive() {
        currentSectionIndex.value = index;
      },
    }))
  );
  
 </script>
  
  <style scoped lang="scss">
  article {
    display: flex;
    flex-wrap: wrap;
    gap: var(--spacing-large);
  
    .heading {
      width: 100%;
    }
  
    > section {
      flex: 1;
      display: none;
  
      &.is-active {
        display: block;
      }
  
      div :deep(h3),
      div :deep(h4) {
        margin-block-start: var(--spacing-default);
      }
  
      :deep(ul) {
        list-style: disc;
        margin-inline-start: var(--spacing-default);
        margin-block: var(--spacing-small);
  
        ul {
          list-style: circle;
          margin-block: 0;
        }
      }
  
      :deep(td) {
        border: 1px var(--color-tertiary) solid;
        padding: var(--spacing-small);
      }
  
      :deep(table) {
        margin-block: var(--spacing-small);
      }
  
      :deep(p + p) {
        margin-block-start: var(--spacing-small);
      }
    }
  }
  
  nav ul {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-extrasmall);
  
    li {
      border: 1px solid var(--color-tertiary);
  
      a,
      span {
        text-decoration: none;
        color: inherit;
        padding: var(--spacing-small);
        display: block;
      }
    }
  
    .is-active {
      background-color: var(--color-tertiary);
      color: var(--color-white);
      text-decoration: underline;
    }
  }
  </style>
  