<template>
  <utrecht-heading :level="1">Vac</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="vac">
    <beheer-form>
      <template #formFields> </template>
      <template #formMenuListItems>
        <li>
          <router-link
            to="/Beheer/vacs/"
            class="utrecht-button utrecht-button--secondary-action"
          >
            Annuleren
          </router-link>
        </li>

        <li>
          <utrecht-button appearance="primary-action-button" type="submit">
            Opslaan
          </utrecht-button>
        </li>
      </template>
    </beheer-form>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
// import { fetchLoggedIn } from "@/services";
import BeheerForm from "@/components/beheer/BeheerForm.vue";
import type { Vac } from "@/features/search/types";

const props = defineProps<{ uuid?: string }>();

const loading = ref<boolean>(false);

const vac = ref<Vac>();

onMounted(async () => {
  loading.value = true;

  loading.value = false;
});
</script>

<style scoped lang="scss">
.p-r {
  position: relative;
}
</style>
