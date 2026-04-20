import { useState } from 'react';

interface LoginFormProps {
	isLoading?: boolean;
	error?: string | null;
	onSubmit?: (email: string, password: string) => Promise<void>;
}

export function LoginForm({ isLoading = false, error = null, onSubmit }: LoginFormProps) {
	const [email, setEmail] = useState('');
	const [password, setPassword] = useState('');

	return (
		<section className="auth-panel" aria-label="authentication">
			<h1>Sign in to Orders Dashboard</h1>
			<p>Authenticate to create, edit and delete customer orders.</p>

			{error ? (
				<div role="alert" className="auth-error">
					{error}
				</div>
			) : null}

			<form
				className="auth-form"
				onSubmit={async (event) => {
					event.preventDefault();
					await onSubmit?.(email, password);
				}}
			>
				<label htmlFor="email">Email</label>
				<input
					id="email"
					name="email"
					type="email"
					value={email}
					onChange={(event) => setEmail(event.target.value)}
					autoComplete="email"
					required
				/>

				<label htmlFor="password">Password</label>
				<input
					id="password"
					name="password"
					type="password"
					value={password}
					onChange={(event) => setPassword(event.target.value)}
					autoComplete="current-password"
					required
				/>

				<button type="submit" disabled={isLoading}>
					{isLoading ? 'Signing In...' : 'Sign In'}
				</button>
			</form>
		</section>
	);
}
