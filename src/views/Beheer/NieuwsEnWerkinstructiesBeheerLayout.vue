<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/Beheer/NieuwsEnWerkinstructies"
      >Nieuws en werkinstructies</router-link
    >
    <router-link to="/Beheer/Skills">Skills</router-link>
  </nav>
  <main>
    <router-view />
  </main>
</template>

<style lang="scss" scoped>
nav {
  display: flex;
  gap: var(--spacing-default);
}
main {
  max-width: var(--section-width-large);
}
</style>
