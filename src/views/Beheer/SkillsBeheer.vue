<template>
  <h1>Skills</h1>
  <div v-if="loading">loading...</div>
  <div v-else-if="error">Er is een fout opgetreden.</div>
  <ul v-else>
    <li v-for="skill in skills" :key="skill.id">
      <router-link :to="'/SkillBeheer/' + skill.id">{{
        skill.naam
      }}</router-link>
    </li>
  </ul>
  <router-link to="/SkillBeheer/">+</router-link>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

type skill = {
  id: number;
  naam: string;
};

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
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

onMounted(() => {
  load();
});
</script>

<style scoped lang="scss"></style>
