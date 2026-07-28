<section class="quick-stats container">
  <div class="stat-card glass">
    <h3>8,848m</h3>
    <p>Highest Peak</p>
  </div>
  <div class="stat-card glass">
    <h3>100+</h3>
    <p>Ethnic Groups</p>
  </div>
  <div class="stat-card glass">
    <h3>10</h3>
    <p>UNESCO Sites</p>
  </div>
</section>

<style>
  .quick-stats {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 2rem;
    margin-top: -4rem;
    position: relative;
    z-index: 2;
  }

  .stat-card {
    padding: 2rem;
    text-align: center;
    color: white;
  }

  .stat-card h3 {
    font-size: 2rem;
    margin-bottom: 0.5rem;
    color: var(--color-accent);
  }

  .stat-card p {
    font-weight: 500;
    opacity: 0.9;
  }
  @media (max-width: 768px) {
    .quick-stats {
      margin-top: 2rem;
      padding: 0 1rem;
      gap: 1rem;
    }
    
    .stat-card {
      padding: 1.5rem;
    }
  }
</style>
