import {
  createRouter,
  createWebHistory,
  type NavigationGuard,
} from "vue-router";
import HomeView from "../views/HomeView.vue";
import AfhandelingView from "../views/AfhandelingView.vue";
import ZakenView from "../views/ZakenView.vue";
import PersonenView from "../views/PersonenView.vue";
import PersoonDetailView from "../views/PersoonDetailView.vue";
import ZaakDetailView from "../views/ZaakDetailView.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { redirectRoute } from "@/features/login";
import BedrijvenView from "@/views/BedrijvenView.vue";
import BedrijfDetailView from "@/views/BedrijfDetailView.vue";
import LinksView from "@/views/LinksView.vue";
import NieuwsEnWerkinstructiesBeheer from "@/views/Beheer/NieuwsEnWerkinstructiesBeheer.vue";
import NieuwsEnWerkinstructieBeheer from "@/views/Beheer/NieuwsEnWerkinstructieBeheer.vue";
import SkillsBeheer from "@/views/Beheer/SkillsBeheer.vue";
import SkillBeheer from "@/views/Beheer/SkillBeheer.vue";

const guardContactMoment: NavigationGuard = (to, from, next) => {
  const contactmoment = useContactmomentStore();
  if (contactmoment.contactmomentLoopt) {
    next();
  } else {
    next("/");
  }
};

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/afhandeling",
      name: "afhandeling",
      component: AfhandelingView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: false,
        showNotitie: false,
        showSearch: false,
        hideSidebar: true,
      },
    },
    {
      path: "/personen",
      name: "personen",
      component: PersonenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/personen/:persoonId",
      name: "persoonDetail",
      props: true,
      component: PersoonDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/bedrijven",
      name: "bedrijven",
      component: BedrijvenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/bedrijven/:bedrijfId",
      name: "bedrijfDetail",
      props: true,
      component: BedrijfDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/zaken",
      name: "zaken",
      component: ZakenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/zaken/:zaakId",
      name: "zaakDetail",
      props: true,
      component: ZaakDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/links",
      name: "links",
      component: LinksView,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/NieuwsEnWerkinstructiesBeheer",
      name: "NieuwsEnWerkinstructiesBeheer",
      component: NieuwsEnWerkinstructiesBeheer,
      meta: {},
    },
    {
      path: "/SkillsBeheer",
      name: "SkillsBeheer",
      component: SkillsBeheer,
      meta: {},
    },
    {
      path: "/NieuwsEnWerkinstructieBeheer",
      name: "NieuwsEnWerkinstructieBeheer",
      component: NieuwsEnWerkinstructieBeheer,
      meta: {},
    },
    {
      path: "/SkillBeheer",
      name: "SkillBeheer",
      component: SkillBeheer,
      meta: {},
    },
    redirectRoute,
  ],
});

export default router;
