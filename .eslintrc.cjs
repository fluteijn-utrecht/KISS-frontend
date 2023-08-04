/* eslint-env node */
require("@rushstack/eslint-patch/modern-module-resolution");

module.exports = {
  root: true,
  extends: [
    "plugin:vue/vue3-essential",
    "eslint:recommended",
    "@vue/eslint-config-typescript/recommended",
    "eslint-config-prettier",
  ],
  env: {
    "vue/setup-compiler-macros": true,
  },
  rules: {
    "vue/multi-word-component-names": [
      "error",
      {
        ignores: ["Pagination", "Paragraph"],
      },
    ],
    // TODO: treat these rules as errors and fix all affected code
    "vue/no-setup-props-destructure": ["warn"],
    "vue/prefer-import-from-vue": ["warn"],
  },
};
