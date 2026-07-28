<script lang="ts">
    import type { ActionData } from "./$types";
    import { NTB_LOGO_URL } from "$lib/constants/branding";

    let { form } = $props<{ form?: ActionData }>();

    let showPassword = $state(false);
</script>

<div class="login-container">
    <div class="login-card glass animate-fade-in">
        <div class="logo-container">
            <img
                src={NTB_LOGO_URL}
                alt="Nepal Tourism Board"
                class="ntb-logo"
            />
        </div>
        <h1>Portal Login</h1>
        <p>Access the official Nepal Tourism Board system</p>

        {#if form?.message}
            <div class="form-alert" class:error={!form.success}>
                {form.message}
            </div>
        {/if}

        <form method="POST">
            <div class="form-group">
                <label for="email">Email Address</label>
                <input
                    class="input-text"
                    type="email"
                    id="email"
                    name="email"
                    value={form?.email ?? ""}
                    placeholder="admin@ntb.gov.np"
                    required
                />
            </div>
            <div class="form-group">
                <label for="password">Password</label>
                <div class="input-wrapper">
                    <input
                        class="input-text"
                        type={showPassword ? "text" : "password"}
                        id="password"
                        name="password"
                        placeholder="••••••••"
                        required
                    />
                    <button
                        type="button"
                        class="eye-btn"
                        onclick={() => (showPassword = !showPassword)}
                        aria-label={showPassword
                            ? "Hide password"
                            : "Show password"}
                    >
                        {#if showPassword}
                            <i class="fi fi-rr-eye-crossed"></i>
                        {:else}
                            <i class="fi fi-rr-eye"></i>
                        {/if}
                    </button>
                </div>
            </div>
            <button type="submit" class="btn btn-primary login-btn"
                >Sign In</button
            >
        </form>

        <div class="login-footer">
            <a href="/">Back to Home</a>
        </div>
    </div>
</div>

<style>
    .login-container {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: linear-gradient(
            135deg,
            #0f1f15 0%,
            #1a3020 50%,
            #c8102e 100%
        );
        padding: 20px;
    }

    .login-card {
        width: 100%;
        max-width: 400px;
        padding: 40px;
        text-align: center;
        color: white;
    }

    .logo-container {
        background: white;
        padding: 12px;
        border-radius: 12px;
        width: 120px;
        margin: 0 auto 24px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .ntb-logo {
        width: 100%;
        height: auto;
    }

    h1 {
        font-size: 24px;
        margin-bottom: 8px;
        color: white;
    }

    p {
        font-size: 14px;
        color: rgba(255, 255, 255, 0.7);
        margin-bottom: 32px;
    }

    .form-group {
        text-align: left;
        margin-bottom: 20px;
    }

    label {
        display: block;
        font-size: 12px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 1px;
        margin-bottom: 8px;
        color: rgba(255, 255, 255, 0.8);
    }

    /* Wrapper for input + eye button */
    .input-wrapper {
        position: relative;
        display: flex;
        align-items: center;
    }
    .input-text {
        color: black;
    }

    input {
        width: 100%;
        padding: 12px 16px;
        border-radius: 8px;
        background: rgba(255, 255, 255, 0.1);
        border: 1px solid rgba(255, 255, 255, 0.2);
        color: white;
        font-size: 14px;
        outline: none;
        transition: all 0.2s;
        box-sizing: border-box;
    }

    /* Extra right padding so text doesn't go under the eye button */
    .input-wrapper input {
        padding-right: 44px;
    }

    input::placeholder {
        color: rgba(255, 255, 255, 0.4);
    }

    input:focus {
        background: rgba(249, 246, 246, 0.15);
        border-color: var(--color-accent);
        box-shadow: 0 0 0 2px rgba(212, 175, 55, 0.2);
    }

    .eye-btn {
        position: absolute;
        right: 12px;
        background: none;
        border: none;
        cursor: pointer;
        color: rgba(255, 255, 255, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 4px;
        border-radius: 4px;
        transition: color 0.2s;
        font-size: 16px;
        line-height: 1;
    }

    .eye-btn:hover {
        color: rgba(255, 255, 255, 0.9);
    }

    .login-btn {
        width: 100%;
        justify-content: center;
        margin-top: 12px;
        padding: 14px;
        font-size: 15px;
    }

    .login-footer {
        margin-top: 24px;
        font-size: 13px;
    }

    .form-alert {
        margin-bottom: 18px;
        padding: 12px 14px;
        border-radius: 10px;
        background: rgba(255, 255, 255, 0.12);
        border: 1px solid rgba(255, 255, 255, 0.22);
        color: rgba(255, 255, 255, 0.92);
        text-align: left;
        font-size: 13px;
    }

    .form-alert.error {
        background: rgba(200, 16, 46, 0.18);
        border-color: rgba(255, 255, 255, 0.16);
    }

    .login-footer a {
        color: rgba(255, 255, 255, 0.6);
        text-decoration: none;
        transition: color 0.2s;
    }

    .login-footer a:hover {
        color: white;
    }

    @media (max-width: 480px) {
        .login-card {
            padding: 24px 20px;
        }

        h1 {
            font-size: 20px;
        }
    }
</style>
