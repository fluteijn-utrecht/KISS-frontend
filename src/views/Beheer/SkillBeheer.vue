<template>
  <h1>Skill</h1>

  <template v-if="loading"> ..loading </template>

  <template v-else-if="success"> klaar. ga terug... </template>

  <template v-else>
    <label for="naam">Naam</label>
    <input type="text" id="naam" v-model="skill.naam" />
    <input type="submit" value="ok" @click="submit" />

    <template if="error"> fout:... </template>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

const props = defineProps(["id"]);

type skillType = {
  id?: number;
  naam?: string;
};

const loading = ref<boolean>(true);
const success = ref<boolean>(false);
const error = ref<boolean>(false);

const skill = ref<skillType>({});

const submit = async () => {
  loading.value = true;
  error.value = false;
  success.value = false;
  try {
    if (props.id) {
      await fetch("/api/Skills/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
      });
    } else {
      await fetch("/api/Skills/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
      });
    }
    success.value = true;
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  loading.value = true;
  error.value = false;
  if (props.id) {
    try {
      const response = await fetch("/api/Skills/" + props.id);
      const jsonData = await response.json();
      skill.value = jsonData;
    } catch {
      error.value = true;
    }
  }
  loading.value = false;
});
</script>

<style></style>
