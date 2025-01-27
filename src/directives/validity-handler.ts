import type { Directive } from "vue";

const validityHandler: Directive<
  HTMLInputElement & { onInputHandler?: EventListener },
  (el: HTMLInputElement) => void
> = {
  mounted(el, binding) {
    const validator = binding.value;
    const handler = () => validator(el);

    validator(el);

    el.addEventListener("input", handler);
    el.onInputHandler = handler;
  },
  unmounted(el) {
    if (el.onInputHandler) {
      el.removeEventListener("input", el.onInputHandler);
      delete el.onInputHandler;
    }
  },
};

export default validityHandler;
