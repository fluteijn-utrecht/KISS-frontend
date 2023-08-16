<template>
  <div class="toelichtingen-container">
    <div class="toelichtingen">
      <utrecht-heading :level="3">Toelichtingen</utrecht-heading>

      <p
        class="toelichting"
        v-for="(toelichting, idx) in toelichtingen"
        :key="idx"
      >
        {{ toelichting }}
      </p>
    </div>

    <form class="add-toelichting" @submit.prevent="submit">
      <div class="add-toelichting-heading">
        <utrecht-heading :level="3">Nieuwe toelichting </utrecht-heading>

        <simple-spinner v-if="formIsLoading" class="spinner" />
      </div>

      <textarea
        required
        :disabled="formIsLoading"
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="10"
        placeholder="Schrijf een nieuwe toelichting bij de zaak"
        v-model="newToelichtingInputValue"
      ></textarea>

      <utrecht-button
        :disabled="formIsLoading"
        appearance="primary-action-button"
        type="submit"
      >
        Opslaan
      </utrecht-button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import type { ZaakDetails } from "./../types";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import { toast } from "@/stores/toast";
import { updateToelichting } from "./../service";
import SimpleSpinner from "../../../components/SimpleSpinner.vue";

const props = defineProps<{
  zaak: ZaakDetails;
}>();

const SUCCESS = "success";
const emit = defineEmits<{ (e: typeof SUCCESS): void }>();

const toelichtingen = ref<string[]>(props.zaak.toelichting.split("\n\n"));
const formIsLoading = ref<boolean>(false);
const newToelichtingInputValue = ref<string>("");

watch(
  () => props.zaak.toelichting,
  (toelichting) => {
    toelichtingen.value = toelichting.split("\n\n");
  }
);
const submit = async () => {
  formIsLoading.value = true;

  const newToelichting = newToelichtingInputValue.value.replace(
    /(\r\n|\n|\r)/gm,
    " "
  );
  const newToelichtingen = `${props.zaak.toelichting}\n\n${newToelichting}`;

  updateToelichting(props.zaak, newToelichtingen)
    .then(() => {
      toast({ text: "De toelichting is opgeslagen." });
      toelichtingen.value = [
        ...toelichtingen.value,
        newToelichtingInputValue.value,
      ];
      newToelichtingInputValue.value = "";
      emit(SUCCESS);
    })
    .catch(() => {
      toast({
        type: "error",
        text: "Oeps het lukt niet om deze toelichting op te slaan. Probeer het later opnieuw.",
      });
    })
    .finally(() => {
      formIsLoading.value = false;
    });
};
</script>

<style scoped lang="scss">
.toelichtingen-container {
  display: flex;
  gap: var(--spacing-large);
  justify-content: space-between;

  utrecht-heading {
    margin-block-end: var(--spacing-default);
  }

  .toelichtingen {
    flex: 3;

    .toelichting {
      padding-block: var(--spacing-small);
      padding-inline-start: var(--spacing-small);
      margin-block-end: var(--spacing-default);
      border-left: var(--spacing-extrasmall) solid var(--color-secondary);
    }
  }

  .add-toelichting {
    flex: 2;
    max-width: 600px;

    .add-toelichting-heading {
      display: flex;
      align-items: center;
      gap: var(--spacing-default);

      .spinner {
        margin: 0;
        bottom: var(--spacing-small);
        font-size: var(--spacing-default);
      }
    }
  }
}
</style>
