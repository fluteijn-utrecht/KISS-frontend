<template>
  <utrecht-heading :level="1">Skill</utrecht-heading>

  <template v-if="loading"><SimpleSpinner /></template>

  <template v-else-if="success">
    <div class="container">
      <p>De skill is opgeslagen.</p>
      <router-link to="/Beheer/Skills/"> Terug naar het overzicht </router-link>
    </div>
  </template>

  <template v-else>
    <form class="container" @submit.prevent="submit">
      <label for="naam" class="utrecht-form-label"
        ><span>Naam</span>
        <input type="text" id="naam" v-model="skill.naam" required
      /></label>
    </form>

    <menu>
      <li>
        <router-link to="/Beheer/Skills/">
          <utrecht-button appearance="secondary-action-button" type="button">
            Annuleren
          </utrecht-button>
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
    </menu>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";

const props = defineProps(["id"]);

type skillType = {
  id?: number;
  naam?: string;
};

const loading = ref<boolean>(false);
const success = ref<boolean>(false);

const skill = ref<skillType>({});

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const submit = async () => {
  loading.value = true;

  success.value = false;
  try {
    if (props.id) {
      const result = await fetch("/api/Skills/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        success.value = true;
      }
    } else {
      const result = await fetch("/api/Skills/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        success.value = true;
      }
    }
    success.value = true;
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  if (props.id) {
    loading.value = true;
    try {
      const response = await fetch("/api/Skills/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();
      skill.value = jsonData;
    } catch {
      showError();
    } finally {
      loading.value = false;
    }
  }
});
</script>

<style>
menu {
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

form {
  margin-top: var(--spacing-default);
}
label > span {
  display: block;
}
</style>
