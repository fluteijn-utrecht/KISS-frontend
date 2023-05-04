<template>
  <h1>Berichten</h1>
  <div v-if="loading">loading...</div>
  <div v-else-if="error">Er is een fout opgetreden.</div>
  <ul v-else>
    <li v-for="bericht in berichten" :key="bericht.id">
      <router-link :to="'/BerichtBeheer/' + bericht.id">{{
        bericht.titel
      }}</router-link>
    </li>
  </ul>
  <router-link to="/BerichtBeheer/">+</router-link>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

type berichtType = {
  id: number;
  titel: string;
};

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
const berichten = ref<Array<berichtType>>([]);

async function load() {
  loading.value = true;
  error.value = false;
  try {
    const response = await fetch("/api/berichten");
    const jsonData = await response.json();
    berichten.value = jsonData;
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
