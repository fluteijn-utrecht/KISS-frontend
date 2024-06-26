// we export these from here so we can combine asynchronous imports with tree shaking
import _Ckeditor from "@ckeditor/ckeditor5-vue";

export {
  ClassicEditor,
  Essentials,
  Autoformat,
  Bold,
  Italic,
  BlockQuote,
  Heading,
  Link,
  List,
  Paragraph,
  type EditorConfig,
} from "ckeditor5";

export const Ckeditor = _Ckeditor.component;
