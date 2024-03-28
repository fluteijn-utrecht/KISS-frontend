<template>
  <utrecht-heading :level="1">Kanalen</utrecht-heading>
 <kanalen-overzicht></kanalen-overzicht>
  <menu>
    <router-link
      to="/Beheer/kanaal/"
      title="toevoegen"
      class="utrecht-button utrecht-button--primary-action icon icon-after plus icon-only"
    >
    </router-link>
  </menu>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { KanalenOverzicht } from "../../../features/Kanalen";

 
</script>

<style scoped lang="scss">
li h2 {
  margin-block-start: var(--spacing-large);
}
</style>
