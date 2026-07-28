<!-- src/lib/components/RichTextEditor.svelte -->
<script lang="ts">
  import { onMount } from "svelte";

  let {
    value = $bindable(""),
    placeholder = "Write something rich...",
  }: { value?: string; placeholder?: string } = $props();

  let editorContainer: HTMLDivElement;
  let quillInstance: any = null;
  let isUpdatingFromExternal = false;

  onMount(async () => {
    if (typeof window !== "undefined") {
      const cssId = "quill-cdn-css";
      if (!document.getElementById(cssId)) {
        const link = document.createElement("link");
        link.id = cssId;
        link.rel = "stylesheet";
        link.href =
          "https://cdn.jsdelivr.net/npm/quill@2.0.2/dist/quill.snow.css";
        document.head.appendChild(link);
      }

      if (!(window as any).Quill) {
        await new Promise<void>((resolve, reject) => {
          const script = document.createElement("script");
          script.src = "https://cdn.jsdelivr.net/npm/quill@2.0.2/dist/quill.js";
          script.onload = () => resolve();
          script.onerror = () => reject(new Error("Quill script load failed"));
          document.body.appendChild(script);
        });
      }

      const Quill = (window as any).Quill;
      if (Quill && editorContainer) {
        quillInstance = new Quill(editorContainer, {
          theme: "snow",
          placeholder: placeholder,
          modules: {
            toolbar: [
              ["bold", "italic", "underline", "strike"],
              [{ header: [1, 2, 3, false] }],
              [{ list: "ordered" }, { list: "bullet" }],
              ["link", "clean"],
            ],
          },
        });

        if (value) {
          quillInstance.root.innerHTML = value;
        }

        quillInstance.on("text-change", () => {
          if (isUpdatingFromExternal) return;
          const html = quillInstance.root.innerHTML;
          if (html === "<p><br></p>") {
            value = "";
          } else {
            value = html;
          }
        });
      }
    }
  });

  $effect(() => {
    if (quillInstance && value !== undefined) {
      const currentHtml = quillInstance.root.innerHTML;
      const normalizedValue = value || "";
      if (currentHtml !== normalizedValue) {
        isUpdatingFromExternal = true;
        const cursor = quillInstance.getSelection();
        quillInstance.root.innerHTML = normalizedValue;
        if (cursor) {
          quillInstance.setSelection(cursor.index, cursor.length);
        }
        isUpdatingFromExternal = false;
      }
    }
  });
</script>

<div class="quill-editor-wrapper">
  <div bind:this={editorContainer}></div>
</div>

<style>
  .quill-editor-wrapper {
    display: block;
    width: 100%;
    margin-top: 4px;
    border-radius: 10px;
    overflow: hidden;
    border: 1px solid #c9d6e2;
    background: #fff;
  }

  :global(.ql-toolbar.ql-snow) {
    border: none !important;
    border-bottom: 1px solid #d8e2eb !important;
    background: #f8fafc;
    padding: 8px 12px !important;
  }

  :global(.ql-container.ql-snow) {
    border: none !important;
    min-height: 150px;
    font-family: inherit !important;
    font-size: 14px !important;
  }

  :global(.ql-editor) {
    min-height: 150px;
    color: #10243e;
    line-height: 1.6;
  }

  :global(.ql-editor.ql-blank::before) {
    color: #94a3b8;
    font-style: normal;
  }
  /* 
  @media (prefers-color-scheme: dark) {
    .quill-editor-wrapper {
      border-color: #444;
      background: #2b2b2b;
    }
    :global(.ql-toolbar.ql-snow) {
      background: #202020;
      border-bottom-color: #444 !important;
    }
    :global(.ql-toolbar.ql-snow .ql-stroke) {
      stroke: #e0e0e0 !important;
    }
    :global(.ql-toolbar.ql-snow .ql-fill) {
      fill: #e0e0e0 !important;
    }
    :global(.ql-toolbar.ql-snow .ql-picker) {
      color: #e0e0e0 !important;
    }
    :global(.ql-toolbar.ql-snow .ql-picker-options) {
      background-color: #2b2b2b !important;
      border-color: #444 !important;
    }
    :global(.ql-editor) {
      color: #e0e0e0;
    }
  } */
</style>
