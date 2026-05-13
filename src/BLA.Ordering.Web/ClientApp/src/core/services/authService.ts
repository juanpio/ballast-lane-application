export interface AuthSession {
	isAuthenticated: boolean;
	userId: string;
	email: string;
	authenticationType: string | null;
}

interface LoginResponse {
	userId: number;
	email: string;
	accessToken: string;
	expiresAtUtc: string;
	tokenType: string;
}

export interface LoginResult {
	session: AuthSession;
	accessToken: string;
	expiresAtUtc: string;
	tokenType: string;
}

class AuthServiceError extends Error {
	readonly status: number;

	constructor(message: string, status: number) {
		super(message);
		this.name = 'AuthServiceError';
		this.status = status;
	}
}

async function readErrorMessage(response: Response, fallbackMessage: string) {
	try {
		const payload = (await response.json()) as { message?: string };
		return payload.message ?? fallbackMessage;
	} catch {
		return fallbackMessage;
	}
}

function buildSessionHeaders(accessToken?: string) {
	return accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined;
}

export async function getCurrentSession(accessToken?: string): Promise<AuthSession | null> {
	const response = await fetch('/api/auth/session', {
		credentials: 'include',
		headers: buildSessionHeaders(accessToken),
	});

	if (response.status === 401) {
		return null;
	}

	if (!response.ok) {
		throw new AuthServiceError(await readErrorMessage(response, 'Unable to validate the current session.'), response.status);
	}

	return (await response.json()) as AuthSession;
}

export async function login(email: string, password: string): Promise<LoginResult> {
	const response = await fetch('/api/auth/login', {
		method: 'POST',
		credentials: 'include',
		headers: {
			'Content-Type': 'application/json',
		},
		body: JSON.stringify({
			email: email.trim(),
			password,
		}),
	});

	if (!response.ok) {
		throw new AuthServiceError(await readErrorMessage(response, 'Invalid email or password.'), response.status);
	}

	const payload = (await response.json()) as LoginResponse;

	return {
		session: {
			isAuthenticated: true,
			userId: payload.userId.toString(),
			email: payload.email,
			authenticationType: payload.tokenType,
		},
		accessToken: payload.accessToken,
		expiresAtUtc: payload.expiresAtUtc,
		tokenType: payload.tokenType,
	};
}

export async function logout(accessToken?: string): Promise<void> {
	const response = await fetch('/api/auth/logout', {
		method: 'POST',
		credentials: 'include',
		headers: buildSessionHeaders(accessToken),
	});

	if (response.status === 401) {
		return;
	}

	if (!response.ok) {
		throw new AuthServiceError(await readErrorMessage(response, 'Unable to sign out.'), response.status);
	}
}

export { AuthServiceError };
