<template>
  <h1>Skills</h1>
  <div v-if="loading">loading...</div>
  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li v-for="skill in skills" :key="skill.id">
      <router-link :to="'/Beheer/Skill/' + skill.id">{{
        skill.naam
      }}</router-link>
      <button @click="confirmVerwijder(skill.id)">verwijder</button>
    </li>
  </ul>
  <router-link to="/Beheer/Skill/">+</router-link>
  <div v-if="deletesuccess">skill verwijderd</div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

type skill = {
  id: number;
  naam: string;
};

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
const deletesuccess = ref<boolean>(false);
const skills = ref<Array<skill>>([]);

async function load() {
  loading.value = true;
  error.value = false;
  try {
    const response = await fetch("/api/Skills");
    const jsonData = await response.json();
    skills.value = jsonData;
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;
  error.value = false;
  deletesuccess.value = false;
  try {
    await fetch("/api/Skills/" + id, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
    });
    deletesuccess.value = true;
    load();
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("weet u zeker dat u deze skill wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
</script>

<style scoped lang="scss"></style>
