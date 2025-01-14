<template>
  <utrecht-heading :level="1">{{ uuid }}</utrecht-heading>

  <menu>
    <li>
      <router-link
        to="/Beheer/vacs/"
        class="utrecht-button utrecht-button--secondary-action"
      >
        Annuleren
      </router-link>
    </li>
  </menu>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";

defineProps<{ uuid?: string }>();
</script>
