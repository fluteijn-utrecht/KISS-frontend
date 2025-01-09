<template>
  <utrecht-heading :level="1">Vac</utrecht-heading>

  <div v-if="loading"><SimpleSpinner /></div>

  <div v-else-if="error">Er is een fout opgetreden.</div>

  <template v-else>
    <beheer-form @submit="submit">
      <template #formFields>
        <label class="utrecht-form-label" for="vraag">Vraag</label>
        <ck-editor v-model="vac.vraag" required />

        <label class="utrecht-form-label" for="antwoord">Antwoord</label>
        <ck-editor v-model="vac.antwoord" required />
      </template>

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
          <utrecht-button
            appearance="primary-action-button"
            type="submit"
            @click="submit"
          >
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
import CkEditor from "@/components/ckeditor";
import BeheerForm from "@/components/beheer/BeheerForm.vue";
import { fetchLoggedIn, parseJson } from "@/services";
import type { Vac } from "@/features/search/types";

type VacObject = {
  uuid?: string;
  record: {
    data: Vac;
  };
};

const props = defineProps<{ uuid?: string }>();

const vac = ref<Vac>({
  vraag: "",
  antwoord: "",
});

const loading = ref(false);
const error = ref(false);

const submit = async () => {
  loading.value = true;

  // fetchLoggedIn(`/api/vacs/api/v2/objects/${props.uuid}`, {
  //   method: "PUT",
  //   headers: {
  //     "Content-Type": "application/json",
  //   },
  //   body: JSON.stringify(),
  // })
  //   .catch(() => (error.value = true))
  //   .finally(() => (loading.value = false));
};

onMounted(() => {
  if (!props.uuid) return;

  loading.value = true;

  fetchLoggedIn(`/api/vacs/api/v2/objects/${props.uuid}`)
    .then(parseJson)
    .then((r: VacObject) => (vac.value = r.record.data))
    .catch(() => (error.value = true))
    .finally(() => (loading.value = false));
});
</script>
