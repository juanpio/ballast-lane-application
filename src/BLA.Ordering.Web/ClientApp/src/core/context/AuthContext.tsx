import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import {
	AuthServiceError,
	getCurrentSession,
	login as loginRequest,
	logout as logoutRequest,
} from '../services/authService';

interface AuthContextValue {
	isAuthenticated: boolean;
	userName: string | null;
	isLoading: boolean;
	error: string | null;
	login: (email: string, password: string) => Promise<void>;
	logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
	children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
	const [isAuthenticated, setIsAuthenticated] = useState(false);
	const [userName, setUserName] = useState<string | null>(null);
	const [accessToken, setAccessToken] = useState<string | null>(null);
	const [isLoading, setIsLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);

	useEffect(() => {
		let isMounted = true;

		async function bootstrapSession() {
			setError(null);

			try {
				const session = await getCurrentSession();
				if (!isMounted || session === null) {
					return;
				}

				setIsAuthenticated(session.isAuthenticated);
				setUserName(session.email);
			} catch (sessionError) {
				if (!isMounted) {
					return;
				}

				setError(
					sessionError instanceof AuthServiceError
						? sessionError.message
						: 'Unable to validate the current session.',
				);
			} finally {
				if (isMounted) {
					setIsLoading(false);
				}
			}
		}

		void bootstrapSession();

		return () => {
			isMounted = false;
		};
	}, []);

	const login = async (email: string, password: string) => {
		setIsLoading(true);
		setError(null);

		try {
			if (!email.trim() || !password.trim()) {
				setError('Email and password are required');
				return;
			}

			const result = await loginRequest(email, password);
			const session = await getCurrentSession(result.accessToken);

			setAccessToken(result.accessToken);
			setIsAuthenticated(session?.isAuthenticated ?? result.session.isAuthenticated);
			setUserName(session?.email ?? result.session.email);
		} catch (loginError) {
			setAccessToken(null);
			setIsAuthenticated(false);
			setUserName(null);
			setError(
				loginError instanceof AuthServiceError
					? loginError.message
					: 'Unable to sign in right now.',
			);
		} finally {
			setIsLoading(false);
		}
	};

	const logout = async () => {
		setIsLoading(true);
		setError(null);

		try {
			await logoutRequest(accessToken ?? undefined);
		} catch (logoutError) {
			setError(
				logoutError instanceof AuthServiceError
					? logoutError.message
					: 'Unable to sign out right now.',
			);
		} finally {
			setAccessToken(null);
			setIsAuthenticated(false);
			setUserName(null);
			setIsLoading(false);
		}
	};

	const value = useMemo<AuthContextValue>(
		() => ({
			isAuthenticated,
			userName,
			isLoading,
			error,
			login,
			logout,
		}),
		[error, isAuthenticated, isLoading, userName],
	);

	return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
	const context = useContext(AuthContext);
	if (!context) {
		throw new Error('useAuth must be used within an AuthProvider');
	}
	return context;
}
