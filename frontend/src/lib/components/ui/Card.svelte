<script>
  let { 
    children, 
    title,
    subtitle,
    class: className = '',
    ...rest 
  } = $props();
</script>

<div class="card glass {className}" {...rest}>
  {#if title || subtitle}
    <div class="card-header">
      {#if title}<h3>{title}</h3>{/if}
      {#if subtitle}<p class="subtitle">{subtitle}</p>{/if}
    </div>
  {/if}
  
  <div class="card-content">
    {@render children()}
  </div>
</div>

<style>
  .card {
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  .card-header h3 {
    margin: 0;
    font-size: 1.25rem;
  }

  .subtitle {
    font-size: 0.875rem;
    color: var(--color-text-muted);
  }

  .card-content {
    flex-grow: 1;
  }
</style>
