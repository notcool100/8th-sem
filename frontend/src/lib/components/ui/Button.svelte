<script>
  let { 
    variant = 'primary', 
    size = 'md', 
    onclick, 
    children, 
    class: className = '',
    ...rest 
  } = $props();

  const baseClass = 'btn';
  const variantClass = `btn-${variant}`;
  const sizeClass = `btn-${size}`;
</script>

<button 
  class="{baseClass} {variantClass} {sizeClass} {className}" 
  {onclick}
  {...rest}
>
  {@render children()}
</button>

<style>
  /* Base styles are in app.css, but we can add component-specific ones here */
  .btn-sm { padding: 0.5rem 1rem; font-size: 0.875rem; }
  .btn-md { padding: 0.75rem 1.5rem; font-size: 1rem; }
  .btn-lg { padding: 1rem 2rem; font-size: 1.125rem; }
</style>
