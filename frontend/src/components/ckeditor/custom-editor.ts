import { ClassicEditor } from "@ckeditor/ckeditor5-editor-classic";
import { Essentials } from "@ckeditor/ckeditor5-essentials";
import { Autoformat } from "@ckeditor/ckeditor5-autoformat";
import { Bold } from "@ckeditor/ckeditor5-basic-styles";
import { Italic } from "@ckeditor/ckeditor5-basic-styles";
import { BlockQuote } from "@ckeditor/ckeditor5-block-quote";
import { Heading } from "@ckeditor/ckeditor5-heading";
import { Link } from "@ckeditor/ckeditor5-link";
import { List } from "@ckeditor/ckeditor5-list";
import { Paragraph } from "@ckeditor/ckeditor5-paragraph";

export default class CustomEditor extends ClassicEditor {}

CustomEditor.builtinPlugins = [
  Essentials,
  Autoformat,
  Bold,
  Italic,
  BlockQuote,
  Heading,
  Link,
  List,
  Paragraph,
];

CustomEditor.defaultConfig = {
  toolbar: {
    items: [
      "heading",
      "|",
      "bold",
      "italic",
      "link",
      "bulletedList",
      "numberedList",
      "undo",
      "redo",
    ],
  },
  language: "en",
};
